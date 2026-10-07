using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Sandbox;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    public static class GameplaySandboxBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/GameplaySandbox.unity";
        [MenuItem("Wait Your Turn/Gameplay/Open One Wagon")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            var scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/NavigationSandbox.unity");
            // Save a separate fixture before changing anything; the accepted nav lab stays intact.
            EditorSceneManager.SaveScene(scene, ScenePath);
            Object.DestroyImmediate(Object.FindFirstObjectByType<NavigationSandboxController>().gameObject);
            EntryPortal portal = Object.FindFirstObjectByType<EntryPortal>();
            Renderer doorVisual = portal.GetComponentInChildren<Renderer>();
            HealthComponent doorHealth = Health(portal.gameObject, 10, Team.Neutral);
            Transform repairPoint = new GameObject("Repair anchor (inside wagon)").transform;
            repairPoint.SetParent(portal.transform, false);
            repairPoint.position = new Vector3(0, 0, 2.2f);
            DoorController door = portal.gameObject.AddComponent<DoorController>();
            door.Configure(doorHealth, portal, repairPoint);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Repair marker";
            marker.transform.position = repairPoint.position + Vector3.up * 0.015f;
            marker.transform.localScale = new Vector3(0.8f, 0.03f, 0.8f);
            marker.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/NavigationLab/Marker.mat");
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            Camera camera = Object.FindFirstObjectByType<Camera>();
            GameObject hero = new GameObject("Player (shared health / controller)");
            hero.transform.position = new Vector3(0, 0.05f, 5.5f);
            CharacterController body = hero.AddComponent<CharacterController>();
            body.height = 1.7f; body.radius = 0.3f; body.center = new Vector3(0, 0.85f, 0);
            body.skinWidth = 0.03f;
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Player visual";
            visual.transform.SetParent(hero.transform, false);
            visual.transform.localPosition = new Vector3(0, 0.85f, 0);
            visual.transform.localScale = new Vector3(0.6f, 0.85f, 0.6f);
            visual.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Player.mat");
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            HealthComponent player = Health(hero, 100, Team.Player);
            MoveInput input = hero.AddComponent<MoveInput>();
            PlayerMotor movement = hero.AddComponent<PlayerMotor>(); movement.Configure(input, player, camera);
            TargetRegistry registry = new GameObject("Live target registry").AddComponent<TargetRegistry>();
            HitscanPistol pistol = hero.AddComponent<HitscanPistol>(); pistol.Configure(player);
            AutoAim aim = hero.AddComponent<AutoAim>(); aim.Configure(registry, pistol);
            ProximityRepair repair = hero.AddComponent<ProximityRepair>(); repair.Configure(door, player);

            PortalNavigator oldMotor = Object.FindFirstObjectByType<PortalNavigator>();
            GameObject zombie = oldMotor.gameObject;
            zombie.name = "Enemy template (inactive)";
            zombie.SetActive(false);
            Object.DestroyImmediate(oldMotor);
            HealthComponent enemyHealth = Health(zombie, 10, Team.Enemy);
            AgentMotor motor = zombie.AddComponent<AgentMotor>();
            MeleeAttack attack = zombie.AddComponent<MeleeAttack>(); attack.Configure(enemyHealth);
            EnemyBrain brain = zombie.AddComponent<EnemyBrain>(); brain.Configure(enemyHealth, motor, attack);
            EnemyPool pool = new GameObject("Enemy pool (capacity 12)").AddComponent<EnemyPool>();
            pool.Configure(brain, door, player, registry);
            LineRenderer trace = new GameObject("Pistol trace (presentation only)").AddComponent<LineRenderer>();
            trace.positionCount = 2; trace.startWidth = trace.endWidth = 0.035f;
            trace.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Tracer.mat");
            trace.enabled = false;
            new GameObject("Gameplay lab HUD / acceptance fixture").AddComponent<GameplaySandboxController>()
                .Configure(door, player, movement, input, aim, pistol, repair, pool, doorVisual, trace);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[GameplaySandbox] Created one wagon using the existing carved nav and shared health modules.");
        }
        private static HealthComponent Health(GameObject root, float maximum, Team team)
        {
            HealthComponent health = root.AddComponent<HealthComponent>();
            var serialized = new SerializedObject(health);
            serialized.FindProperty("startingMaxHealth").floatValue = maximum;
            serialized.FindProperty("team").enumValueIndex = (int)team;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return health;
        }
    }
}
