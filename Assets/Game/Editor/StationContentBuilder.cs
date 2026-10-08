using UnityEditor;
using UnityEngine;
using WaitYourTurn.Enemies;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    public static class StationContentBuilder
    {
        private const string Folder = "Assets/Game/Content/StationPrograms";
        public static StationDefinition[] EnsurePrograms()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "StationPrograms");
            var normal = Profile("Normal", "normal", 24, 2.1f, 2, 1, 8, new Color(.68f, .39f, .27f));
            var fast = Profile("Fast", "fast", 16, 3f, 1, .75f, 10, new Color(.55f, .75f, .35f));
            var tough = Profile("Tough", "tough", 70, 1.45f, 5, 1.4f, 20, new Color(.55f, .35f, .65f));
            return new[] { Program("Station01", 1, normal, fast, tough, 8, 2, 1),
                Program("Station03", 3, normal, fast, tough, 10, 4, 2),
                Program("Station06", 6, normal, fast, tough, 12, 6, 4) };
        }
        private static EnemyProfile Profile(string name, string id, float health, float speed, float damage, float interval, int reward, Color tint)
        {
            string path = Folder + "/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<EnemyProfile>(path); if (value != null) return value;
            value = ScriptableObject.CreateInstance<EnemyProfile>(); value.id = id; value.health = health;
            value.speed = speed; value.damage = damage; value.attackInterval = interval; value.reward = reward; value.blockoutTint = tint;
            AssetDatabase.CreateAsset(value, path); return value;
        }
        private static StationDefinition Program(string name, int first, EnemyProfile normal, EnemyProfile fast, EnemyProfile tough,
            int normalCount, int fastCount, int toughCount)
        {
            string path = Folder + "/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<StationDefinition>(path); if (value != null) return value;
            value = ScriptableObject.CreateInstance<StationDefinition>(); value.firstStation = first;
            value.bands = new[] { new SpawnBand { profile = normal, count = normalCount, firstAt = 2, interval = 3 },
                new SpawnBand { profile = fast, count = fastCount, firstAt = 10, interval = 7 },
                new SpawnBand { profile = tough, count = toughCount, firstAt = 20, interval = 10 } };
            AssetDatabase.CreateAsset(value, path); return value;
        }
    }
}
