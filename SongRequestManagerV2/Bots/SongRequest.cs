using BeatSaberMarkupLanguage.Attributes;
using CatCore.Models.Shared;
using CatCore.Models.Twitch.IRC;
using CatCore.Models.Twitch.Media;
using HMUI;
using Newtonsoft.Json;
using SongCore;
using SongRequestManagerV2.Bases;
using SongRequestManagerV2.Configuration;
using SongRequestManagerV2.Networks;
using SongRequestManagerV2.Models;
using SongRequestManagerV2.SimpleJsons;
using SongRequestManagerV2.Statics;
using SongRequestManagerV2.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace SongRequestManagerV2.Bots
{
    public class SongRequest : BindableBase
    {
        [UIComponent("coverImage")]
        public ImageView _coverImage;

        [UIComponent("songNameText")]
        public TextMeshProUGUI _songNameText;

        [UIComponent("authorNameText")]
        public TextMeshProUGUI _authorNameText;

        [Inject]
        private readonly DynamicText.DynamicTextFactory _textFactory;
        [Inject]
        private readonly MapDatabase _mapDatabase;

        /// <summary>説明 を取得、設定</summary>
        private string hint_;
        /// <summary>説明 を取得、設定</summary>
        [UIValue("hover-hint")]
        public string Hint
        {
            get => this.hint_ ?? "";

            set => this.SetProperty(ref this.hint_, value);
        }

        /// <summary>説明 を取得、設定</summary>
        private string songName_;
        /// <summary>説明 を取得、設定</summary>
        [UIValue("song-name")]
        public string SongName
        {
            get => this.songName_ ?? "";

            set => this.SetProperty(ref this.songName_, value);
        }

        /// <summary>説明 を取得、設定</summary>
        private string authorName_;
        /// <summary>説明 を取得、設定</summary>
        [UIValue("author-name")]
        public string AuthorName
        {
            get => this.authorName_ ?? "";

            set => this.SetProperty(ref this.authorName_, value);
        }
        /// <summary>
        /// beatsaver json object<br />
        /// https://api.beatsaver.com/docs/index.html?url=./swagger.json
        /// </summary>
        public JSONObject SongNode { get; private set; }
        public JSONObject SongMetaData => this.SongNode["metadata"].AsObject;

        public JSONObject SongVersion { get; private set; }
        public bool IsWIP { get; private set; }
        public IChatUser Requestor { get; private set; }
        public DateTime RequestTime { get; private set; }
        public RequestStatus Status { get; set; }
        [UIValue("is-search")]
        public bool IsSearch => this.Status == RequestStatus.SongSearch;
        public string RequestInfo; // Contains extra song info, Like : Sub/Donation request, Deck pick, Empty Queue pick,Mapper request, etc.
        /// <summary>
        /// bsr key
        /// </summary>
        public string ID { get; private set; }
        private string _hash;
        private string _coverURL;
        private string _downloadURL;
        private string _songName;
        private string _rating;

        private const int s_coverCacheCapacity = 200;
        private static readonly object s_coverCacheLock = new object();
        private static readonly Dictionary<string, LinkedListNode<CachedCoverTexture>> s_coverTextureMap = new Dictionary<string, LinkedListNode<CachedCoverTexture>>();
        private static readonly LinkedList<CachedCoverTexture> s_coverTextureLru = new LinkedList<CachedCoverTexture>();

        private class CachedCoverTexture
        {
            public string Url;
            public Texture2D Texture;
        }

        public SongRequest Init(JSONObject obj)
        {
            _ = this.Init(
                obj["song"].AsObject,
                this.CreateRequester(obj),
                DateTime.FromFileTime(long.Parse(obj["time"].Value)),
                (RequestStatus)Enum.Parse(typeof(RequestStatus),
                obj["status"].Value),
                obj["requestInfo"].Value);
            return this;
        }

        public SongRequest Init(JSONObject song, IChatUser requestor, DateTime requestTime, RequestStatus status = RequestStatus.Invalid, string requestInfo = "")
        {
            this.SongNode = song;
            this._songName = this.SongMetaData["songName"].Value;
            this.ID = this.SongNode["id"].Value?.ToLower();
            this.Requestor = requestor;
            this.Status = status;
            this.RequestTime = requestTime;
            this.RequestInfo = requestInfo;
            var version = this.SongNode["versions"].AsArray.Children.FirstOrDefault(x => x["state"].Value == MapStatus.Published.ToString());
            if (version == null) {
                this.SongVersion = this.SongNode["versions"].AsArray.Children.OrderBy(x => DateTime.Parse(x["createdAt"].Value)).LastOrDefault().AsObject;
                this.IsWIP = true;
            }
            else {
                this.SongVersion = this.SongNode["versions"].AsArray.Children.FirstOrDefault(x => x["state"].Value == MapStatus.Published.ToString()).AsObject;
                this.IsWIP = false;
            }
            this._hash = this.SongVersion["hash"].Value;
            this._coverURL = this.SongVersion["coverURL"].Value;
            var cdnRoot = RequestBot.BEATMAPS_CDN_ROOT_URL.TrimEnd('/');
            // as.だのna.だの指定されると重くなるっぽい？
            this._downloadURL = this.SongVersion["downloadURL"].Value
                .Replace(RequestBot.BEATMAPS_AS_CDN_ROOT_URL, cdnRoot)
                .Replace(RequestBot.BEATMAPS_NA_CDN_ROOT_URL, cdnRoot);
            if (this._mapDatabase.PPMap.TryGetValue(this.ID, out var pp)) {
                this.SongNode.Add("pp", new JSONNumber(pp));
            }
            return this;
        }

        [UIAction("#post-parse")]
        internal void Setup()
        {
            var builder = new StringBuilder();
            _ = builder.Append(this.IsWIP ? $"<color=\"yellow\">[WIP]</color> {this._songName}" : this._songName);
            this._rating = RequestBotConfig.Instance.PPSearch && this._mapDatabase.PPMap.TryGetValue(this.ID, out var pp) && 0 < pp
                ? $" <size=50%>{Utility.GetRating(this.SongNode)} <color=#4169e1>{pp:0.00} PP</color></size>"
                : $" <size=50%>{Utility.GetRating(this.SongNode)}</size>";
            _ = builder.Append(this._rating);
            this.SongName = builder.ToString();
            this.SetCover();
        }

        [UIAction("selected")]
        private void Selected() { }

        [UIAction("hovered")]
        private void Hovered() { }

        [UIAction("un-selected-un-hovered")]
        private void UnSelectedUnHovered() { }
        /// <summary>
        /// lookup song from level id
        /// </summary>
        /// <returns></returns>
        private BeatmapLevel GetCustomLevel()
        {
            return Loader.GetLevelByHash(this._hash.ToUpper());
        }

        public void SetCover()
        {
            Dispatcher.RunOnMainThread(async () =>
            {
                try {
                    this._coverImage.enabled = false;
                    var dt = this._textFactory.Create().AddSong(this.SongNode).AddUser(this.Requestor); // Get basic fields
                    _ = dt.Add("Status", this.Status.ToString());
                    _ = dt.Add("Info", this.RequestInfo != "" ? " / " + this.RequestInfo : "");
                    _ = dt.Add("RequestTime", this.RequestTime.ToLocalTime().ToString("hh:mm"));
                    this.AuthorName = dt.Parse(StringFormat.QueueListRow2);
                    this.Hint = dt.Parse(StringFormat.SongHintText);

                    var imageSet = false;

                    if (Loader.AreSongsLoaded) {
                        var level = this.GetCustomLevel();
                        if (level != null) {
                            //Logger.Debug("custom level found");
                            // set image from song's cover image
                            var tex = await level.previewMediaData.GetCoverSpriteAsync();
                            this._coverImage.sprite = tex;
                            imageSet = true;
                        }
                    }

                    if (!imageSet) {
                        var cdnRoot = RequestBot.BEATMAPS_CDN_ROOT_URL.TrimEnd('/');
                        var url = !string.IsNullOrEmpty(this._coverURL) ? this._coverURL : $"{cdnRoot}/{this._hash.ToLower()}.jpg";
                        if (!TryGetCoverTextureFromCache(url, out var tex)) {
                            var b = await WebClient.DownloadImage(url, CancellationToken.None).ConfigureAwait(true);

                            if (b != null && b.Length > 0) {
                                tex = new Texture2D(2, 2);
                                if (tex.LoadImage(b)) {
                                    AddCoverTextureToCache(url, tex);
                                }
                                else {
                                    DisposeTexture(tex);
                                    tex = null;
                                }
                            }
                        }

                        if (tex != null) {
                            this._coverImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                        }
                    }
                }
                catch (Exception e) {
                    Logger.Error(e);
                }
                finally {
                    this._coverImage.enabled = true;
                }
            });
        }

        public JSONObject ToJson()
        {
            try {
                var obj = new JSONObject();
                obj.Add("status", new JSONString(this.Status.ToString()));
                obj.Add("requestInfo", new JSONString(this.RequestInfo));
                obj.Add("time", new JSONString(this.RequestTime.ToFileTime().ToString()));
                obj.Add("requestor", JsonConvert.SerializeObject(this.Requestor));
                obj.Add("song", this.SongNode);
                return obj;
            }
            catch (Exception ex) {
                Logger.Error(ex);
                return null;
            }
        }

        private IChatUser CreateRequester(JSONObject obj)
        {
            try {
                var requesterText = obj["requestor"].Value;
                if (string.IsNullOrWhiteSpace(requesterText)) {
                    Logger.Debug("CreateRequester: requestor 为空，回退 GenericChatUser。");
                    return new GenericChatUser("UnknownUser");
                }

                var userObj = JSONNode.Parse(requesterText)?.AsObject;
                if (userObj == null) {
                    Logger.Debug("CreateRequester: requestor 非法 JSON，回退 GenericChatUser。");
                    return new GenericChatUser("UnknownUser");
                }

                var displayName = userObj["DisplayName"].Value;
                if (string.IsNullOrWhiteSpace(displayName)) {
                    displayName = userObj["UserName"].Value;
                }
                if (string.IsNullOrWhiteSpace(displayName)) {
                    displayName = userObj["Id"].Value;
                }
                if (string.IsNullOrWhiteSpace(displayName)) {
                    displayName = "UnknownUser";
                }

                var badges = userObj["Badges"].AsArray;
                if (badges == null) {
                    var isInjectedBilibili = userObj["IsFan"].Value != string.Empty || userObj["GuardLevel"].Value != string.Empty;
                    if (isInjectedBilibili) {
                        return new InjectedBilibiliUser {
                            Id = userObj["Id"].Value ?? string.Empty,
                            UserName = string.IsNullOrWhiteSpace(userObj["UserName"].Value) ? displayName : userObj["UserName"].Value,
                            DisplayName = displayName,
                            Color = string.IsNullOrWhiteSpace(userObj["Color"].Value) ? "#FFFFFFFF" : userObj["Color"].Value,
                            IsBroadcaster = userObj["IsBroadcaster"].AsBool,
                            IsModerator = userObj["IsModerator"].AsBool,
                            IsFan = userObj["IsFan"].AsBool,
                            GuardLevel = userObj["GuardLevel"].AsInt
                        };
                    }

                    Logger.Debug("CreateRequester: Badges 缺失且非注入用户，回退 GenericChatUser。");
                    return new GenericChatUser(displayName);
                }

                var badgeList = new List<IChatBadge>();
                foreach (var badge in badges.Children) {
                    var tmp = new TwitchBadge(badge["Id"].Value, badge["Name"].Value, badge["Uri"].Value);
                    badgeList.Add(tmp);
                }
                var temp = new TwitchUser(
                    string.IsNullOrWhiteSpace(userObj["Id"].Value) ? displayName : userObj["Id"].Value,
                    string.IsNullOrWhiteSpace(userObj["UserName"].Value) ? displayName : userObj["UserName"].Value,
                    displayName,
                    userObj["Color"].Value,
                    userObj["IsModerator"].AsBool,
                    userObj["IsBroadcaster"].AsBool,
                    userObj["IsSubscriber"].AsBool,
                    userObj["IsTurbo"].AsBool,
                    userObj["IsVip"].AsBool,
                    new ReadOnlyCollection<IChatBadge>(badgeList));
                return temp;
            }
            catch (Exception e) {
                Logger.Error(e);
                return new GenericChatUser("UnknownUser");
            }
        }

        public async Task<string> DownloadZip(CancellationToken token = default(CancellationToken), IProgress<double> progress = null, Action<DownloadProgressInfo> advanced = null)
        {
            try {
                var cdnRoot = RequestBot.BEATMAPS_CDN_ROOT_URL.TrimEnd('/');
                var url = !string.IsNullOrEmpty(this._downloadURL)
                    ? this._downloadURL
                    : $"{cdnRoot}/{this._hash.ToLower()}.zip";

                if (DownloadService.Instance == null) {
                    Logger.Error("DownloadService.Instance == null，无法下载谱面。");
                    return null;
                }

                return await DownloadService.Instance.DownloadZip(url, token, progress, advanced).ConfigureAwait(false);
            }
            catch (TaskCanceledException) {
                Logger.Debug("谱面下载被取消。");
                return null;
            }
            catch (Exception e) {
                Logger.Error(e);
                return null;
            }
        }

        private static bool TryGetCoverTextureFromCache(string url, out Texture2D texture)
        {
            lock (s_coverCacheLock) {
                if (s_coverTextureMap.TryGetValue(url, out var node)) {
                    s_coverTextureLru.Remove(node);
                    s_coverTextureLru.AddFirst(node);
                    texture = node.Value.Texture;
                    return texture != null;
                }
            }

            texture = null;
            return false;
        }

        private static void AddCoverTextureToCache(string url, Texture2D texture)
        {
            if (string.IsNullOrEmpty(url) || texture == null) {
                return;
            }

            lock (s_coverCacheLock) {
                if (s_coverTextureMap.TryGetValue(url, out var existingNode)) {
                    if (!ReferenceEquals(existingNode.Value.Texture, texture)) {
                        DisposeTexture(existingNode.Value.Texture);
                        existingNode.Value.Texture = texture;
                    }

                    s_coverTextureLru.Remove(existingNode);
                    s_coverTextureLru.AddFirst(existingNode);
                }
                else {
                    var node = new LinkedListNode<CachedCoverTexture>(new CachedCoverTexture { Url = url, Texture = texture });
                    s_coverTextureLru.AddFirst(node);
                    s_coverTextureMap[url] = node;
                }

                while (s_coverTextureMap.Count > s_coverCacheCapacity) {
                    var oldest = s_coverTextureLru.Last;
                    if (oldest == null) {
                        break;
                    }

                    s_coverTextureLru.RemoveLast();
                    s_coverTextureMap.Remove(oldest.Value.Url);
                    DisposeTexture(oldest.Value.Texture);
                }
            }
        }

        private static void DisposeTexture(Texture2D texture)
        {
            if (texture == null) {
                return;
            }

            UnityEngine.Object.Destroy(texture);
        }

        public class SongRequestFactory : PlaceholderFactory<SongRequest>
        {

        }
    }
}
