#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CoffeeKing.EditorTools
{
    public static class ReleasePrepAudit
    {
        private const string ReportRelativePath = "build/outputs/logs/release-prep-audit.txt";
        private static readonly string[] RequiredFinalSprites =
        {
            "wall_background",
            "counter_edge",
            "counter_surface",
            "machine_empty",
            "machine_locked",
            "portafilter_empty",
            "portafilter_filled",
            "portafilter_tamped",
            "grinder_empty",
            "grinder_locked",
            "tamper",
            "shot_glass_empty",
            "shot_glass_filling",
            "shot_glass_full",
            "cup_plastic_empty",
            "cup_plastic_ice",
            "cup_plastic_shot",
            "cup_plastic_americano",
            "cup_plastic_latte",
            "cup_hot_empty",
            "cup_hot_americano",
            "cup_hot_latte",
            "cup_americano_lidded",
            "cup_latte_lidded",
            "cup_hot_americano_lidded",
            "cup_hot_latte_lidded",
            "lid",
            "hot_lid",
            "hot_water_dispenser",
            "water_bottle",
            "water_cup_empty",
            "water_cup_full",
            "milk_pitcher",
            "milk_pitcher_steamed",
            "monitor",
            "cup_trai",
            "coffeebean_basket",
            "steam_wand",
            "customer01",
            "customer_02",
            "customer_03",
            "customer_04",
            "customer_05"
        };

        [MenuItem("Coffee King/Run Release Prep Audit")]
        public static void RunMenu()
        {
            RunInternal();
        }

        public static void RunFromBatchMode()
        {
            RunInternal();
        }

        private static void RunInternal()
        {
            var infos = new List<string>();
            var warnings = new List<string>();

            AuditIdentity(infos, warnings);
            AuditAndroidBuildProfile(infos, warnings);
            AuditFinalArt(infos, warnings);

            var report = BuildReport(infos, warnings);
            var absoluteReportPath = Path.Combine(Directory.GetCurrentDirectory(), ReportRelativePath);
            var reportDirectory = Path.GetDirectoryName(absoluteReportPath);
            if (!string.IsNullOrEmpty(reportDirectory))
            {
                Directory.CreateDirectory(reportDirectory);
            }

            File.WriteAllText(absoluteReportPath, report);

            Debug.Log(report);
            if (warnings.Count > 0)
            {
                Debug.LogWarning($"Release prep audit completed with {warnings.Count} warning(s). Report: {absoluteReportPath}");
            }
            else
            {
                Debug.Log($"Release prep audit completed with no warnings. Report: {absoluteReportPath}");
            }
        }

        private static void AuditIdentity(List<string> infos, List<string> warnings)
        {
            infos.Add($"Company Name: {PlayerSettings.companyName}");
            infos.Add($"Product Name: {PlayerSettings.productName}");

            var androidApplicationId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            infos.Add($"Android Application Id: {androidApplicationId}");

            if (string.IsNullOrWhiteSpace(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany")
            {
                warnings.Add("Company name still uses the Unity template default.");
            }

            if (string.IsNullOrWhiteSpace(PlayerSettings.productName) || PlayerSettings.productName == "coffee-king")
            {
                warnings.Add("Product name still looks like a placeholder slug.");
            }

            if (string.IsNullOrWhiteSpace(androidApplicationId) || androidApplicationId.Contains("DefaultCompany"))
            {
                warnings.Add("Android application identifier is missing or still template-based.");
            }

            if (!PlayerSettings.Android.useCustomKeystore)
            {
                warnings.Add("Android custom keystore is not configured yet.");
            }
        }

        private static void AuditAndroidBuildProfile(List<string> infos, List<string> warnings)
        {
            var buildProfileDirectory = Path.Combine("Assets", "Settings", "Build Profiles");
            if (!Directory.Exists(buildProfileDirectory))
            {
                warnings.Add($"Android build profile directory not found at {buildProfileDirectory}.");
                return;
            }

            var buildProfilePaths = Directory.GetFiles(buildProfileDirectory, "Android*.asset");
            if (buildProfilePaths.Length == 0)
            {
                warnings.Add($"Android build profile asset not found inside {buildProfileDirectory}.");
                return;
            }

            var buildProfileText = File.ReadAllText(buildProfilePaths[0]);
            var appBundleEnabled = buildProfileText.Contains("m_BuildAppBundle: 1");
            infos.Add($"Android Build Profile App Bundle: {(appBundleEnabled ? "Enabled" : "Disabled")}");

            if (!appBundleEnabled)
            {
                warnings.Add("Android build profile is not configured for app bundle output.");
            }
        }

        private static void AuditFinalArt(List<string> infos, List<string> warnings)
        {
            var finalSpritesPath = Path.Combine(Application.dataPath, "Resources", "Sprites", "Final");
            var missingAssets = new List<string>();
            var presentCount = 0;

            for (var index = 0; index < RequiredFinalSprites.Length; index++)
            {
                var assetName = RequiredFinalSprites[index];
                var assetPath = Path.Combine(finalSpritesPath, assetName + ".png");
                if (File.Exists(assetPath))
                {
                    presentCount++;
                }
                else
                {
                    missingAssets.Add(assetName);
                }
            }

            infos.Add($"Final Art Coverage: {presentCount}/{RequiredFinalSprites.Length}");

            if (missingAssets.Count > 0)
            {
                warnings.Add("Missing final sprite assets: " + string.Join(", ", missingAssets));
            }
        }

        private static string BuildReport(IReadOnlyList<string> infos, IReadOnlyList<string> warnings)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Coffee King Release Prep Audit");
            builder.AppendLine();
            builder.AppendLine("Info");
            for (var index = 0; index < infos.Count; index++)
            {
                builder.AppendLine("- " + infos[index]);
            }

            builder.AppendLine();
            builder.AppendLine("Warnings");
            if (warnings.Count == 0)
            {
                builder.AppendLine("- None");
            }
            else
            {
                for (var index = 0; index < warnings.Count; index++)
                {
                    builder.AppendLine("- " + warnings[index]);
                }
            }

            return builder.ToString();
        }
    }
}
#endif
