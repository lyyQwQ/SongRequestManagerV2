using ChatCore.Interfaces;
using ChatCore.Utilities;
using System;

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
        public IChatBadge[] Badges { get; set; } = Array.Empty<IChatBadge>();

        public bool IsFan { get; set; }
        public int GuardLevel { get; set; }

        ChatCore.Utilities.JSONObject IChatUser.ToJson()
        {
            var obj = new JSONObject();
            obj.Add(nameof(this.Id), new JSONString(this.Id ?? ""));
            obj.Add(nameof(this.UserName), new JSONString(this.UserName ?? ""));
            obj.Add(nameof(this.DisplayName), new JSONString(this.DisplayName ?? ""));
            obj.Add(nameof(this.Color), new JSONString(this.Color ?? ""));
            obj.Add(nameof(this.IsBroadcaster), new JSONBool(this.IsBroadcaster));
            obj.Add(nameof(this.IsModerator), new JSONBool(this.IsModerator));
            var badges = new JSONArray();
            if (this.Badges != null) {
                foreach (var badge in this.Badges) {
                    if (badge != null) {
                        badges.Add(badge.ToJson());
                    }
                }
            }
            obj.Add(nameof(this.Badges), badges);
            obj.Add(nameof(this.IsFan), new JSONBool(this.IsFan));
            obj.Add(nameof(this.GuardLevel), new JSONNumber(this.GuardLevel));
            obj.Add("UserType", new JSONString(nameof(InjectedBilibiliUser)));
            return obj;
        }
    }
}
