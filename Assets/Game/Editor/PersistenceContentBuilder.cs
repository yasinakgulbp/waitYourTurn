using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class PersistenceContentBuilder
    {
        [MenuItem("Wait Your Turn/Save/Install Local Save %&F3")]
        public static void InstallCurrent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != TrainIntegrationBuilder.ScenePath) return;
            Install(Object.FindAnyObjectByType<RunDriver>()); EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[Save] Installed local save/resume; existing train and navigation preserved. Editor F5 save / F6 load / F4 acceptance.");
        }
        public static void Install(RunDriver run)
        {
            if (run == null || run.Match == null || Object.FindAnyObjectByType<RunPersistence>() != null) return;
            var root = new GameObject("Local run persistence - F5 save F6 load F4 acceptance"); root.SetActive(false);
            var economy = Object.FindAnyObjectByType<RunEconomy>(); var spawner = Object.FindAnyObjectByType<StationSpawner>();
            var save = root.AddComponent<RunPersistence>(); save.Configure(run, economy, spawner);
            root.AddComponent<PersistenceAcceptance>().Configure(run, economy, spawner, save);
            root.AddComponent<PersistenceHud>().Configure(save); root.SetActive(true);
        }
    }
}
