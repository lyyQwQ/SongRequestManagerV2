#if !BS_1423
using SiraUtil.Interfaces;
#endif
using Zenject;

namespace SongRequestManagerV2.Localizes
{
    /// <summary>
    /// SiraLocalizer復活したらこっちでの実装も考える。
    /// </summary>
    public class LocalizeController : IInitializable
    {
#if BS_1423
        // 1.42.3 由 Harmony patch 在 Polyglot 载入前注入 csv。
        public void Initialize()
        {
        }
#else
        private readonly ILocalizer _localizer;

        public LocalizeController([InjectOptional(Id = "SIRA.Localizer")] ILocalizer localizer)
        {
            this._localizer = localizer;
        }

        public void Initialize()
        {
            Logger.Debug($"{this._localizer}:{this._localizer.GetType()}");
            _ = (this._localizer?.AddLocalizationSheetFromAssembly("SongRequestManagerV2.Resources.localize.csv", BGLib.Polyglot.GoogleDriveDownloadFormat.CSV));
        }
#endif
    }
}
