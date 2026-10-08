using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    public static class BotContentBuilder
    {
        [MenuItem("Wait Your Turn/Battle/Install Bots In Integration %&F2")]
        public static void InstallCurrent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != TrainIntegrationBuilder.ScenePath) return;
            var run = Object.FindAnyObjectByType<RunDriver>();
            if (run == null || run.Match != null) return;
            Install(run, Object.FindAnyObjectByType<TargetRegistry>(), Object.FindAnyObjectByType<RunEconomy>(), Object.FindAnyObjectByType<StationSpawner>());
            EditorSceneManager.SaveScene(run.gameObject.scene); AssetDatabase.SaveAssets();
            Debug.Log("[Battle] Installed reusable bot participants and 3 skill profiles; train geometry/navigation preserved.");
        }
        public static void Install(RunDriver run, TargetRegistry registry, RunEconomy economy, StationSpawner spawner)
        {
            if (run.Wagons.Length < 2) return;
            const string folder = "Assets/Game/Content/Bots";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "Bots");
            var profiles = new BotProfile[3];
            for (int i = 0; i < 3; i++)
            {
                string skill = new[] { "Rookie", "Regular", "Veteran" }[i];
                profiles[i] = AssetDatabase.LoadAssetAtPath<BotProfile>(folder + "/" + skill + ".asset");
                if (profiles[i] != null) continue;
                var data = ScriptableObject.CreateInstance<BotProfile>(); data.skill = skill;
                data.decisionInterval = new[] { .6f, .35f, .2f }[i]; data.aimInterval = new[] { .35f, .2f, .12f }[i];
                data.reactionDelay = new[] { .55f, .25f, .08f }[i]; data.moveSpeed = new[] { 2.8f, 3.2f, 3.5f }[i];
                data.dangerDistance = new[] { 1.15f, 1.45f, 1.8f }[i]; data.purchaseInterval = new[] { 3f, 2f, 1.3f }[i];
                data.repairBelow = new[] { .45f, .65f, .85f }[i]; data.healBelow = new[] { .35f, .5f, .6f }[i];
                data.preferredWeapon = i == 0 ? 1 : 2; data.buyTurrets = i == 2;
                data.color = new[] { new Color(.35f, .65f, 1), new Color(.8f, .4f, 1), new Color(1, .7f, .2f) }[i];
                AssetDatabase.CreateAsset(data, folder + "/" + skill + ".asset"); profiles[i] = data;
            }
            var root = new GameObject("Battle - reusable participants"); root.SetActive(false);
            var bots = new BotController[run.Wagons.Length - 1];
            var names = new[] { "Ray", "Mira", "Kaya", "Nova" };
            for (int i = 0; i < bots.Length; i++)
            {
                var obj = Object.Instantiate(run.Player.gameObject, root.transform); obj.SetActive(false);
                string nick = i < names.Length ? names[i] : "Rider " + (i + 1);
                obj.name = nick + " - local bot";
                Object.DestroyImmediate(obj.GetComponent<MoveInput>());
                var health = obj.GetComponent<HealthComponent>(); var gun = obj.GetComponent<HitscanWeapon>(); gun.Configure(health);
                var aim = obj.GetComponent<AutoAim>(); aim.Configure(registry, gun); aim.enabled = false;
                var motor = obj.GetComponent<PlayerMotor>(); motor.ConfigureWorldControl(health, profiles[i % 3].moveSpeed);
                var repair = obj.GetComponent<ProximityRepair>(); repair.Configure(null, health);
                var line = new GameObject("Bot reusable shot trace").AddComponent<LineRenderer>(); line.transform.SetParent(obj.transform, false);
                line.positionCount = 2; line.useWorldSpace = true; line.startWidth = line.endWidth = .025f;
                line.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Tracer.mat"); line.enabled = false;
                obj.GetComponent<ShotTracer>().Configure(gun, line);
                foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                {
                    if (!renderer.name.StartsWith("Body")) continue;
                    var material = AssetDatabase.LoadAssetAtPath<Material>(folder + "/" + profiles[i % 3].skill + ".mat");
                    if (material == null)
                    {
                        material = new Material(renderer.sharedMaterial); material.color = profiles[i % 3].color;
                        AssetDatabase.CreateAsset(material, folder + "/" + profiles[i % 3].skill + ".mat");
                    }
                    renderer.sharedMaterial = material;
                }
                bots[i] = obj.AddComponent<BotController>(); bots[i].Configure(nick, profiles[i % 3], health, motor, aim, gun, repair);
            }
            var match = root.AddComponent<RunMatch>(); match.Configure(run, economy, bots);
            root.SetActive(true);
            var hud = new GameObject("Battle HUD and acceptance - F7");
            hud.AddComponent<BattleHud>().Configure(run, match);
            hud.AddComponent<BotAcceptance>().Configure(run, match, economy, spawner);
        }
    }
}
