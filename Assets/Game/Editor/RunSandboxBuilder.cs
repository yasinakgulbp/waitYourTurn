using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
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
        private const string TrainScenePath = "Assets/Game/Scenes/TrainSandbox.unity";
        private const string LayoutPath = "Assets/Game/Content/FiveWagonLayout.asset";

        [MenuItem("Wait Your Turn/Run/Upgrade Journey Visuals %#F7")]
        public static void UpgradeJourneyVisuals()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string path = EditorSceneManager.GetActiveScene().path;
            if (path != ScenePath && path != TrainScenePath) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string labPath in new[] { ScenePath, TrainScenePath })
            {
                if (!File.Exists(labPath)) continue;
                EditorSceneManager.OpenScene(labPath); EnsureWagonNavigation();
            }
            EditorSceneManager.OpenScene(path);
            Debug.Log("[RunJourney] Moving station/scenery visuals installed in both labs; colliders, baked navigation and spawn anchors remain fixed.");
        }

        [MenuItem("Wait Your Turn/Run/Align Lab Track Visuals")]
        public static void AlignTrackVisuals()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string path = EditorSceneManager.GetActiveScene().path;
            if (path != ScenePath && path != TrainScenePath) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string labPath in new[] { ScenePath, TrainScenePath })
            {
                if (!File.Exists(labPath)) continue;
                EditorSceneManager.OpenScene(labPath);
                EnsureWagonNavigation();
            }
            EditorSceneManager.OpenScene(path);
            Debug.Log("[RunTrack] Rails and sleepers aligned beneath the wagons along X; visual hierarchy has no colliders and gameplay geometry is unchanged.");
        }

        [MenuItem("Wait Your Turn/Run/Apply Five Wagon Layout")]
        public static void ApplyFiveLayout()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorSceneManager.GetActiveScene().path != TrainScenePath) return;
            TrainLayout layout = AssetDatabase.LoadAssetAtPath<TrainLayout>(LayoutPath);
            RunDriver run = Object.FindAnyObjectByType<RunDriver>();
            if (run == null || layout == null || layout.wagons == null || layout.wagons.Length != run.Wagons.Length) return;
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (WagonPlacement entry in layout.wagons)
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !ids.Add(entry.id) ||
                    !(entry.doorHealth > 0) || float.IsInfinity(entry.doorHealth))
                { Debug.LogError("Invalid or duplicate wagon layout entry."); return; }
            for (int i = 0; i < run.Wagons.Length; i++)
            {
                WagonRuntime wagon = run.Wagons[i]; WagonPlacement entry = layout.wagons[i];
                wagon.transform.position = entry.position;
                wagon.Configure(entry.id, wagon.Doors, wagon.Enemies, wagon.Area);
                foreach (DoorController door in wagon.Doors)
                {
                    var health = new SerializedObject(door.Durability);
                    health.FindProperty("startingMaxHealth").floatValue = entry.doorHealth;
                    health.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            EnsureWagonNavigation();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Wait Your Turn/Run/Open Five Wagons")]
        public static void OpenFive()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(TrainScenePath)) { EditorSceneManager.OpenScene(TrainScenePath); EnsureWagonNavigation(); return; }
            if (!File.Exists(ScenePath)) Open();
            TrainLayout layout = AssetDatabase.LoadAssetAtPath<TrainLayout>(LayoutPath);
            if (layout == null)
            {
                layout = ScriptableObject.CreateInstance<TrainLayout>();
                layout.wagons = new WagonPlacement[5];
                for (int i = 0; i < 5; i++) layout.wagons[i] = new WagonPlacement
                { id = "wagon-" + (char)('a' + i), position = Vector3.right * (10 * i), doorHealth = 10 * (i + 1) };
                AssetDatabase.CreateAsset(layout, LayoutPath);
            }
            if (layout.wagons == null || layout.wagons.Length != 5)
            { Debug.LogError("Five-wagon lab requires five layout entries."); return; }
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (WagonPlacement entry in layout.wagons)
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !ids.Add(entry.id) ||
                    !(entry.doorHealth > 0) || float.IsInfinity(entry.doorHealth))
                { Debug.LogError("Invalid or duplicate wagon layout entry."); return; }
            var scene = EditorSceneManager.OpenScene(ScenePath);
            EditorSceneManager.SaveScene(scene, TrainScenePath);
            RunDriver run = Object.FindAnyObjectByType<RunDriver>();
            WagonRuntime template = run.Wagons[0];
            for (int i = 1; i < run.Wagons.Length; i++) Object.DestroyImmediate(run.Wagons[i].gameObject);
            HealthComponent player = run.Player;
            EnemyBrain enemyTemplate = Object.FindObjectsByType<EnemyBrain>(FindObjectsInactive.Include)[0];
            TargetRegistry registry = Object.FindAnyObjectByType<TargetRegistry>();
            var wagons = new WagonRuntime[layout.wagons.Length];
            for (int i = 0; i < wagons.Length; i++)
            {
                WagonPlacement entry = layout.wagons[i];
                WagonRuntime wagon = i == 0 ? template : Object.Instantiate(template);
                wagon.name = entry.id + " - persistent gameplay";
                wagon.transform.position = entry.position;
                wagon.Configure(entry.id, wagon.GetComponentsInChildren<DoorController>(),
                    wagon.GetComponentInChildren<EnemyPool>(), wagon.GetComponentInChildren<MovementArea>());
                wagon.Enemies.Configure(enemyTemplate, wagon.Doors[0], player, registry);
                foreach (DoorController door in wagon.Doors)
                {
                    var health = new SerializedObject(door.Durability);
                    health.FindProperty("startingMaxHealth").floatValue = entry.doorHealth;
                    health.ApplyModifiedPropertiesWithoutUndo();
                }
                wagons[i] = wagon;
            }
            run.Configure(wagons, player, player.GetComponent<PlayerMotor>(), player.GetComponent<MoveInput>(),
                player.GetComponent<AutoAim>(), player.GetComponent<HitscanWeapon>(), player.GetComponent<ProximityRepair>());
            EnsureWagonNavigation();
            EditorSceneManager.SaveScene(scene, TrainScenePath); AssetDatabase.SaveAssets();
            Debug.Log("[TrainSandbox] Created five wagons from layout data; existing run flow reused.");
        }
        [MenuItem("Wait Your Turn/Run/Open Two Wagons")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); EnsureWagonNavigation(); return; }
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
                Object.FindObjectsByType<EnemyBrain>(FindObjectsInactive.Include)[0],
                second.Doors[0], player, registry);
            var doorHealth = new SerializedObject(second.Doors[0].Durability);
            doorHealth.FindProperty("startingMaxHealth").floatValue = 20;
            doorHealth.ApplyModifiedPropertiesWithoutUndo();
            foreach (WagonRuntime wagon in new[] { first, second })
                wagon.transform.Find("Connected Gameplay Geometry/Station Gameplay Surface/Station floor").GetComponent<Renderer>()
                    .shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            RunDriver run = new GameObject("Run flow - single phase authority").AddComponent<RunDriver>();
            run.Configure(new[] { first, second }, player, player.GetComponent<PlayerMotor>(), player.GetComponent<MoveInput>(),
                player.GetComponent<AutoAim>(), player.GetComponent<HitscanWeapon>(), player.GetComponent<ProximityRepair>());
            StationSpawner spawner = new GameObject("Bounded station spawn adapter").AddComponent<StationSpawner>(); spawner.Configure(run);
            RunPresentation presentation = new GameObject("Train journey presentation").AddComponent<RunPresentation>();
            Transform visualEnvironment = GameObject.Find("Moving Visual Environment (no physics)").transform;
            presentation.Configure(run, visualEnvironment, Object.FindAnyObjectByType<Camera>());
            player.gameObject.AddComponent<ShotTracer>().Configure(player.GetComponent<HitscanWeapon>(), Object.FindAnyObjectByType<LineRenderer>());
            new GameObject("Run lab HUD and acceptance fixture").AddComponent<RunSandboxController>().Configure(run, spawner, presentation);
            EnsureWagonNavigation();
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets();
            Debug.Log("[RunSandbox] Created two persistent wagons with 10/20 HP doors, shared player and bounded pools.");
        }

        private static void EnsureWagonNavigation()
        {
            // AI Navigation clears shared scene-surface data in OnValidate. Each lab wagon owns an asset.
            const string folder = "Assets/Game/Content/RunNavigation";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "RunNavigation");
            NavMeshData source = AssetDatabase.LoadAssetAtPath<NavMeshData>("Assets/Game/Content/NavigationLab/ConnectedNavMesh.asset");
            RunDriver run = Object.FindAnyObjectByType<RunDriver>();
            var scene = EditorSceneManager.GetActiveScene();
            bool changed = EnsureTrackVisuals(run);
            changed |= EnsureJourneyVisuals(run);
            for (int i = 0; i < run.Wagons.Length; i++)
            {
                NavMeshSurface surface = run.Wagons[i].GetComponentInChildren<NavMeshSurface>();
                string path = $"{folder}/{scene.name}-{i}.asset";
                NavMeshData data = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
                if (data == null)
                {
                    data = Object.Instantiate(source);
                    data.name = scene.name + " wagon " + i;
                    AssetDatabase.CreateAsset(data, path);
                }
                if (surface.navMeshData == data) continue;
                surface.RemoveData(); surface.navMeshData = data; surface.AddData();
                EditorUtility.SetDirty(surface); changed = true;
            }
            if (changed) { EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); }
            AssetDatabase.SaveAssets();
        }

        private static bool EnsureJourneyVisuals(RunDriver run)
        {
            bool changed = false;
            Transform station = Root("Station Visuals (no physics)");
            Transform scenery = Root("Journey Scenery (no physics)");
            Material wall = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/NavigationLab/Wall.mat");
            Material marker = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/NavigationLab/Marker.mat");
            float first = float.PositiveInfinity, last = float.NegativeInfinity;
            foreach (WagonRuntime wagon in run.Wagons)
            {
                first = Mathf.Min(first, wagon.transform.position.x); last = Mathf.Max(last, wagon.transform.position.x);
                Transform floor = wagon.transform.Find("Connected Gameplay Geometry/Station Gameplay Surface/Station floor");
                Renderer source = floor.GetComponent<Renderer>();
                if (source.enabled) { source.enabled = false; EditorUtility.SetDirty(source); changed = true; }
                // Copy only the mesh presentation. Never clone the source collider or surface.
                Part(station, wagon.Id + " platform", floor.position, floor.lossyScale, source.sharedMaterial);
                Part(station, wagon.Id + " platform edge", wagon.transform.TransformPoint(new Vector3(0, .012f, .7f)),
                    new Vector3(9.8f, .025f, .15f), marker);
                for (int i = 0; i < 2; i++)
                    Part(station, wagon.Id + " station column " + i,
                        wagon.transform.TransformPoint(new Vector3(i == 0 ? -3 : 3, 1.2f, -7.8f)),
                        new Vector3(.3f, 2.4f, .3f), wall);
            }
            // Uniform base and a 20-unit repeating pattern: the wrapped edges stay off camera.
            float min = Mathf.Floor((first - 80) / 20) * 20, max = Mathf.Ceil((last + 80) / 20) * 20;
            Part(scenery, "Journey ground", new Vector3((min + max) * .5f, -.65f, 0),
                new Vector3(max - min + 40, .2f, 60), wall);
            for (float x = min; x <= max; x += 20)
            {
                Part(scenery, "Wayside marker " + x, new Vector3(x, .35f, -12), new Vector3(1, 1, 1), marker);
                Part(scenery, "Wayside strip " + x, new Vector3(x + 5, -.48f, -15), new Vector3(8, .08f, .2f), marker);
            }
            RunPresentation presentation = Object.FindAnyObjectByType<RunPresentation>();
            if (presentation.StationVisuals != station || presentation.Scenery != scenery)
            { presentation.ConfigureJourney(station, scenery); EditorUtility.SetDirty(presentation); changed = true; }
            return changed;

            Transform Root(string name)
            {
                GameObject existing = GameObject.Find(name);
                if (existing != null) return existing.transform;
                changed = true; return new GameObject(name).transform;
            }
            void Part(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
            {
                Transform child = parent.Find(name);
                if (child == null)
                {
                    GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    part.name = name; part.transform.SetParent(parent, false); child = part.transform;
                    Object.DestroyImmediate(part.GetComponent<Collider>()); changed = true;
                }
                Renderer renderer = child.GetComponent<Renderer>();
                if (child.position == position && child.localScale == scale && renderer.sharedMaterial == material &&
                    renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.Off && !renderer.receiveShadows) return;
                child.SetPositionAndRotation(position, Quaternion.identity); child.localScale = scale;
                renderer.sharedMaterial = material; renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false; EditorUtility.SetDirty(child); EditorUtility.SetDirty(renderer); changed = true;
            }
        }

        private static bool EnsureTrackVisuals(RunDriver run)
        {
            Transform root = GameObject.Find("Moving Visual Environment (no physics)").transform;
            // These dimensions describe only the rectangular lab wagons, not production train assets.
            float first = float.PositiveInfinity, last = float.NegativeInfinity;
            foreach (WagonRuntime wagon in run.Wagons)
            { first = Mathf.Min(first, wagon.transform.position.x); last = Mathf.Max(last, wagon.transform.position.x); }
            const float spacing = 2;
            float min = Mathf.Floor((first - 16) / spacing) * spacing;
            float max = Mathf.Ceil((last + 16) / spacing) * spacing;
            Vector3 railCenter = new Vector3((min + max) * .5f, -.24f, 2);
            Vector3 railScale = new Vector3(max - min + spacing, .08f, .12f);
            int sleepers = Mathf.RoundToInt((max - min) / spacing) + 1;
            Transform existingRail = root.Find("Longitudinal rail 0");
            if (existingRail != null && root.childCount == sleepers + 2 &&
                existingRail.localPosition == railCenter && existingRail.localScale == railScale) return false;
            for (int i = root.childCount - 1; i >= 0; i--) Object.DestroyImmediate(root.GetChild(i).gameObject);
            root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.localScale = Vector3.one;
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/NavigationLab/Wall.mat");
            for (int i = 0; i < sleepers; i++)
                TrackPart("Rail sleeper " + i, new Vector3(min + i * spacing, -.34f, 5), new Vector3(.18f, .12f, 8.7f));
            for (int i = 0; i < 2; i++)
                TrackPart("Longitudinal rail " + i, railCenter + Vector3.forward * (6 * i), railScale);
            return true;

            void TrackPart(string name, Vector3 position, Vector3 scale)
            {
                GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                part.name = name; part.transform.SetParent(root, false);
                part.transform.localPosition = position; part.transform.localScale = scale;
                Object.DestroyImmediate(part.GetComponent<Collider>());
                Renderer renderer = part.GetComponent<Renderer>(); renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }
    }
}
