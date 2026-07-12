using System;
using System.Linq;
using System.Threading;
using SongRequestManagerV2.Extentions;
using UnityEngine;

#if BS_1423
using OculusStudios.Platform.Core;
#endif

namespace SongRequestManagerV2.Compatibility
{
    internal sealed class PlatformUserSnapshot
    {
        public PlatformUserSnapshot(string platformUserId, string userName, string displayName)
        {
            this.PlatformUserId = platformUserId ?? string.Empty;
            this.UserName = userName ?? string.Empty;
            this.DisplayName = displayName ?? string.Empty;
        }

        public string PlatformUserId { get; }

        public string UserName { get; }

        public string DisplayName { get; }
    }

    internal static class PlatformUserCompat
    {
#if BS_1423
        public static PlatformUserSnapshot TryGetCurrentUser()
        {
            try {
                var platform = Resources.FindObjectsOfTypeAll<PlatformLeaderboardsModel>()
                    .LastOrDefault(model => model != null && model._platform != null)
                    ?._platform;
                var user = platform?.user;
                if (user == null || user.userId == 0) {
                    return null;
                }

                var displayName = string.IsNullOrWhiteSpace(user.displayName) ? user.userId.ToString() : user.displayName;
                return new PlatformUserSnapshot(user.userId.ToString(), displayName, displayName);
            }
            catch (Exception ex) {
                Logger.Error(ex);
                return null;
            }
        }
#else
        public static void InitializeCurrentUser(IPlatformUserModel platformUserModel, Action<PlatformUserSnapshot> setCurrentUser)
        {
            if (platformUserModel == null) {
                return;
            }

            platformUserModel.GetUserInfo(CancellationToken.None).Await(result =>
            {
                if (result == null) {
                    return;
                }

                var displayName = string.IsNullOrWhiteSpace(result.userName) ? result.platformUserId : result.userName;
                setCurrentUser?.Invoke(new PlatformUserSnapshot(result.platformUserId, displayName, displayName));
            });
        }
#endif
    }
}
