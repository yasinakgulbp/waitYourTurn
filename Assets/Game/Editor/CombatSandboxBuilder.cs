using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class CombatSandboxBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/CombatSandbox.unity";

        [MenuItem("Wait Your Turn/Combat/Open Sandbox")]
        public static void Open()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) { EditorSceneManager.OpenScene(ScenePath); return; }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = Color.gray;
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Lab floor";
            floor.transform.position = new Vector3(0, -0.1f, 0);
            floor.transform.localScale = new Vector3(12, 0.2f, 10);
            floor.GetComponent<Renderer>().sharedMaterial = Material("Floor", new Color(0.12f, 0.2f, 0.25f));
            HealthComponent player = Target("Player", new Vector3(-3, 1, 0), Team.Player, 100, Color.green, PrimitiveType.Capsule);
            HealthComponent zombie = Target("Zombie", new Vector3(0, 1, 0), Team.Enemy, 10, Color.white, PrimitiveType.Capsule);
            HealthComponent door = Target("Door", new Vector3(3, 1, 0), Team.Neutral, 10, Color.red, PrimitiveType.Cube);
            door.transform.localScale = new Vector3(2, 2, 0.2f);
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0, 12, -12);
            camera.transform.LookAt(new Vector3(0, 0, 5));
            camera.orthographic = true;
            camera.orthographicSize = 7;
            camera.backgroundColor = new Color(0.04f, 0.06f, 0.08f);
            Light sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(55, -30, 0);
            LineRenderer tracer = new GameObject("Test pistol tracer").AddComponent<LineRenderer>();
            tracer.positionCount = 2;
            tracer.startWidth = tracer.endWidth = 0.035f;
            tracer.sharedMaterial = Material("Tracer", Color.yellow);
            tracer.enabled = false;
            new GameObject("Combat Lab Controls").AddComponent<CombatSandboxController>().Configure(player, zombie, door, camera, tracer);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[CombatSandbox] Created isolated common-health and hitscan proof scene.");
        }

        private static HealthComponent Target(string name, Vector3 position, Team team, float maximum, Color color, PrimitiveType primitive)
        {
            GameObject target = GameObject.CreatePrimitive(primitive);
            target.name = name;
            target.transform.position = position;
            target.GetComponent<Renderer>().sharedMaterial = Material(name, color);
            var health = target.AddComponent<HealthComponent>();
            var serialized = new SerializedObject(health);
            serialized.FindProperty("startingMaxHealth").floatValue = maximum;
            serialized.FindProperty("team").enumValueIndex = (int)team;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return health;
        }

        private static Material Material(string name, Color color)
        {
            const string folder = "Assets/Game/Content/CombatLab";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "CombatLab");
            string path = folder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Standard")) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
