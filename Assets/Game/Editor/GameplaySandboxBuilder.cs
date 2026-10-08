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
            HitscanWeapon pistol = hero.AddComponent<HitscanWeapon>(); pistol.Configure(player);
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
            ApplyDefenseRules();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[GameplaySandbox] Created one wagon using the existing carved nav and shared health modules.");
        }
        [MenuItem("Wait Your Turn/Gameplay/Apply Window And Player Boundary")]
        public static void ApplyDefenseRules()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorSceneManager.GetActiveScene().path != ScenePath) return;
            EntryPortal portal = Object.FindFirstObjectByType<EntryPortal>();
            PlayerMotor movement = Object.FindFirstObjectByType<PlayerMotor>();
            HitscanWeapon pistol = Object.FindFirstObjectByType<HitscanWeapon>();
            GameplaySandboxController hud = Object.FindFirstObjectByType<GameplaySandboxController>();
            if (portal == null || movement == null || pistol == null || hud == null) return;
            int transparentLayer = EnsureShotLayer();
            if (transparentLayer < 0) return;
            var serializedPortal = new SerializedObject(portal);
            BoxCollider movementBlocker = (BoxCollider)serializedPortal.FindProperty("blocker").objectReferenceValue;
            movementBlocker.gameObject.layer = transparentLayer;
            movementBlocker.GetComponent<Renderer>().enabled = false;
            GameObject solid = Panel(portal, "Solid lower door panel", 0.35f, 0.7f,
                AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/NavigationLab/Door.mat"), true);
            GameObject glass = Panel(portal, "Shoot-through upper window", 1.35f, 1.3f, GlassMaterial(), false);
            DoorPanels panels = portal.GetComponent<DoorPanels>();
            if (panels == null) panels = portal.gameObject.AddComponent<DoorPanels>();
            panels.Configure(portal, solid, glass);
            MovementArea area = movement.transform.parent == null ?
                Object.FindFirstObjectByType<MovementArea>() : movement.GetComponentInParent<MovementArea>();
            if (area == null) area = new GameObject("Player movement area (wagon interior)").AddComponent<MovementArea>();
            movement.SetMovementArea(area);
            pistol.SetHitMask(~(1 << transparentLayer));
            var serializedHud = new SerializedObject(hud);
            serializedHud.FindProperty("doorVisual").objectReferenceValue = solid.GetComponent<Renderer>();
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("[GameplaySandbox] Upper window allows shots; lower panel/walls block shots. Player movement stays in wagon.");
        }
        private static int EnsureShotLayer()
        {
            int existing = LayerMask.NameToLayer("ShotTransparent");
            if (existing >= 0) return existing;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = settings.FindProperty("layers");
            for (int i = 8; i < 32; i++)
            {
                if (!string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) continue;
                layers.GetArrayElementAtIndex(i).stringValue = "ShotTransparent";
                settings.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
            Debug.LogError("No free user layer for ShotTransparent; scene was not upgraded.");
            return -1;
        }
        private static GameObject Panel(EntryPortal portal, string name, float centerY, float height, Material material, bool solid)
        {
            Transform existing = portal.transform.Find(name);
            if (existing != null) return existing.gameObject;
            GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = name;
            panel.transform.SetParent(portal.transform, false);
            panel.transform.localPosition = Vector3.up * centerY;
            panel.transform.localScale = new Vector3(2.2f, height, 0.22f);
            panel.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(panel.GetComponent<Collider>());
            else panel.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            return panel;
        }
        private static Material GlassMaterial()
        {
            const string path = "Assets/Game/Content/CombatLab/DoorWindow.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Standard")) { color = new Color(0.3f, 0.75f, 0.95f, 0.22f), renderQueue = 3000 };
            material.SetFloat("_Mode", 3);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            AssetDatabase.CreateAsset(material, path);
            return material;
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
