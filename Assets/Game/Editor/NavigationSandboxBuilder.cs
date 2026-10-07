using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Navigation;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class NavigationSandboxBuilder
    {
        private const string Root = "Assets/Game";
        private const string ScenePath = Root + "/Scenes/NavigationSandbox.unity";

        [MenuItem("Wait Your Turn/Navigation/Upgrade Lab Platform Edge")]
        public static void UpgradeLabPlatformEdge()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildSandbox();
        }

        [MenuItem("Wait Your Turn/Navigation/Inspect Crowd Targets")]
        public static void InspectCrowdTargets()
        {
            if (!EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().path != ScenePath) return;
            foreach (PortalNavigator navigator in Object.FindObjectsByType<PortalNavigator>(FindObjectsSortMode.None))
            {
                float distance = Vector3.Distance(navigator.transform.position, navigator.InsideGoal);
                if (distance <= 0.4f) continue;
                NavMeshAgent agent = navigator.Agent;
                Debug.Log($"[NavigationCrowdTarget] {navigator.name}: position={navigator.transform.position:F3}, " +
                    $"goal={navigator.InsideGoal:F3}, distance={distance:F3}, state={navigator.State}, " +
                    $"path={agent.pathStatus}, remaining={agent.remainingDistance:F3}, velocity={agent.velocity:F3}", navigator);
            }
        }

        [MenuItem("Wait Your Turn/Navigation/Open Sandbox")]
        public static void OpenSandbox()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else BuildSandbox();
        }

        [MenuItem("Wait Your Turn/Navigation/Play Sandbox")]
        public static void PlaySandbox()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            OpenSandbox();
            if (EditorSceneManager.GetActiveScene().path != ScenePath) return;
            EditorApplication.isPlaying = true;
            // Actual acceptance controls remain available in the Game view (T or the button).
        }

        [MenuItem("Wait Your Turn/Navigation/Fit Lab Zombie Model")]
        public static void FitLabModel()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorSceneManager.GetActiveScene().path != ScenePath) return;
            PortalNavigator navigator = Object.FindFirstObjectByType<PortalNavigator>();
            if (navigator == null || navigator.transform.childCount == 0) return;
            FitVisual(navigator.transform.GetChild(0).gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        private static void FitVisual(GameObject visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            if (bounds.size.y <= 0.001f) return;
            visual.transform.localScale *= 1.7f / bounds.size.y;
            bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
            visual.transform.position += Vector3.up * (visual.transform.parent.position.y - bounds.min.y);
        }

        private static void BuildSandbox()
        {
            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Content/NavigationLab");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(0.55f, 0.6f, 0.7f);

            Material stationMat = MaterialAsset("Station", new Color(0.15f, 0.29f, 0.4f));
            Material wagonMat = MaterialAsset("Wagon", new Color(0.22f, 0.4f, 0.31f));
            Material wallMat = MaterialAsset("Wall", new Color(0.24f, 0.27f, 0.3f));
            Material doorMat = MaterialAsset("Door", new Color(0.9f, 0.22f, 0.17f));
            Material lineMat = MaterialAsset("Marker", new Color(0.9f, 0.7f, 0.25f));

            GameObject station = new GameObject("Station Gameplay Surface");
            GameObject geometry = new GameObject("Connected Gameplay Geometry");
            station.transform.SetParent(geometry.transform, false);
            Cube("Station floor", station.transform, new Vector3(0, -0.1f, -4.05f), new Vector3(10, 0.2f, 10), stationMat);
            GameObject wagon = new GameObject("Wagon Gameplay Surface");
            wagon.transform.SetParent(geometry.transform, false);
            Cube("Wagon floor", wagon.transform, new Vector3(0, -0.1f, 5), new Vector3(8, 0.2f, 8), wagonMat);
            Cube("Back wall", wagon.transform, new Vector3(0, 0.8f, 9), new Vector3(8, 1.6f, 0.2f), wallMat);
            Cube("Left wall", wagon.transform, new Vector3(-4, 0.8f, 5), new Vector3(0.2f, 1.6f, 8), wallMat);
            Cube("Right wall", wagon.transform, new Vector3(4, 0.8f, 5), new Vector3(0.2f, 1.6f, 8), wallMat);
            Cube("Front left", wagon.transform, new Vector3(-2.6f, 0.8f, 1), new Vector3(2.8f, 1.6f, 0.2f), wallMat);
            Cube("Front right", wagon.transform, new Vector3(2.6f, 0.8f, 1), new Vector3(2.8f, 1.6f, 0.2f), wallMat);
            Cube("Door sill", wagon.transform, new Vector3(0, -0.1f, 0.975f), new Vector3(2.2f, 0.2f, 0.1f), wallMat);
            NavMeshSurface connected = Surface(geometry);
            connected.overrideVoxelSize = true;
            connected.voxelSize = 0.1f;
            connected.BuildNavMesh();
            string navPath = Root + "/Content/NavigationLab/ConnectedNavMesh.asset";
            NavMeshData existingData = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if (existingData == null) AssetDatabase.CreateAsset(connected.navMeshData, navPath);
            else
            {
                EditorUtility.CopySerialized(connected.navMeshData, existingData);
                connected.navMeshData = existingData;
                EditorUtility.SetDirty(existingData);
            }

            GameObject entry = new GameObject("Entry Portal");
            entry.transform.SetParent(wagon.transform, false);
            entry.transform.position = new Vector3(0, 0, 1);
            GameObject door = Cube("Wagon door", entry.transform, new Vector3(0, 1, 1), new Vector3(2.2f, 2, 0.22f), doorMat);
            door.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            NavMeshObstacle cut = door.AddComponent<NavMeshObstacle>();
            cut.shape = NavMeshObstacleShape.Box;
            cut.center = Vector3.zero;
            cut.size = Vector3.one;
            cut.carving = true;
            cut.carveOnlyStationary = false;
            Transform approach = Anchor("Outside approach", entry.transform, new Vector3(0, 0, -0.2f));
            Transform goal = Anchor("Inside destination", entry.transform, new Vector3(0, 0, 6));
            EntryPortal portal = entry.AddComponent<EntryPortal>();
            portal.Configure(cut, door.GetComponent<BoxCollider>(), approach, goal);
            GameObject marker = Cube("Destination marker", null, goal.position + Vector3.up * 0.015f, new Vector3(0.6f, 0.03f, 0.6f), lineMat);
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            GameObject zombie = new GameObject("Lab Zombie 1");
            zombie.transform.position = new Vector3(0, 0, -7);
            NavMeshAgent agent = zombie.AddComponent<NavMeshAgent>();
            agent.enabled = false; // Activate only after baked surfaces are registered and spawn is validated.
            agent.agentTypeID = connected.agentTypeID;
            agent.radius = 0.3f;
            agent.height = 1.7f;
            agent.speed = 3f;
            agent.acceleration = 12f;
            agent.angularSpeed = 360f;
            agent.stoppingDistance = 0.12f;
            agent.autoRepath = true;
            CapsuleCollider capsule = zombie.AddComponent<CapsuleCollider>();
            capsule.radius = 0.3f;
            capsule.height = 1.7f;
            capsule.center = new Vector3(0, 0.85f, 0);
            PortalNavigator navigator = zombie.AddComponent<PortalNavigator>();
            navigator.Configure(portal, Vector3.zero, Vector3.zero);
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Scripts/EnemyPrefabs/Cananın göderdiği karakter/Beyaz/Beyaz-Zombie-Character.fbx");
            if (model != null)
            {
                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
                visual.name = "Zombie Visual (existing asset)";
                visual.transform.SetParent(zombie.transform, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = Vector3.one * 0.55f;
                foreach (Animator animator in visual.GetComponentsInChildren<Animator>()) animator.applyRootMotion = false;
                foreach (Collider collider in visual.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
                FitVisual(visual);
            }

            GameObject environment = new GameObject("Moving Visual Environment (no physics)");
            for (int i = 0; i < 8; i++)
            {
                GameObject strip = Cube("Rail sleeper " + i, environment.transform,
                    new Vector3(-7, -0.15f, -7 + i * 2), new Vector3(2, 0.12f, 0.18f), wallMat);
                Object.DestroyImmediate(strip.GetComponent<Collider>());
            }
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(12, 20, -16);
            cameraObject.transform.LookAt(Vector3.zero);
            camera.orthographic = true;
            camera.orthographicSize = 12;
            camera.backgroundColor = new Color(0.055f, 0.07f, 0.1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            Light sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(55, -30, 0);
            new GameObject("Navigation Lab Controls").AddComponent<NavigationSandboxController>()
                .Configure(portal, navigator, door.GetComponent<Renderer>(), environment.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = zombie;
            Debug.Log("[NavigationSandbox] Created a platform-edge wagon door on a continuous carved NavMesh.");
        }

        private static NavMeshSurface Surface(GameObject root)
        {
            NavMeshSurface surface = root.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            return surface;
        }
        private static Transform Anchor(string name, Transform parent, Vector3 position)
        {
            var anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            anchor.position = position;
            return anchor;
        }
        private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }
        private static Material MaterialAsset(string name, Color color)
        {
            string path = Root + "/Content/NavigationLab/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard")) { color = color };
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
