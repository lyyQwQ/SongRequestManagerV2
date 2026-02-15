using CatCore.Models.Shared;

namespace SongRequestManagerV2.Models
{
    public sealed class InjectedBilibiliUser : IChatUser
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Color { get; set; } = "#FFFFFFFF";
        public bool IsBroadcaster { get; set; }
        public bool IsModerator { get; set; }

        public bool IsFan { get; set; }
        public int GuardLevel { get; set; }
    }
}
