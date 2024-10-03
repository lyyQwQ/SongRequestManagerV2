using SongRequestManagerV2.SimpleJSON;
using SongRequestManagerV2.Utils;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SongRequestManagerV2
{
    internal class WebResponse
    {
        public readonly HttpStatusCode StatusCode;
        public readonly string ReasonPhrase;
        public readonly HttpResponseHeaders Headers;
        public readonly HttpRequestMessage RequestMessage;
        public readonly bool IsSuccessStatusCode;

        private readonly byte[] _content;

        internal WebResponse(HttpResponseMessage resp, byte[] body)
        {
            this.StatusCode = resp.StatusCode;
            this.ReasonPhrase = resp.ReasonPhrase;
            this.Headers = resp.Headers;
            this.RequestMessage = resp.RequestMessage;
            this.IsSuccessStatusCode = resp.IsSuccessStatusCode;

            this._content = body;
        }

        // 新增构造函数：处理 UnityWebRequest
        internal WebResponse(UnityWebRequest uwr, byte[] body)
        {
            // UnityWebRequest 没有与 HttpStatusCode 完全等价的属性，
            // 我们可以根据 uwr.responseCode 将其映射到 HttpStatusCode
            this.StatusCode = (HttpStatusCode)uwr.responseCode;
            this.ReasonPhrase = uwr.error;  // UnityWebRequest 使用 error 表示错误信息
            this.IsSuccessStatusCode = uwr.result == UnityWebRequest.Result.Success;

            this._content = body;
        }

        public byte[] ContentToBytes()
        {
            return this._content;
        }

        public string ContentToString()
        {
            return Encoding.UTF8.GetString(this._content);
        }

        public JSONNode ConvertToJsonNode()
        {
            return JSONNode.Parse(this.ContentToString());
        }
    }

    internal static class WebClient
    {
        private static HttpClient _client;
        private static HttpClientHandler _handler;

        private static HttpClient Client
        {
            get
            {
                if (_client == null) {
                    Connect();
                }

                return _client;
            }
        }

        private static readonly int RETRY_COUNT = 5;

        private static void Connect()
        {
            try {
                _client?.Dispose();
            }
            catch (Exception e) {
                Logger.Error(e);
            }

            _handler = new HttpClientHandler
            {
                UseCookies = false, // 不使用 Cookie
                UseProxy = true, // 确保使用代理
                Proxy = WebRequest.DefaultWebProxy, // 使用系统默认代理
            };

            _client = new HttpClient(_handler) { Timeout = new TimeSpan(0, 0, 15) };
            _client.DefaultRequestHeaders.UserAgent.TryParseAdd($"SongRequestManagerV2/{Plugin.Version}");
            _client.DefaultRequestHeaders.Add("Cookie", "aprilFools=1;");
        }

        internal static async Task<WebResponse> GetAsync(string url, CancellationToken token)
        {
            try {
                return await SendAsyncUnity(HttpMethod.Get, url, token);
            }
            catch (Exception e) {
                Logger.Error(e);
                return null;
            }
        }

        internal static async Task<byte[]> DownloadImage(string url, CancellationToken token)
        {
            try {
                var response = await SendAsyncUnity(HttpMethod.Get, url, token);
                if (response?.IsSuccessStatusCode == true) {
                    return response.ContentToBytes();
                }

                return null;
            }
            catch (Exception e) {
                // Logger.Error(e);
                return null;
            }
        }

        /// <summary>
        /// たぶんもう使わない。
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="token"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        internal static async Task<byte[]> DownloadSong(string hash, CancellationToken token,
            IProgress<double> progress = null)
        {
            // check if beatsaver url needs to be pre-pended
            if (!hash.StartsWith(@"https://cdn.beatsaver.com/")) {
                hash = $"https://cdn.beatsaver.com/{hash}.zip";
            }

            try {
                var response = await SendAsyncUnity(HttpMethod.Get, hash, token, progress: progress);

                if (response?.IsSuccessStatusCode == true) {
                    return response.ContentToBytes();
                }

                return null;
            }
            catch (Exception e) {
                // Logger.Error(e);
                return null;
            }
        }

        internal static async Task<WebResponse> SendAsync(HttpMethod methodType, string url, CancellationToken token,
            IProgress<double> progress = null)
        {
            // send request
            try {
                HttpResponseMessage resp = null;
                var retryCount = 0;
                do {
                    try {
                        // create new request messsage
#if DEBUG
                        Logger.Debug(url);
#endif
                        var req = new HttpRequestMessage(methodType, url);
                        if (retryCount != 0) {
                            await Task.Delay(1000);
                        }

                        retryCount++;
                        resp = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token)
                            .ConfigureAwait(false);
                        if (resp.StatusCode != HttpStatusCode.OK) {
                            Logger.Debug(
                                $"Response code : {resp.StatusCode}, url: {url}, retry: {retryCount}/{RETRY_COUNT}");
                        }
                    }
                    catch (Exception e) {
                        if (resp == null) {
                            Logger.Error($"Response Timeout, url: {url}, retry: {retryCount}/{RETRY_COUNT}");
                        }
                        else {
                            Logger.Error(
                                $"Response code : {resp?.StatusCode}, url: {url}, retry: {retryCount}/{RETRY_COUNT}");
                        }

                        Logger.Error(e);
                    }
                } while (resp?.StatusCode != HttpStatusCode.NotFound && resp?.IsSuccessStatusCode != true &&
                         retryCount <= RETRY_COUNT);


                if (token.IsCancellationRequested) {
                    throw new TaskCanceledException();
                }

                using (var memoryStream = new MemoryStream())
                using (var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                    var buffer = new byte[65536];
                    var bytesRead = 0;

                    var contentLength = resp?.Content.Headers.ContentLength;
                    var totalRead = 0;

                    // send report
                    progress?.Report(0);

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0) {
                        if (token.IsCancellationRequested) {
                            throw new TaskCanceledException();
                        }

                        if (contentLength != null) {
                            progress?.Report(totalRead / (double)contentLength);
                        }

                        await memoryStream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                        totalRead += bytesRead;
                    }

                    progress?.Report(1);
                    var bytes = memoryStream.ToArray();

                    return new WebResponse(resp, bytes);
                }
            }
            catch (Exception e) {
                Logger.Error(e);
                throw;
            }
        }


        /// <summary>
        /// 使用 UnityWebRequest 发送异步请求，类似于 SendAsync
        /// </summary>
        /// <param name="methodType">HTTP 方法</param>
        /// <param name="url">请求的 URL</param>
        /// <param name="token">取消令牌</param>
        /// <param name="progress">进度回报</param>
        /// <returns>WebResponse 对象</returns>
        // 修改 SendAsyncUnity 方法
        internal static async Task<WebResponse> SendAsyncUnity(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress = null)
        {
            using (UnityWebRequest uwr = new UnityWebRequest(url, methodType.Method))
            {
                // 设置请求头
                uwr.SetRequestHeader("User-Agent", $"SongRequestManagerV2/{Plugin.Version}");

                // 根据方法类型设置下载处理器
                if (methodType == HttpMethod.Get)
                {
                    uwr.downloadHandler = new DownloadHandlerBuffer();
                }
                else
                {
                    // 根据需要选择合适的 DownloadHandler
                    uwr.downloadHandler = new DownloadHandlerBuffer();
                }

                // 创建 TaskCompletionSource
                var tcs = new TaskCompletionSource<UnityWebRequest>();

                // 启动协程并等待完成
                CoroutineRunner.Instance.StartCoroutine(RunRequest(uwr, tcs, progress, token));

                // 等待请求完成或取消
                try
                {
                    using (token.Register(() => uwr.Abort()))
                    {
                        var completedUwr = await tcs.Task.ConfigureAwait(false);

                        // 创建 WebResponse 对象
                        var response = new WebResponse(completedUwr, completedUwr.downloadHandler.data);
                        return response;
                    }
                }
                catch (TaskCanceledException)
                {
                    Logger.Error("Download canceled.");
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Download failed: {ex.Message}");
                    return null;
                }
            }
        }


        // 修改 RunRequest 方法
        private static IEnumerator RunRequest(UnityWebRequest uwr, TaskCompletionSource<UnityWebRequest> tcs, IProgress<double> progress, CancellationToken token)
        {
            var request = uwr.SendWebRequest();

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            while (!request.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    uwr.Abort();
                    tcs.TrySetCanceled();
                    yield break;
                }

                // 超时控制（例如 15 秒）
                if (stopwatch.ElapsedMilliseconds > 15000)
                {
                    uwr.Abort();
                    tcs.TrySetException(new TimeoutException("Request timed out."));
                    yield break;
                }

                // 报告进度
                progress?.Report(request.progress);

                yield return null;
            }

            stopwatch.Stop();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                tcs.TrySetException(new Exception(uwr.error));
            }
            else
            {
                progress?.Report(1.0);
                tcs.TrySetResult(uwr);
            }
        }

    }
}