using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WaitYourTurn.Editor
{
    /// <summary>Builds the current integrated game under a separate test app identity.</summary>
    public static class TrainIntegrationAndroidBuild
    {
        [MenuItem("Wait Your Turn/Run/Android/Build Integration APK")]
        public static void BuildApk()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.LogError("Prepare the Android platform and allow script reload before building integration.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string scene = "Assets/Game/Scenes/TrainIntegration.unity";
            if (!File.Exists(scene)) { Debug.LogError("TrainIntegration scene is missing."); return; }
            Object settings = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            if (settings == null) { Debug.LogError("Cannot snapshot PlayerSettings; integration build cancelled."); return; }
            string settingsSnapshot = EditorJsonUtility.ToJson(settings);
            bool appBundle = EditorUserBuildSettings.buildAppBundle;
            bool exportProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            try
            {
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,
                    "com.yasinakgulbp.waityourturn.integrationtest");
                PlayerSettings.productName = "Wait Your Turn Integration Test";
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                Directory.CreateDirectory("Builds");
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = "Builds/TrainIntegration.apk",
                    target = BuildTarget.Android,
                    options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
                });
                Debug.Log($"[TrainIntegrationBuild] {report.summary.result}; " +
                    $"{report.summary.totalSize} bytes; {report.summary.totalTime.TotalSeconds:F1}s.");
                if (report.summary.result != BuildResult.Succeeded)
                    Debug.LogError("[TrainIntegrationBuild] APK was not successfully built. Inspect the build errors.");
            }
            finally
            {
                EditorJsonUtility.FromJsonOverwrite(settingsSnapshot, settings);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                EditorUserBuildSettings.buildAppBundle = appBundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = exportProject;
            }
        }
    }
}
