using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SongRequestManagerV2.Networks
{
    public interface IDownloadService
    {
        Task<SongRequestManagerV2.WebResponse> SendAsyncUnity(HttpMethod methodType, string url, CancellationToken token, IProgress<double> progress = null, bool retry = true);

        Task<byte[]> DownloadImage(string url, CancellationToken token, IProgress<double> progress = null);

        Task<byte[]> DownloadZip(string url, CancellationToken token, IProgress<double> progress = null);
    }
}

