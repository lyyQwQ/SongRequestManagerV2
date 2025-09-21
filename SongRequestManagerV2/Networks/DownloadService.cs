using SongRequestManagerV2; // Logger, Plugin, Dispatcher
using SongRequestManagerV2.Configuration;
using SongRequestManagerV2.Statics;
using System;
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

        private SemaphoreSlim _zipSemaphore = new SemaphoreSlim(2, 2);
        private SemaphoreSlim _imageSemaphore = new SemaphoreSlim(4, 4);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<WebResponse> SendAsyncUnity(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress = null, bool retry = true)
        {
            // 选择信号量：默认按 ZIP 通道限制
            var gate = _zipSemaphore;
            await gate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                return await SendUnityRequestInternal(methodType, url, token, progress, retry).ConfigureAwait(false);
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task<byte[]> DownloadImage(string url, CancellationToken token, IProgress<double> progress = null)
        {
            await _imageSemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var resp = await SendUnityRequestInternal(HttpMethod.Get, url, token, progress, retry: true).ConfigureAwait(false);
                if (resp?.IsSuccessStatusCode == true)
                {
                    return resp.ContentToBytes();
                }
                return null;
            }
            finally
            {
                _imageSemaphore.Release();
            }
        }

        public async Task<byte[]> DownloadZip(string url, CancellationToken token, IProgress<double> progress = null)
        {
            await _zipSemaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                var resp = await SendUnityRequestInternal(HttpMethod.Get, url, token, progress, retry: true).ConfigureAwait(false);
                if (resp?.IsSuccessStatusCode == true)
                {
                    return resp.ContentToBytes();
                }
                return null;
            }
            finally
            {
                _zipSemaphore.Release();
            }
        }

        private async Task<WebResponse> SendUnityRequestInternal(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress, bool retry)
        {
            var maxRetries = retry ? 5 : 1;
            var retryDelay = 1000;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    var tcs = new TaskCompletionSource<UnityWebRequest>();
                    // 主线程发起协程
                    Dispatcher.RunOnMainThread(() =>
                    {
                        var uwr = new UnityWebRequest(url, methodType.Method);
                        uwr.downloadHandler = new DownloadHandlerBuffer();
                        uwr.timeout = 300; // 5 分钟
                        uwr.SetRequestHeader("User-Agent", $"SongRequestManagerV2/{Plugin.Version}");
                        // 特定中转服务器：仅添加 API Key，不做速度/切换策略
                        if (RequestBotConfig.Instance.BeatsaverServer == BeatsaverServer.EstrellaTest)
                        {
                            uwr.SetRequestHeader("X-API-Key", "song-request-manager-estrella-20241006");
                            Logger.Info("使用测试中转服务器");
                        }
                        StartCoroutine(RunRequest(uwr, tcs, progress, token));
                    });

                    using (token.Register(() => tcs.TrySetCanceled()))
                    {
                        var completedUwr = await tcs.Task.ConfigureAwait(false);
                        var response = new WebResponse(completedUwr, completedUwr.downloadHandler?.data);
                        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return response;
                        }
                        Logger.Debug($"Response code : {response.StatusCode}, url: {url}, retry: {retryCount + 1}/{maxRetries}");
                    }
                }
                catch (TaskCanceledException)
                {
                    Logger.Error("下载已取消。");
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Error($"下载失败，重试（{retryCount + 1}/{maxRetries}）：{ex.Message}");
                }

                if (retryCount < maxRetries - 1)
                {
                    try { await Task.Delay(retryDelay, token).ConfigureAwait(false); } catch { }
                    retryDelay *= 2;
                }
            }
            return null;
        }

        private System.Collections.IEnumerator RunRequest(UnityWebRequest uwr, TaskCompletionSource<UnityWebRequest> tcs, IProgress<double> progress, CancellationToken token)
        {
            var operation = uwr.SendWebRequest();
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            while (!operation.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    uwr.Abort();
                    tcs.TrySetCanceled();
                    yield break;
                }

                progress?.Report(uwr.downloadProgress);
                yield return null;
            }

            stopwatch.Stop();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                tcs.TrySetResult(uwr); // 让上层根据状态码/结果判断
            }
            else
            {
                progress?.Report(1.0);
                tcs.TrySetResult(uwr);
            }
        }
    }
}
