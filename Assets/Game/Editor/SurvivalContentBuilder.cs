using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Enemies;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class SurvivalContentBuilder
    {
        public static StationDefinition[] EnsurePrograms()
        {
            StationContentBuilder.EnsurePrograms();
            const string folder = "Assets/Game/Content/SurvivalPrograms";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "SurvivalPrograms");
            // Independent onboarding curve. Existing assets remain Inspector-editable after creation.
            return new[] { Program("Survival01", 1, new[] { Band(1, 4, 3, 3.5f) }),
                Program("Survival02", 2, new[] { Band(0, 2, 4, 4), Band(1, 3, 3, 4), Band(2, 2, 5, 4) }),
                Program("Survival03", 3, new[] { Band(0, 3, 4, 4, 1), Band(1, 4, 3, 4, 1), Band(2, 3, 5, 4, 1) }), Progression() };
            StationDefinition Progression()
            {
                string path = folder + "/Survival04.asset";
                var existing = AssetDatabase.LoadAssetAtPath<StationDefinition>(path); if (existing != null) return existing;
                var value = ScriptableObject.CreateInstance<StationDefinition>(); value.firstStation = 4;
                value.useThreatBudget = true; value.globalLiveLimit = 24; value.wagonLiveLimit = 8;
                value.bands = new[] { Threat("Intro", 1, 6, 4, 120), Threat("Fast", 2, 2, 5, 40), Threat("Tough", 4, 1, 8, 20) };
                AssetDatabase.CreateAsset(value, path); return value;
            }
            SpawnBand Threat(string name, int cost, int weight, int unlock, int cap) => new SpawnBand
            { profile = AssetDatabase.LoadAssetAtPath<EnemyProfile>("Assets/Game/Content/StationPrograms/" + name + ".asset"),
                wagon = -1, count = 1, maximumCount = cap, threatCost = cost, threatWeight = weight, unlockStation = unlock,
                firstAt = 4, interval = 2.5f };
            SpawnBand Band(int wagon, int count, float firstAt, float interval, int growth = 0) => new SpawnBand
            { profile = AssetDatabase.LoadAssetAtPath<EnemyProfile>("Assets/Game/Content/StationPrograms/Intro.asset"),
                wagon = wagon, count = count, firstAt = firstAt, interval = interval, perStationIncrease = growth, maximumCount = 12 };
            StationDefinition Program(string name, int first, SpawnBand[] bands)
            {
                string path = folder + "/" + name + ".asset";
                var value = AssetDatabase.LoadAssetAtPath<StationDefinition>(path);
                if (value != null) return value;
                value = ScriptableObject.CreateInstance<StationDefinition>();
                value.firstStation = first; value.globalLiveLimit = 24; value.wagonLiveLimit = 8;
                value.bands = bands;
                AssetDatabase.CreateAsset(value, path); return value;
            }
        }
        [MenuItem("Wait Your Turn/Survival/Apply Ammo and Difficulty %&u")]
        public static void ApplyTuning()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != TrainIntegrationBuilder.SurvivalScenePath) return;
            var spawner = Object.FindAnyObjectByType<StationSpawner>();
            spawner.ConfigurePrograms(EnsurePrograms());
            ShopContentBuilder.AddAmmoProducts(Object.FindAnyObjectByType<RunEconomy>().Catalog);
            EditorSceneManager.MarkSceneDirty(spawner.gameObject.scene); EditorSceneManager.SaveScene(spawner.gameObject.scene); AssetDatabase.SaveAssets();
            Debug.Log("[Survival] Cheap magazine products and station 4+ bounded threat budget installed; first 4/7/10 preserved.");
        }
        [MenuItem("Wait Your Turn/Survival/Check Open Train %&k")]
        public static void CheckCurrent()
        {
            if (!EditorApplication.isPlaying) return;
            var check = Object.FindAnyObjectByType<SurvivalAcceptance>();
            if (check != null) check.StartCoroutine(check.Check());
        }
        [MenuItem("Wait Your Turn/Survival/Check Onboarding and Shop %&j")]
        public static void CheckOnboarding()
        {
            if (!EditorApplication.isPlaying) return;
            var check = Object.FindAnyObjectByType<SurvivalOnboardingAcceptance>();
            if (check != null) check.StartCoroutine(check.Check());
        }
    }
}
