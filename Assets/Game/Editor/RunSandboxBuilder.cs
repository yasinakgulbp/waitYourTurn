using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.AI.Navigation;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    public static class RunSandboxBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/RunSandbox.unity";
        [MenuItem("Wait Your Turn/Run/Open Two Wagons")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            var scene = EditorSceneManager.OpenScene("Assets/Game/Scenes/GameplaySandbox.unity");
            EditorSceneManager.SaveScene(scene, ScenePath);
            Object.DestroyImmediate(Object.FindAnyObjectByType<GameplaySandboxController>().gameObject);
            HealthComponent player = Object.FindAnyObjectByType<PlayerMotor>().GetComponent<HealthComponent>();
            TargetRegistry registry = Object.FindAnyObjectByType<TargetRegistry>();
            EnemyPool firstPool = Object.FindAnyObjectByType<EnemyPool>();
            GameObject firstRoot = new GameObject("Wagon A - persistent gameplay");
            Object.FindAnyObjectByType<NavMeshSurface>().transform.SetParent(firstRoot.transform, true);
            MovementArea firstArea = Object.FindAnyObjectByType<MovementArea>();
            firstArea.transform.SetParent(firstRoot.transform, true);
            firstPool.transform.SetParent(firstRoot.transform, true);
            WagonRuntime first = firstRoot.AddComponent<WagonRuntime>();
            first.Configure("wagon-a", firstRoot.GetComponentsInChildren<DoorController>(), firstPool, firstArea);
            GameObject secondRoot = Object.Instantiate(firstRoot);
            secondRoot.name = "Wagon B - persistent gameplay";
            secondRoot.transform.position = Vector3.right * 10;
            WagonRuntime second = secondRoot.GetComponent<WagonRuntime>();
            second.Configure("wagon-b", secondRoot.GetComponentsInChildren<DoorController>(),
                secondRoot.GetComponentInChildren<EnemyPool>(), secondRoot.GetComponentInChildren<MovementArea>());
            // Unity remaps door/area references within the cloned root; shared hero/registry/template remain shared.
            secondRoot.GetComponentInChildren<EnemyPool>().Configure(
                Object.FindObjectsByType<EnemyBrain>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0],
                second.Doors[0], player, registry);
            var doorHealth = new SerializedObject(second.Doors[0].Durability);
            doorHealth.FindProperty("startingMaxHealth").floatValue = 20;
            doorHealth.ApplyModifiedPropertiesWithoutUndo();
            foreach (WagonRuntime wagon in new[] { first, second })
                wagon.transform.Find("Connected Gameplay Geometry/Station Gameplay Surface/Station floor").GetComponent<Renderer>()
                    .shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            RunDriver run = new GameObject("Run flow - single phase authority").AddComponent<RunDriver>();
            run.Configure(new[] { first, second }, player, player.GetComponent<PlayerMotor>(), player.GetComponent<MoveInput>(),
                player.GetComponent<AutoAim>(), player.GetComponent<HitscanPistol>(), player.GetComponent<ProximityRepair>());
            StationSpawner spawner = new GameObject("Bounded station spawn adapter").AddComponent<StationSpawner>(); spawner.Configure(run);
            RunPresentation presentation = new GameObject("Train journey presentation").AddComponent<RunPresentation>();
            Transform visualEnvironment = GameObject.Find("Moving Visual Environment (no physics)").transform;
            presentation.Configure(run, visualEnvironment, Object.FindAnyObjectByType<Camera>());
            player.gameObject.AddComponent<ShotTracer>().Configure(player.GetComponent<HitscanPistol>(), Object.FindAnyObjectByType<LineRenderer>());
            new GameObject("Run lab HUD and acceptance fixture").AddComponent<RunSandboxController>().Configure(run, spawner, presentation);
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("[RunSandbox] Created two persistent wagons with 10/20 HP doors, shared player and bounded pools.");
        }
    }
}
