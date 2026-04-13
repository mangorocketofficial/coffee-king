#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace CoffeeKing.EditorTools
{
    public static class AndroidQaBuildTools
    {
        private const string QaOutputRelativePath = "build/qa/CoffeeKing-qa.apk";
        private const string FallbackScenePath = "Assets/Scenes/SampleScene.unity";

        [MenuItem("Coffee King/Build Android QA APK")]
        public static void BuildAndroidQaApkMenu()
        {
            BuildAndroidQaApk();
        }

        public static void BuildAndroidQaApk()
        {
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), QaOutputRelativePath);
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var previousBuildAppBundle = EditorUserBuildSettings.buildAppBundle;
            try
            {
                EditorUserBuildSettings.buildAppBundle = false;

                var scenes = GetEnabledScenes();
                if (scenes.Length == 0)
                {
                    throw new InvalidOperationException("No enabled scenes found in EditorBuildSettings.");
                }

                EnsureBuildSettingsContainScenes(scenes);

                var options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outputPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.Development
                };

                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Android QA build failed with result {report.summary.result} at {outputPath}");
                }
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBuildAppBundle;
            }
        }

        private static string[] GetEnabledScenes()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), FallbackScenePath)))
            {
                return new[] { FallbackScenePath };
            }

            return Array.Empty<string>();
        }

        private static void EnsureBuildSettingsContainScenes(string[] scenes)
        {
            var editorScenes = new EditorBuildSettingsScene[scenes.Length];
            for (var index = 0; index < scenes.Length; index++)
            {
                editorScenes[index] = new EditorBuildSettingsScene(scenes[index], true);
            }

            EditorBuildSettings.scenes = editorScenes;
        }
    }
}
#endif
