using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WaitYourTurn.Editor
{
    /// <summary>Builds the isolated lab without replacing the prototype's scene list or app identity.</summary>
    public static class NavigationLabAndroidBuild
    {
        [MenuItem("Wait Your Turn/Navigation/Android/Prepare Platform")]
        public static void PreparePlatform()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        [MenuItem("Wait Your Turn/Navigation/Android/Build Lab APK")]
        public static void BuildApk()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.LogError("Prepare the Android platform and allow script reload before building the lab.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string scene = "Assets/Game/Scenes/NavigationSandbox.unity";
            if (!File.Exists(scene)) { Debug.LogError("Open the navigation sandbox first."); return; }
            // SetApplicationIdentifier also changes the override flag. Preserve the serialized
            // settings together so an isolated lab build cannot change the prototype identity.
            Object settings = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
            if (settings == null) { Debug.LogError("Cannot snapshot PlayerSettings; lab build cancelled."); return; }
            string settingsSnapshot = EditorJsonUtility.ToJson(settings);
            bool appBundle = EditorUserBuildSettings.buildAppBundle;
            bool exportProject = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            try
            {
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,
                    "com.yasinakgulbp.waityourturn.navigationlab");
                PlayerSettings.productName = "Wait Your Turn Navigation Lab";
                EditorUserBuildSettings.buildAppBundle = false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                Directory.CreateDirectory("Builds");
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = "Builds/NavigationLab.apk",
                    target = BuildTarget.Android,
                    options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
                });
                Debug.Log($"[NavigationLabBuild] {report.summary.result}; " +
                    $"{report.summary.totalSize} bytes; {report.summary.totalTime.TotalSeconds:F1}s.");
                if (report.summary.result != BuildResult.Succeeded)
                    Debug.LogError("[NavigationLabBuild] APK was not successfully built. Inspect the build errors.");
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
