using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class ProgressionContentBuilder
    {
        [MenuItem("Wait Your Turn/Progression/Install Loot and Profile %&F9")]
        public static void InstallCurrent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var run = Object.FindAnyObjectByType<RunDriver>();
            if (run == null || run.Solo == null) return;
            Install(run, !run.Solo.OpenTrainSurvival); EditorSceneManager.MarkSceneDirty(run.gameObject.scene); EditorSceneManager.SaveScene(run.gameObject.scene);
            AssetDatabase.SaveAssets(); Debug.Log("[Progression] Permanent profile installed; loot follows the scene policy; navigation unchanged.");
        }
        public static void Install(RunDriver run, bool includeLoot = true)
        {
            if (run.Solo == null || run.Loot != null) return;
            foreach (var installed in Object.FindObjectsByType<RunProfile>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (installed.gameObject.scene == run.gameObject.scene) return;
            var root = new GameObject("Solo loot and permanent profile"); root.SetActive(false);
            var crates = new GameObject[run.Wagons.Length];
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/IntegrationBlockout/Marker.mat");
            for (int i = 1; includeLoot && i < crates.Length; i++)
            {
                var crate = GameObject.CreatePrimitive(PrimitiveType.Cube); crate.name = "Ammo crate - replaceable visual only";
                crate.transform.SetParent(run.Wagons[i].transform, false); crate.transform.localPosition = new Vector3(-3.6f, .3f, 0);
                crate.transform.localScale = new Vector3(.55f, .5f, .5f); crate.GetComponent<Renderer>().sharedMaterial = material;
                Object.DestroyImmediate(crate.GetComponent<Collider>()); crate.SetActive(false); crates[i] = crate;
            }
            var economy = Object.FindAnyObjectByType<RunEconomy>(); var save = Object.FindAnyObjectByType<RunPersistence>();
            if (includeLoot) root.AddComponent<SoloLoot>().Configure(run, crates);
            var profile = root.AddComponent<RunProfile>(); profile.Configure(run, save, economy);
            root.AddComponent<ProfileHud>().Configure(run, profile, run.Player.GetComponent<MoveInput>());
            root.AddComponent<ProgressionAcceptance>().Configure(run, economy, save, profile, Object.FindAnyObjectByType<StationSpawner>());
            root.SetActive(true);
        }
        [MenuItem("Wait Your Turn/Progression/Check Loot and Profile %&l")]
        public static void CheckCurrent()
        {
            if (!EditorApplication.isPlaying) return;
            var check = Object.FindAnyObjectByType<ProgressionAcceptance>(); if (check != null) check.StartCoroutine(check.Check());
        }
    }
}
