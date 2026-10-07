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
            Cube("Station floor", station.transform, new Vector3(0, -0.1f, -5), new Vector3(10, 0.2f, 8), stationMat);
            NavMeshSurface outside = Surface(station);
            GameObject wagon = new GameObject("Wagon Gameplay Surface");
            Cube("Wagon floor", wagon.transform, new Vector3(0, -0.1f, 5), new Vector3(8, 0.2f, 8), wagonMat);
            Cube("Back wall", wagon.transform, new Vector3(0, 0.8f, 9), new Vector3(8, 1.6f, 0.2f), wallMat);
            Cube("Left wall", wagon.transform, new Vector3(-4, 0.8f, 5), new Vector3(0.2f, 1.6f, 8), wallMat);
            Cube("Right wall", wagon.transform, new Vector3(4, 0.8f, 5), new Vector3(0.2f, 1.6f, 8), wallMat);
            Cube("Front left", wagon.transform, new Vector3(-2.6f, 0.8f, 1), new Vector3(2.8f, 1.6f, 0.2f), wallMat);
            Cube("Front right", wagon.transform, new Vector3(2.6f, 0.8f, 1), new Vector3(2.8f, 1.6f, 0.2f), wallMat);
            NavMeshSurface inside = Surface(wagon);
            outside.BuildNavMesh();
            inside.BuildNavMesh();
            AssetDatabase.CreateAsset(outside.navMeshData, Root + "/Content/NavigationLab/StationNavMesh.asset");
            AssetDatabase.CreateAsset(inside.navMeshData, Root + "/Content/NavigationLab/WagonNavMesh.asset");

            GameObject entry = new GameObject("Entry Portal");
            NavMeshLink link = entry.AddComponent<NavMeshLink>();
            link.agentTypeID = outside.agentTypeID;
            link.startPoint = new Vector3(0, 0, -1.7f);
            link.endPoint = new Vector3(0, 0, 1.8f);
            link.width = 1.2f;
            link.bidirectional = true;
            link.autoUpdate = false;
            link.activated = false;
            GameObject door = Cube("Test gate", entry.transform, new Vector3(0, 1, 0), new Vector3(2.2f, 2, 0.22f), doorMat);
            GameObject bridge = Cube("Visual boarding bridge", entry.transform, new Vector3(0, -0.05f, 0), new Vector3(2.2f, 0.1f, 2), wallMat);
            Object.DestroyImmediate(bridge.GetComponent<Collider>()); // Navigation connection is exclusively the link.
            Transform approach = Anchor("Outside approach", entry.transform, new Vector3(0, 0, -2.2f));
            Transform goal = Anchor("Inside destination", entry.transform, new Vector3(0, 0, 6));
            EntryPortal portal = entry.AddComponent<EntryPortal>();
            portal.Configure(link, door.GetComponent<BoxCollider>(), approach, goal);
            GameObject marker = Cube("Destination marker", null, goal.position + Vector3.up * 0.015f, new Vector3(0.6f, 0.03f, 0.6f), lineMat);
            Object.DestroyImmediate(marker.GetComponent<Collider>());

            GameObject zombie = new GameObject("Lab Zombie 1");
            zombie.transform.position = new Vector3(0, 0, -7);
            NavMeshAgent agent = zombie.AddComponent<NavMeshAgent>();
            agent.enabled = false; // Activate only after baked surfaces are registered and spawn is validated.
            agent.agentTypeID = outside.agentTypeID;
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
            Debug.Log("[NavigationSandbox] Created isolated scene with two baked surfaces and one controlled link.");
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
