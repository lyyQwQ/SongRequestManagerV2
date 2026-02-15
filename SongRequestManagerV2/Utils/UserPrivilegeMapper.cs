using SongRequestManagerV2.Statics;

namespace SongRequestManagerV2.Utils
{
    public static class UserPrivilegeMapper
    {
        public static bool HasCommandTierRights(bool isBroadcaster, bool isModerator, bool isSubscriber, bool isVip, CmdFlags commandFlags, bool modFullRights)
        {
            if (isModerator && modFullRights) {
                return true;
            }

            if (isBroadcaster && commandFlags.HasFlag(CmdFlags.Broadcaster)) {
                return true;
            }

            if (isModerator && commandFlags.HasFlag(CmdFlags.Mod)) {
                return true;
            }

            if (isSubscriber && commandFlags.HasFlag(CmdFlags.Sub)) {
                return true;
            }

            if (isVip && commandFlags.HasFlag(CmdFlags.VIP)) {
                return true;
            }

            return false;
        }

        public static int ComputeRequestLimit(bool isBroadcaster, bool isModerator, bool isSubscriber, bool isVip, int userLimit, int subLimit, int modLimit, int vipBonus)
        {
            if (isBroadcaster) {
                return int.MaxValue;
            }

            var limit = userLimit;
            if (isSubscriber) {
                limit = System.Math.Max(limit, subLimit);
            }

            if (isModerator) {
                limit = System.Math.Max(limit, modLimit);
            }

            if (isVip) {
                limit += vipBonus;
            }

            return limit;
        }
    }
}
