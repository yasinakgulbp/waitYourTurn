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
            // Independent onboarding curve. Existing assets remain Inspector-editable after creation.
            return new[] { Program("Survival01", 1, new[] { Band(1, 4, 3, 3.5f) }),
                Program("Survival02", 2, new[] { Band(0, 2, 4, 4), Band(1, 3, 3, 4), Band(2, 2, 5, 4) }),
                Program("Survival03", 3, new[] { Band(0, 3, 4, 4, 1), Band(1, 4, 3, 4, 1), Band(2, 3, 5, 4, 1) }) };
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
