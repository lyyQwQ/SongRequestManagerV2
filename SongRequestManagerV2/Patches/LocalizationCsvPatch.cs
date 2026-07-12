#if BS_1423
using BGLib.AppFlow.Initialization;
using BGLib.Polyglot;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SongRequestManagerV2.Patches
{
    [HarmonyPatch]
    internal static class LocalizationCsvPatch
    {
        private static TextAsset s_asset;

        private static MethodBase TargetMethod()
        {
            var registryType = AccessTools.Inner(typeof(AsyncInstaller), "IInstallerRegistry");
            if (registryType == null) {
                Logger.Error("Failed to resolve AsyncInstaller.IInstallerRegistry.");
                return null;
            }

            return AccessTools.Method(typeof(LocalizationAsyncInstaller), "LoadResourcesBeforeInstall", new[] { typeof(IList<TextAsset>), registryType });
        }

        [HarmonyPrepare]
        private static bool Prepare()
        {
            if (s_asset != null) {
                return true;
            }

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SongRequestManagerV2.Resources.localize.csv");
            if (stream == null) {
                Logger.Error("Failed to load SongRequestManagerV2.Resources.localize.csv.");
                return false;
            }

            using var reader = new StreamReader(stream);
            s_asset = new TextAsset(reader.ReadToEnd());
            return true;
        }

        [HarmonyPrefix]
        private static void Prefix(IList<TextAsset> assets)
        {
            if (s_asset != null) {
                assets.Add(s_asset);
            }
        }
    }
}
#endif
