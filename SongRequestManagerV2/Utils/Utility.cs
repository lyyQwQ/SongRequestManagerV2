using ChatCore.Interfaces;
using ChatCore.Models.Twitch;
using ChatCore.Models.Bilibili;
using SongRequestManagerV2.Configuration;
using SongRequestManagerV2.Interfaces;
using SongRequestManagerV2.Models;
using SongRequestManagerV2.SimpleJSON;
using SongRequestManagerV2.Statics;
using System;
using System.IO;
using System.Linq;

namespace SongRequestManagerV2.Utils
{
    public static class Utility
    {
        public static void EmptyDirectory(string directory, bool delete = true)
        {
            if (Directory.Exists(directory)) {
                var directoryInfo = new DirectoryInfo(directory);
                foreach (var file in directoryInfo.GetFiles()) {
                    file.Delete();
                }

                foreach (var subDirectory in directoryInfo.GetDirectories()) {
                    subDirectory.Delete(true);
                }

                if (delete) {
                    Directory.Delete(directory);
                }
            }
        }

        public static TimeSpan GetFileAgeDifference(string filename)
        {
            var lastModified = File.GetLastWriteTime(filename);
            return DateTime.Now - lastModified;
        }

        public static bool HasRights(ISRMCommand botcmd, IChatUser user, CmdFlags flags)
        {
            if (flags.HasFlag(CmdFlags.Local)) {
                return true;
            }

            if (botcmd.Flags.HasFlag(CmdFlags.Disabled)) {
                return false;
            }

            if (botcmd.Flags.HasFlag(CmdFlags.Everyone)) {
                return true; // Not sure if this is the best approach actually, not worth thinking about right now
            }

            GetUserTierFlags(user, out var isSubscriber, out var isVip);
            return UserPrivilegeMapper.HasCommandTierRights(
                user.IsBroadcaster,
                user.IsModerator,
                isSubscriber,
                isVip,
                botcmd.Flags,
                RequestBotConfig.Instance.ModFullRights);
        }

        public static int GetRequestLimit(IChatUser user, int userLimit, int subLimit, int modLimit, int vipBonus)
        {
            GetUserTierFlags(user, out var isSubscriber, out var isVip);
            return UserPrivilegeMapper.ComputeRequestLimit(user.IsBroadcaster, user.IsModerator, isSubscriber, isVip, userLimit, subLimit, modLimit, vipBonus);
        }

        private static void GetUserTierFlags(IChatUser user, out bool isSubscriber, out bool isVip)
        {
            isSubscriber = false;
            isVip = false;

            if (user is TwitchUser twitchUser) {
                isSubscriber = twitchUser.IsSubscriber;
                isVip = twitchUser.IsVip;
                return;
            }

            if (user is BilibiliChatUser biliBiliChatUser) {
                isSubscriber = biliBiliChatUser.IsFan;
                isVip = biliBiliChatUser.GuardLevel > 0;
                return;
            }

            if (user is InjectedBilibiliUser injectedBilibiliUser) {
                isSubscriber = injectedBilibiliUser.IsFan;
                isVip = injectedBilibiliUser.GuardLevel > 0;
            }
        }

        public static string GetStarRating(JSONObject song, bool mode = true)
        {
            if (!mode) {
                return "";
            }

            var version = song["versions"].AsArray.Children.FirstOrDefault(x => x["state"].Value == MapStatus.Published.ToString());
            if (version == null) {
                return "";
            }
            var maxstar = 0f;
            foreach (var diff in version["diffs"].AsArray.Children) {
                if (maxstar < diff["stars"].AsFloat) {
                    maxstar = diff["stars"].AsFloat;
                }
            }
            var stars = "******";
            var rating = maxstar;
            if (rating < 0 || rating > 100) {
                rating = 0;
            }

            var starrating = stars.Substring(0, (int)(rating / 17)); // 17 is used to produce a 5 star rating from 80ish to 100.
            return starrating;
        }

        public static string GetRating(JSONObject song, bool mode = true)
        {
            if (!mode) {
                return "";
            }

            var rating = song["stats"]["score"].AsFloat * 100f;
            if (rating == 0) {
                return "";
            }

            return $"{rating:0.0}%";
        }

        public static bool IsAprilFool()
        {
#if DEBUG
            return RequestBotConfig.Instance.EnableAprilFool;
#else
            return RequestBotConfig.Instance.EnableAprilFool && DateTime.Now.Month == 4 && DateTime.Now.Day == 1;
#endif
        }
    }
}
