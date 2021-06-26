using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ColonistBarKF
{
    public static class Settings
    {
        [NotNull] public static SettingsColonistBar BarSettings = new SettingsColonistBar();

        [NotNull] public static SettingsPSI PSISettings = new SettingsPSI();

        public static float ViewOpacityCrit => Mathf.Max(PSISettings.IconOpacityCritical, PSISettings.IconOpacity);

        public static void SaveBarSettings([NotNull] string path = "ColonistBar_KF.xml")
        {
            var configFolder = Path.GetDirectoryName(GenFilePaths.ModsConfigFilePath);
            DirectXmlSaver.SaveDataObject(BarSettings, configFolder + "/" + path);
        }

        public static void SavePSISettings([NotNull] string path = "ColonistBar_PSIKF.xml")
        {
            var configFolder = Path.GetDirectoryName(GenFilePaths.ModsConfigFilePath);
            DirectXmlSaver.SaveDataObject(PSISettings, configFolder + "/" + path);
        }

        [NotNull]
        internal static SettingsColonistBar LoadBarSettings([NotNull] string path = "ColonistBar_KF.xml")
        {
            var configFolder = Path.GetDirectoryName(GenFilePaths.ModsConfigFilePath);
            var __result =
                DirectXmlLoader.ItemFromXmlFile<SettingsColonistBar>(configFolder + "/" + path);
            return __result;
        }

        [NotNull]
        internal static SettingsPSI LoadPSISettings([NotNull] string path = "ColonistBar_PSIKF.xml")
        {
            var configFolder = Path.GetDirectoryName(GenFilePaths.ModsConfigFilePath);
            var __result = DirectXmlLoader.ItemFromXmlFile<SettingsPSI>(configFolder + "/" + path);
            return __result;
        }
    }
}