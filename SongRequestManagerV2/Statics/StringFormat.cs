using System.Text;

namespace SongRequestManagerV2.Statics
{
    public class StringFormat
    {
        public static StringBuilder AddSongToQueueText { get; } = new StringBuilder("歌曲 %songName% %songSubName%/%levelAuthorName% %Rating% (%key%) 已添加到队列");
        public static StringBuilder LookupSongDetail { get; } = new StringBuilder("%songName% %songSubName%/%levelAuthorName% %Rating% (%key%)");
        public static StringBuilder BsrSongDetail { get; } = new StringBuilder("%songName% %songSubName%/%levelAuthorName% %Rating% (%key%)");
        public static StringBuilder LinkSonglink { get; } = new StringBuilder("%songName% %songSubName%/%levelAuthorName% %Rating% (%key%) %BeatsaverLink%");
        public static StringBuilder NextSonglink { get; } = new StringBuilder("下一首是 %user% 点的 %songName% %songSubName%/%levelAuthorName% %Rating% (%key%)");
        public static StringBuilder SongHintText { get; } = new StringBuilder("点歌人:%user%%LF%状态: %Status%%Info%%LF%%LF%<size=60%>点歌时间: %RequestTime%</size>");
        public static StringBuilder QueueTextFileFormat { get; } = new StringBuilder("%songName%%LF%");         // Don't forget to include %LF% for these. 
        public static StringBuilder QueueListRow2 { get; } = new StringBuilder("%levelAuthorName% (%key%) <color=white>%songlength%</color>");
        public static StringBuilder BanSongDetail { get; } = new StringBuilder("屏蔽 %songName%/%levelAuthorName% (%key%)");
        public static StringBuilder QueueListFormat { get; } = new StringBuilder("%songName% (%key%)");
        public static StringBuilder HistoryListFormat { get; } = new StringBuilder("%songName% (%key%)");
        public static StringBuilder AddSortOrder { get; } = new StringBuilder("-rating +id");
        public static StringBuilder LookupSortOrder { get; } = new StringBuilder("-rating +id");
        public static StringBuilder AddSongsSortOrder { get; } = new StringBuilder("-rating +id");
    }
}