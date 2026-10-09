using UnityEditor;
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
            // Provisional per-wagon budgets; the original Battle assets remain untouched.
            return new[] { Program("Survival01", 1, "Intro", 6, 6),
                Program("Survival03", 3, "Normal", 8, 5), Program("Survival06", 6, "Normal", 10, 4) };
            StationDefinition Program(string name, int first, string profile, int count, float interval)
            {
                string path = folder + "/" + name + ".asset";
                var value = AssetDatabase.LoadAssetAtPath<StationDefinition>(path);
                if (value != null) return value;
                value = ScriptableObject.CreateInstance<StationDefinition>();
                value.firstStation = first; value.globalLiveLimit = 24; value.wagonLiveLimit = 8;
                value.bands = new[] { new SpawnBand { profile = AssetDatabase.LoadAssetAtPath<EnemyProfile>(
                    "Assets/Game/Content/StationPrograms/" + profile + ".asset"), count = count, firstAt = 4, interval = interval } };
                AssetDatabase.CreateAsset(value, path); return value;
            }
        }
        [MenuItem("Wait Your Turn/Survival/Check Open Train %&k")]
        public static void CheckCurrent()
        {
            if (!EditorApplication.isPlaying) return;
            var check = Object.FindAnyObjectByType<SurvivalAcceptance>();
            if (check != null) check.StartCoroutine(check.Check());
        }
    }
}
