using System;

namespace SongRequestManagerV2.Networks
{
    public class DownloadProgressInfo
    {
        public string Url { get; set; }
        public double Progress { get; set; } // 0..1
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; } // -1 if unknown
        public double BytesPerSecond { get; set; }
        public double ElapsedSeconds { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

