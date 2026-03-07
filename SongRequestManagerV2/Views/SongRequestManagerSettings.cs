using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using SongRequestManagerV2.Configuration;
using SongRequestManagerV2.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace SongRequestManagerV2.Views
{
    [HotReload]
    public class SongRequestManagerSettings : BSMLAutomaticViewController, IInitializable
    {
        private const string MainMenuSceneName = "MainMenu";
        private bool _settingsRegistered;
        private bool _sceneHooked;

        public string ResourceName => "SongRequestManagerV2.Views.SongRequestManagerSettings.bsml";

        [UIValue("autopick-first-song")]
        public bool AutopickFirstSong
        {
            get => RequestBotConfig.Instance.AutopickFirstSong;

            set => RequestBotConfig.Instance.AutopickFirstSong = value;
        }
        [UIValue("clear-nofail")]
        public bool ClearNofail
        {
            get => RequestBotConfig.Instance.ClearNoFail;

            set => RequestBotConfig.Instance.ClearNoFail = value;
        }

        [UIValue("lowest-allowed-rating")]
        public float LowestAllowedRating
        {
            get => RequestBotConfig.Instance.LowestAllowedRating;

            set => RequestBotConfig.Instance.LowestAllowedRating = value;
        }

        [UIValue("maximum-song-length")]
        public int MaximumSongLength
        {
            get => (int)RequestBotConfig.Instance.MaximumSongLength;

            set => RequestBotConfig.Instance.MaximumSongLength = value;
        }

        [UIValue("minimum-njs")]
        public int MinimumNJS
        {
            get => (int)RequestBotConfig.Instance.MinimumNJS;

            set => RequestBotConfig.Instance.MinimumNJS = value;
        }

        [UIValue("tts-support")]
        public bool TtsSupport
        {
            get => !string.IsNullOrEmpty(RequestBotConfig.Instance.BotPrefix);

            set => RequestBotConfig.Instance.BotPrefix = value ? "! " : "";
        }

        [UIValue("user-request-limit")]
        public int UserRequestLimit
        {
            get => RequestBotConfig.Instance.UserRequestLimit;

            set => RequestBotConfig.Instance.UserRequestLimit = value;
        }

        [UIValue("sub-request-limit")]
        public int SubRequestLimit
        {
            get => RequestBotConfig.Instance.SubRequestLimit;

            set => RequestBotConfig.Instance.SubRequestLimit = value;
        }

        [UIValue("mod-request-limit")]
        public int ModRequestLimit
        {
            get => RequestBotConfig.Instance.ModRequestLimit;

            set => RequestBotConfig.Instance.ModRequestLimit = value;
        }

        [UIValue("vip-bonus-requests")]
        public int VipBonusRequests
        {
            get => RequestBotConfig.Instance.VipBonusRequests;

            set => RequestBotConfig.Instance.VipBonusRequests = value;
        }

        [UIValue("mod-full-rights")]
        public bool ModFullRights
        {
            get => RequestBotConfig.Instance.ModFullRights;

            set => RequestBotConfig.Instance.ModFullRights = value;
        }

        [UIValue("limit-user-requests-to-session")]
        public bool LimitUserRequestsToSession
        {
            get => RequestBotConfig.Instance.LimitUserRequestsToSession;

            set => RequestBotConfig.Instance.LimitUserRequestsToSession = value;
        }

        [UIValue("session-reset-after-xhours")]
        public int SessionResetAfterXHours
        {
            get => RequestBotConfig.Instance.SessionResetAfterXHours;

            set => RequestBotConfig.Instance.SessionResetAfterXHours = value;
        }

        [UIValue("performance-mode")]
        public bool PerformanceMode
        {
            get => RequestBotConfig.Instance.PerformanceMode;

            set => RequestBotConfig.Instance.PerformanceMode = value;
        }

        [UIValue("is-sound-enable")]
        public bool IsSoundEnable
        {
            get => RequestBotConfig.Instance.NotifySound;

            set => RequestBotConfig.Instance.NotifySound = value;
        }

        [UIValue("volume")]
        public int Volume
        {
            get => RequestBotConfig.Instance.SoundVolume;

            set => RequestBotConfig.Instance.SoundVolume = value;
        }
        [UIValue("pp-sarch")]
        public bool PPSerch
        {
            get => RequestBotConfig.Instance.PPSearch;

            set => RequestBotConfig.Instance.PPSearch = value;
        }

        [UIValue("feedback-text")]
        public bool FeedbackText
        {
            get => RequestBotConfig.Instance.FeedbackText;

            set => RequestBotConfig.Instance.FeedbackText = value;
        }

        [UIValue("beatsaver-servers")]
        public List<object> BeatsaverServers { get; } = new List<object>()
            {
                BeatsaverServerToChinese(BeatsaverServer.Beatsaver),
                BeatsaverServerToChinese(BeatsaverServer.EstrellaTest)
            };

        [UIValue("beatsaver-server")]
        public string CurrentBeatsaverServer
        {
            get => BeatsaverServerToChinese(NormalizeBeatsaverServer(RequestBotConfig.Instance.BeatsaverServer));

            set => RequestBotConfig.Instance.BeatsaverServer = NormalizeBeatsaverServer(
                Enum.GetValues(typeof(BeatsaverServer)).OfType<BeatsaverServer>().FirstOrDefault(x => BeatsaverServerToChinese(x) == value));
        }

        [UIValue("link-types")]
        public List<object> LinkTypes { get; } = new List<object>()
            {
                LinkTypeToChinese(LinkType.OnlyRequest),
                LinkTypeToChinese(LinkType.All)
            };

        [UIValue("link-type")]
        public string CurrentLinkType
        {
            get => LinkTypeToChinese(RequestBotConfig.Instance.LinkType);

            set => RequestBotConfig.Instance.LinkType = Enum.GetValues(typeof(LinkType)).OfType<LinkType>().FirstOrDefault(x => LinkTypeToChinese(x) == value);
        }
        public void Initialize()
        {
            if (_settingsRegistered)
            {
                return;
            }

            if (IsMainMenu(SceneManager.GetActiveScene()))
            {
                TryRegisterSettingsMenu();
                return;
            }

            if (_sceneHooked)
            {
                return;
            }

            _sceneHooked = true;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene previousScene, Scene nextScene)
        {
            if (!IsMainMenu(nextScene))
            {
                return;
            }

            TryRegisterSettingsMenu();
        }

        private void TryRegisterSettingsMenu()
        {
            if (_settingsRegistered)
            {
                UnhookSceneEvent();
                return;
            }

            try
            {
                BSMLSettings.Instance.AddSettingsMenu("SRM V2", this.ResourceName, this);
                _settingsRegistered = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SRM] Settings menu registration deferred failed: {ex.Message}");
            }
            finally
            {
                UnhookSceneEvent();
            }
        }

        private void UnhookSceneEvent()
        {
            if (!_sceneHooked)
            {
                return;
            }

            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            _sceneHooked = false;
        }

        private static bool IsMainMenu(Scene scene)
        {
            return string.Equals(scene.name, MainMenuSceneName, StringComparison.Ordinal);
        }

        public static string LinkTypeToChinese(LinkType linkType)
        {
            switch (linkType)
            {
                case LinkType.OnlyRequest:
                    return "仅点歌";
                case LinkType.All:
                    return "全部";
                default:
                    return "全部";
            }
        }

        public static string BeatsaverServerToChinese(BeatsaverServer beatsaverServer)
        {
            switch (beatsaverServer)
            {
                case BeatsaverServer.Beatsaver:
                    return "默认(BeatSaver)";
                case BeatsaverServer.EstrellaTest:
                    return "中国大陆(Estrella)";
                default:
                    return "默认(BeatSaver)";
            }
        }

        private static BeatsaverServer NormalizeBeatsaverServer(BeatsaverServer beatsaverServer)
        {
            switch (beatsaverServer)
            {
                case BeatsaverServer.BeatSaberChina:
                case BeatsaverServer.WGzeyu:
                    return BeatsaverServer.Beatsaver;
                default:
                    return beatsaverServer;
            }
        }
    }
}
