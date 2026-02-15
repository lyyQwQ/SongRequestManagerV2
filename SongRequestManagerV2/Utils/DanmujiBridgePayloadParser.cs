using SongRequestManagerV2.SimpleJsons;

namespace SongRequestManagerV2.Utils
{
    public enum DanmujiBridgeParseResult
    {
        NotBridgePayload = 0,
        Valid = 1,
        InvalidBridgePayload = 2
    }

    public readonly struct DanmujiBridgePayload
    {
        public DanmujiBridgePayload(string userId, string userName, string message, bool isAdmin, bool isFan, int guardLevel)
        {
            this.UserId = userId;
            this.UserName = userName;
            this.Message = message;
            this.IsAdmin = isAdmin;
            this.IsFan = isFan;
            this.GuardLevel = guardLevel;
        }

        public string UserId { get; }
        public string UserName { get; }
        public string Message { get; }
        public bool IsAdmin { get; }
        public bool IsFan { get; }
        public int GuardLevel { get; }
    }

    public static class DanmujiBridgePayloadParser
    {
        public static DanmujiBridgeParseResult TryParse(string raw, out DanmujiBridgePayload payload, out string reason)
        {
            payload = default;
            reason = "";

            if (string.IsNullOrWhiteSpace(raw)) {
                return DanmujiBridgeParseResult.NotBridgePayload;
            }

            var text = raw.Trim();
            if (!text.StartsWith("{")) {
                return DanmujiBridgeParseResult.NotBridgePayload;
            }

            JSONNode node;
            try {
                node = JSON.Parse(text);
            }
            catch {
                reason = "JSON_PARSE_ERROR";
                return DanmujiBridgeParseResult.InvalidBridgePayload;
            }

            if (node == null || !node.IsObject) {
                reason = "ROOT_NOT_OBJECT";
                return DanmujiBridgeParseResult.InvalidBridgePayload;
            }

            var obj = node.AsObject;
            if (obj == null) {
                reason = "ROOT_OBJECT_NULL";
                return DanmujiBridgeParseResult.InvalidBridgePayload;
            }

            var version = obj["v"].AsInt;
            if (version != 1) {
                reason = "UNSUPPORTED_VERSION";
                return DanmujiBridgeParseResult.InvalidBridgePayload;
            }

            var uid = obj["uid"].Value ?? "";
            var userName = obj["uname"].Value ?? "";
            var message = obj["msg"].Value ?? "";

            if (string.IsNullOrWhiteSpace(uid) || uid == "0") {
                uid = userName;
            }

            if (string.IsNullOrWhiteSpace(uid)) {
                reason = "MISSING_UID";
                return DanmujiBridgeParseResult.InvalidBridgePayload;
            }

            if (string.IsNullOrWhiteSpace(message)) {
                reason = "MISSING_MESSAGE";
                return DanmujiBridgeParseResult.InvalidBridgePayload;
            }

            if (string.IsNullOrWhiteSpace(userName)) {
                userName = uid;
            }

            var guardLevel = obj["guardLevel"].AsInt;
            if (guardLevel < 0) {
                guardLevel = 0;
            }

            payload = new DanmujiBridgePayload(uid, userName, message, obj["isAdmin"].AsBool, obj["isFan"].AsBool, guardLevel);
            return DanmujiBridgeParseResult.Valid;
        }
    }
}
