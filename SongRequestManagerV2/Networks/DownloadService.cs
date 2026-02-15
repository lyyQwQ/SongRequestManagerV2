using SongRequestManagerV2.Configuration;
using SongRequestManagerV2.Statics;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SongRequestManagerV2.Networks
{
    public class DownloadService : MonoBehaviour, IDownloadService
    {
        public static DownloadService Instance { get; private set; }

        private readonly SemaphoreSlim _zipSemaphore = new SemaphoreSlim(2, 2);
        private readonly SemaphoreSlim _imageSemaphore = new SemaphoreSlim(4, 4);
        private readonly ConcurrentDictionary<string, Lazy<Task<WebResponse>>> _inFlightRequests = new ConcurrentDictionary<string, Lazy<Task<WebResponse>>>();
        private readonly ConcurrentDictionary<string, Lazy<Task<byte[]>>> _inFlightImages = new ConcurrentDictionary<string, Lazy<Task<byte[]>>>();
        private readonly ConcurrentDictionary<string, Lazy<Task<string>>> _inFlightZipFiles = new ConcurrentDictionary<string, Lazy<Task<string>>>();

        private static readonly string s_zipDownloadCachePath = Path.Combine(Environment.CurrentDirectory, ".requestcache");

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<WebResponse> SendAsyncUnity(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress = null, bool retry = true)
        {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }

            var requestKey = $"{methodType.Method}:{retry}:{url}";
            return await this.RunWithInFlightDedup(
                this._inFlightRequests,
                requestKey,
                () => this.SendUnityRequestWithGate(methodType, url, token, progress, retry, this._zipSemaphore),
                token).ConfigureAwait(false);
        }

        public async Task<byte[]> DownloadImage(string url, CancellationToken token, IProgress<double> progress = null)
        {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }

            return await this.RunWithInFlightDedup(
                this._inFlightImages,
                url,
                () => this.DownloadImageCore(url, token, progress),
                token).ConfigureAwait(false);
        }

        public async Task<string> DownloadZip(string url, CancellationToken token, IProgress<double> progress = null, Action<DownloadProgressInfo> advanced = null)
        {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }

            return await this.RunWithInFlightDedup(
                this._inFlightZipFiles,
                url,
                () => this.DownloadZipCore(url, token, progress, advanced),
                token).ConfigureAwait(false);
        }

        private async Task<byte[]> DownloadImageCore(string url, CancellationToken token, IProgress<double> progress)
        {
            var resp = await this.SendUnityRequestWithGate(HttpMethod.Get, url, token, progress, retry: true, this._imageSemaphore).ConfigureAwait(false);
            return resp?.IsSuccessStatusCode == true ? resp.ContentToBytes() : null;
        }

        private async Task<string> DownloadZipCore(string url, CancellationToken token, IProgress<double> progress, Action<DownloadProgressInfo> advanced)
        {
            var zipFilePath = this.CreateTemporaryZipPath();
            var resp = await this.SendUnityRequestWithGate(HttpMethod.Get, url, token, progress, retry: true, this._zipSemaphore, advanced, zipFilePath).ConfigureAwait(false);
            if (resp?.IsSuccessStatusCode == true) {
                return zipFilePath;
            }

            this.TryDeleteFile(zipFilePath);
            return null;
        }

        private async Task<WebResponse> SendUnityRequestWithGate(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress, bool retry, SemaphoreSlim gate, Action<DownloadProgressInfo> advanced = null, string downloadFilePath = null)
        {
            await gate.WaitAsync(token).ConfigureAwait(false);
            try {
                return await this.SendUnityRequestInternal(methodType, url, token, progress, retry, advanced, downloadFilePath).ConfigureAwait(false);
            }
            finally {
                gate.Release();
            }
        }

        private async Task<T> RunWithInFlightDedup<T>(ConcurrentDictionary<string, Lazy<Task<T>>> inFlight, string key, Func<Task<T>> factory, CancellationToken token)
        {
            var created = new Lazy<Task<T>>(factory, LazyThreadSafetyMode.ExecutionAndPublication);
            var lazy = inFlight.GetOrAdd(key, created);
            var isOwner = ReferenceEquals(created, lazy);

            Task<T> task;
            try {
                task = lazy.Value;
            }
            catch {
                inFlight.TryRemove(key, out Lazy<Task<T>> _);
                throw;
            }

            if (isOwner) {
                _ = task.ContinueWith(_ =>
                {
                    inFlight.TryRemove(key, out Lazy<Task<T>> _);
                }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            try {
                return await WaitWithCancellation(task, token).ConfigureAwait(false);
            }
            catch (TaskCanceledException) {
                Logger.Debug($"下载等待被取消，url={key}");
                return default;
            }
        }

        private static async Task<T> WaitWithCancellation<T>(Task<T> task, CancellationToken token)
        {
            if (!token.CanBeCanceled || task.IsCompleted) {
                return await task.ConfigureAwait(false);
            }

            var cancelledTask = Task.Delay(Timeout.Infinite, token);
            var completed = await Task.WhenAny(task, cancelledTask).ConfigureAwait(false);
            if (!ReferenceEquals(completed, task)) {
                throw new TaskCanceledException();
            }

            return await task.ConfigureAwait(false);
        }

        private string CreateTemporaryZipPath()
        {
            Directory.CreateDirectory(s_zipDownloadCachePath);
            return Path.Combine(s_zipDownloadCachePath, $"{Guid.NewGuid():N}.zip");
        }

        private void TryDeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) {
                return;
            }

            try {
                if (File.Exists(filePath)) {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex) {
                Logger.Debug($"删除临时 ZIP 失败: {filePath}, {ex.Message}");
            }
        }

        private async Task<WebResponse> SendUnityRequestInternal(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress, bool retry, Action<DownloadProgressInfo> advanced = null, string downloadFilePath = null)
        {
            var maxRetries = retry ? 5 : 1;
            var retryDelay = 1000;

            for (var retryCount = 0; retryCount < maxRetries; retryCount++) {
                try {
                    var tcs = new TaskCompletionSource<UnityWebRequest>();
                    Dispatcher.RunOnMainThread(() =>
                    {
                        var uwr = new UnityWebRequest(url, methodType.Method) {
                            timeout = 300
                        };

                        if (string.IsNullOrEmpty(downloadFilePath)) {
                            uwr.downloadHandler = new DownloadHandlerBuffer();
                        }
                        else {
                            uwr.downloadHandler = new DownloadHandlerFile(downloadFilePath);
                        }

                        uwr.SetRequestHeader("User-Agent", $"SongRequestManagerV2/{Plugin.Version}");
                        if (RequestBotConfig.Instance.BeatsaverServer == BeatsaverServer.EstrellaTest) {
                            uwr.SetRequestHeader("X-API-Key", "song-request-manager-estrella-20241006");
                        }

                        StartCoroutine(this.RunRequest(uwr, tcs, progress, token, advanced));
                    });

                    using (token.Register(() => tcs.TrySetCanceled())) {
                        var completedUwr = await tcs.Task.ConfigureAwait(false);
                        var body = string.IsNullOrEmpty(downloadFilePath) ? completedUwr.downloadHandler?.data : null;
                        var response = new WebResponse(completedUwr, body);
                        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound) {
                            return response;
                        }

                        Logger.Debug($"Response code : {response.StatusCode}, url: {url}, retry: {retryCount + 1}/{maxRetries}");
                    }
                }
                catch (TaskCanceledException) {
                    Logger.Error("下载已取消。");
                    return null;
                }
                catch (Exception ex) {
                    Logger.Error($"下载失败，重试（{retryCount + 1}/{maxRetries}）：{ex.Message}");
                }

                if (retryCount < maxRetries - 1) {
                    try {
                        await Task.Delay(retryDelay, token).ConfigureAwait(false);
                    }
                    catch {
                    }

                    retryDelay *= 2;
                }
            }

            return null;
        }

        private System.Collections.IEnumerator RunRequest(UnityWebRequest uwr, TaskCompletionSource<UnityWebRequest> tcs, IProgress<double> progress, CancellationToken token, Action<DownloadProgressInfo> advanced)
        {
            var operation = uwr.SendWebRequest();
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var lastReport = 0.0;

            while (!operation.isDone) {
                if (token.IsCancellationRequested) {
                    uwr.Abort();
                    tcs.TrySetCanceled();
                    yield break;
                }

                progress?.Report(uwr.downloadProgress);
                if (advanced != null) {
                    var elapsed = stopwatch.Elapsed.TotalSeconds;
                    if (elapsed - lastReport >= 0.25) {
                        lastReport = elapsed;
                        long downloaded = (long)uwr.downloadedBytes;
                        long total = -1;
                        try {
                            var len = uwr.GetResponseHeader("Content-Length");
                            if (!string.IsNullOrEmpty(len) && long.TryParse(len, out var parsed)) {
                                total = parsed;
                            }
                        }
                        catch {
                        }

                        var bps = elapsed > 0 ? downloaded / elapsed : 0;
                        var info = new DownloadProgressInfo {
                            Url = uwr.url,
                            Progress = uwr.downloadProgress,
                            BytesDownloaded = downloaded,
                            TotalBytes = total,
                            BytesPerSecond = bps,
                            ElapsedSeconds = elapsed,
                            Timestamp = DateTime.Now
                        };
                        try {
                            advanced.Invoke(info);
                        }
                        catch {
                        }
                    }
                }

                yield return null;
            }

            stopwatch.Stop();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError) {
                tcs.TrySetResult(uwr);
            }
            else {
                progress?.Report(1.0);
                if (advanced != null) {
                    long downloaded = (long)uwr.downloadedBytes;
                    long total = -1;
                    try {
                        var len = uwr.GetResponseHeader("Content-Length");
                        if (!string.IsNullOrEmpty(len) && long.TryParse(len, out var parsed)) {
                            total = parsed;
                        }
                    }
                    catch {
                    }

                    var elapsed = stopwatch.Elapsed.TotalSeconds;
                    var bps = elapsed > 0 ? downloaded / elapsed : 0;
                    var info = new DownloadProgressInfo {
                        Url = uwr.url,
                        Progress = 1.0,
                        BytesDownloaded = downloaded,
                        TotalBytes = total,
                        BytesPerSecond = bps,
                        ElapsedSeconds = elapsed,
                        Timestamp = DateTime.Now
                    };
                    try {
                        advanced.Invoke(info);
                    }
                    catch {
                    }
                }

                tcs.TrySetResult(uwr);
            }
        }
    }
}
