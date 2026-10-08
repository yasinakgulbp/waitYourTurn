using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    public static class TrainIntegrationBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/TrainIntegration.unity";
        private const string NavFolder = "Assets/Game/Content/IntegrationNavigation";
        private const string ArtFolder = "Assets/Game/Content/IntegrationBlockout";
        private const string LayoutFolder = "Assets/Game/Content/WagonLayouts";
        private static Material floor, wall, trim, red, glass, ground, rail, yellow, heroMat, enemyMat;

        [MenuItem("Wait Your Turn/Integration/Build One Wagon %#F4")]
        public static void BuildOne() => Build(1);
        [MenuItem("Wait Your Turn/Integration/Build Two Wagons %#F5")]
        public static void BuildTwo() => Build(2);
        [MenuItem("Wait Your Turn/Integration/Build Five Wagons %#F3")]
        public static void BuildFive() => Build(5);

        private static void Build(int count)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            PrepareMaterials();
            var layouts = new[] {
                Layout("SixDoor", DoorSlots.All, 10),
                Layout("FiveDoor", DoorSlots.All & ~DoorSlots.NorthCenter, 20),
                Layout("FourDoor", DoorSlots.All & ~(DoorSlots.NorthCenter | DoorSlots.SouthCenter), 40)
            };
            if (layouts.Any(x => !x.Valid)) throw new System.InvalidOperationException("Invalid wagon layout; check dimensions and 4–6 door slots.");
            var lab = EditorSceneManager.OpenScene("Assets/Game/Scenes/TrainSandbox.unity");
            var oldRun = Object.FindAnyObjectByType<RunDriver>();
            var hero = Object.Instantiate(oldRun.Player.gameObject);
            var template = Object.Instantiate(Object.FindObjectsByType<EnemyBrain>(FindObjectsInactive.Include).First().gameObject);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            SceneManager.MoveGameObjectToScene(hero, scene); SceneManager.MoveGameObjectToScene(template, scene);
            hero.name = "Player - stable gameplay root"; template.name = "Capsule enemy template - stable gameplay root"; template.SetActive(false);
            ReplaceActorVisual(hero, heroMat, true); ReplaceActorVisual(template, enemyMat, false);
            var player = hero.GetComponent<HealthComponent>();
            int agentType = EnsureAgentType();
            var agent = template.GetComponent<NavMeshAgent>(); agent.agentTypeID = agentType; agent.radius = .3f; agent.height = 1.7f;
            var registry = new GameObject("Live target registry").AddComponent<TargetRegistry>();
            var camera = new GameObject("Main Camera - responsive perspective cutaway").AddComponent<Camera>(); camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>(); camera.fieldOfView = 35; camera.nearClipPlane = .1f; camera.farClipPlane = 160;
            camera.transform.SetPositionAndRotation(new Vector3(0, 17, -12), Quaternion.Euler(55, 0, 0));
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.025f, .035f, .045f);
            hero.GetComponent<PlayerMotor>().Configure(hero.GetComponent<MoveInput>(), player, camera);
            hero.GetComponent<AutoAim>().Configure(registry, hero.GetComponent<HitscanWeapon>());
            var tracer = new GameObject("Shot trace").AddComponent<LineRenderer>();
            tracer.positionCount = 2; tracer.startWidth = tracer.endWidth = .035f;
            tracer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Tracer.mat"); tracer.enabled = false;
            hero.GetComponent<ShotTracer>().Configure(hero.GetComponent<HitscanWeapon>(), tracer);
            var light = new GameObject("Blockout key light").AddComponent<Light>(); light.type = LightType.Directional;
            light.intensity = .95f; light.color = new Color(.95f, .93f, .88f); light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(48, -30, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.5f, .52f, .56f);
            var track = new GameObject("Track visuals - no physics").transform;
            var station = new GameObject("Station visuals - both platforms - no physics").transform;
            var scenery = new GameObject("Journey scenery - no physics").transform;
            var fixedTrain = new GameObject("Train and station - fixed gameplay navigation");
            var wagons = new WagonRuntime[count];
            var centers = new float[count]; float cursor = 0;
            for (int i = 0; i < count; i++)
            {
                var layout = layouts[LayoutIndex(i)];
                if (i > 0) cursor += layouts[LayoutIndex(i - 1)].length * .5f + 1 + layout.length * .5f;
                centers[i] = cursor;
                var root = new GameObject("wagon-" + (char)('a' + i)); root.transform.SetParent(fixedTrain.transform, false); root.transform.localPosition = Vector3.right * cursor;
                var visual = new GameObject("Replaceable visuals - no gameplay ownership").transform; visual.SetParent(root.transform, false);
                var physics = new GameObject("Fixed gameplay geometry").transform; physics.SetParent(root.transform, false);
                float half = layout.length * .5f, sideZ = layout.width * .5f;
                Cube("Floor collider", physics, new Vector3(0, -.12f, 0), new Vector3(layout.length, .24f, layout.width), null, true);
                Cube("Floor visual", visual, new Vector3(0, -.08f, 0), new Vector3(layout.length, .16f, layout.width), floor, false);
                for (int x = 0; x < 12; x++)
                    Cube("Floor seam", visual, new Vector3(-half + (x + .5f) * layout.length / 12, .005f, 0), new Vector3(.025f, .01f, layout.width), trim, false);
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Cube("End wall collider", physics, new Vector3(sign * (half - .1f), 1.05f, 0), new Vector3(.2f, 2.1f, layout.width), null, true);
                    Cube("End wall visual", visual, new Vector3(sign * half, .8f, 0), new Vector3(.2f, 1.6f, layout.width), wall, false);
                    Cube("End rim", visual, new Vector3(sign * half, 1.65f, 0), new Vector3(.25f, .12f, layout.width), trim, false);
                    Cube("Coupling", visual, new Vector3(sign * (half + .4f), .15f, 0), new Vector3(.8f, .25f, .55f), trim, false);
                    Cube("Warm end lamp", visual, new Vector3(sign * (half + .12f), .9f, 0), new Vector3(.05f, .3f, .18f), yellow, false);
                }
                var doors = new List<DoorController>(); var spawns = new List<Transform>();
                // Compact array stays ordered by position, alternating sides; no phantom doorway for a missing slot.
                for (int slot = 0; slot < 6; slot++)
                {
                    if (!layout.HasDoor(slot)) continue;
                    int side = slot % 2; float sign = side == 0 ? -1 : 1;
                    float x = layout.DoorX(slot), width = layout.doorWidth;
                    var entry = new GameObject($"Door {doors.Count + 1} {(side == 0 ? "south" : "north")}"); entry.transform.SetParent(physics, false);
                    entry.transform.localPosition = new Vector3(x, 0, sign * sideZ); entry.transform.localRotation = Quaternion.Euler(0, side == 0 ? 0 : 180, 0);
                    var block = Cube("Movement blocker", entry.transform, new Vector3(0, 1.1f, 0), new Vector3(width, 2.2f, .16f), null, true);
                    block.layer = LayerMask.NameToLayer("ShotTransparent"); block.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
                    var cut = block.AddComponent<NavMeshObstacle>(); cut.shape = NavMeshObstacleShape.Box; cut.size = Vector3.one; cut.carving = true; cut.carveOnlyStationary = false;
                    var outside = Anchor("Outside approach", entry.transform, new Vector3(0, 0, -.8f));
                    var inside = Anchor("Inside destination", entry.transform, new Vector3(0, 0, 1));
                    var portal = entry.AddComponent<EntryPortal>(); portal.Configure(cut, block.GetComponent<BoxCollider>(), outside, inside);
                    var health = entry.AddComponent<HealthComponent>(); SetHealth(health, layout.doorHealth, Team.Neutral);
                    var repair = Anchor("Repair anchor", entry.transform, new Vector3(0, 0, .72f));
                    var door = entry.AddComponent<DoorController>(); door.Configure(health, portal, repair); doors.Add(door);
                    // Front wall/door visuals are cut away; full-height blockers retain the same rules on both sides.
                    float visualHeight = side == 0 ? 1.15f : layout.wallHeight;
                    var solid = Cube("Lower panel", entry.transform, new Vector3(0, .3f, 0), new Vector3(width, .6f, .16f), red, true);
                    solid.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
                    var window = Cube("Shoot-through glass", entry.transform, new Vector3(0, .6f + (visualHeight - .6f) * .5f, 0), new Vector3(width - .15f, visualHeight - .6f, .08f), glass, false);
                    entry.AddComponent<DoorPanels>().Configure(portal, solid, window);
                    for (int edge = -1; edge <= 1; edge += 2)
                        Cube("Door jamb", visual, new Vector3(x + edge * (width * .5f + .035f), visualHeight * .5f, sign * sideZ), new Vector3(.07f, visualHeight, .22f), trim, false);
                    Cube("Door header", visual, new Vector3(x, visualHeight, sign * sideZ), new Vector3(width + .14f, .1f, .22f), trim, false);
                    Cube("Repair floor strip", entry.transform, new Vector3(0, .01f, .72f), new Vector3(.6f, .02f, .09f), yellow, false);
                    Cube("Sill", physics, new Vector3(x, -.1f, sign * (sideZ + .1f)), new Vector3(width, .2f, .25f), null, true);
                    spawns.Add(Anchor("Station spawn " + spawns.Count, root.transform, new Vector3(x, 0, sign * (sideZ + 2))));
                }
                // Second row follows all first-row anchors, independent of door count.
                int firstRow = spawns.Count;
                for (int j = 0; j < firstRow; j++)
                {
                    Vector3 p = spawns[j].localPosition; p.z += Mathf.Sign(p.z) * 1.2f;
                    spawns.Add(Anchor("Station spawn " + spawns.Count, root.transform, p));
                }
                for (int side = 0; side < 2; side++)
                {
                    float sign = side == 0 ? -1 : 1, previous = -half;
                    foreach (int slot in Enumerable.Range(0, 6).Where(s => s % 2 == side && layout.HasDoor(s)))
                    { float x = layout.DoorX(slot); Wall(previous, x - layout.doorWidth * .5f); previous = x + layout.doorWidth * .5f; }
                    Wall(previous, half);
                    void Wall(float a, float b)
                    {
                        if (b <= a) return;
                        Cube("Wall segment collider", physics, new Vector3((a + b) * .5f, layout.wallHeight * .5f, sign * sideZ), new Vector3(b - a, layout.wallHeight, .18f), null, true);
                        float h = side == 0 ? 1.15f : layout.wallHeight;
                        Cube("Wall segment visual", visual, new Vector3((a + b) * .5f, h * .5f, sign * sideZ), new Vector3(b - a, h, .18f), wall, false);
                        Cube("Wall rim", visual, new Vector3((a + b) * .5f, h, sign * sideZ), new Vector3(b - a, .1f, .25f), trim, false);
                        if (b - a > 1)
                            Cube("Opaque window inset - wall blocks shots", visual, new Vector3((a + b) * .5f, h * .7f, sign * (sideZ - .1f)), new Vector3((b - a) * .7f, h * .3f, .04f), glass, false);
                    }
                }
                var area = root.AddComponent<MovementArea>(); area.Configure(new Vector2(-half + .2f, -sideZ + .09f), new Vector2(half - .2f, sideZ - .09f));
                var safe = new List<Transform> { Anchor("Safe center", root.transform, new Vector3(0, .05f, 0)) };
                for (int x = 0; x < 10; x++) for (int z = 0; z < 3; z++)
                    safe.Add(Anchor("Safe " + safe.Count, root.transform, new Vector3(Mathf.Lerp(-half + .85f, half - .85f, x / 9f), .05f, (z - 1) * (sideZ - .85f))));
                var space = root.AddComponent<WagonGeometry>(); space.Configure(new Bounds(new Vector3(0, 1, 0), new Vector3(layout.length - .4f, 3, layout.width)), doors.ToArray(), spawns.ToArray(), safe.ToArray());
                var pool = new GameObject("Enemy pool capacity 12").AddComponent<EnemyPool>(); pool.transform.SetParent(root.transform, false); pool.Configure(template.GetComponent<EnemyBrain>(), doors[0], player, registry);
                var wagon = root.AddComponent<WagonRuntime>(); wagon.Configure(root.name, doors.ToArray(), pool, area); wagon.ConfigureGeometry(space); wagons[i] = wagon;
            }
            int LayoutIndex(int i) => count == 5 ? new[] { 2, 1, 0, 1, 2 }[i] : i % layouts.Length;
            float start = -layouts[LayoutIndex(0)].length * .5f, end = cursor + layouts[LayoutIndex(count - 1)].length * .5f;
            float maxWidth = layouts.Max(x => x.width);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Vector3 pos = new Vector3((start + end) * .5f, -.12f, sign * (maxWidth * .5f + 3.08f));
                Vector3 size = new Vector3(end - start + 2, .24f, 6);
                Cube("Continuous fixed platform " + sign, fixedTrain.transform, pos, size, null, true);
                Cube("Continuous moving platform " + sign, station, pos, size, ground, false);
                for (float x = start - 1; x < end + 1; x += 2)
                {
                    Cube("Platform edge stripe", station, new Vector3(x, .01f, sign * (maxWidth * .5f + .5f)), new Vector3(1.2f, .02f, .1f), yellow, false);
                    Cube("Platform expansion seam", station, new Vector3(x, .005f, sign * (maxWidth * .5f + 3)), new Vector3(.03f, .01f, 5.8f), trim, false);
                }
            }
            var surface = fixedTrain.AddComponent<NavMeshSurface>(); surface.collectObjects = CollectObjects.Children; surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.agentTypeID = agentType; surface.overrideVoxelSize = true; surface.voxelSize = .05f;
            EnsureFolder(NavFolder); surface.BuildNavMesh(); surface.navMeshData.name = $"Train-{count}";
            string navPath = $"{NavFolder}/Train-{count}.asset"; var existing = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if (existing == null) AssetDatabase.CreateAsset(surface.navMeshData, navPath);
            else { EditorUtility.CopySerialized(surface.navMeshData, existing); surface.navMeshData = existing; EditorUtility.SetDirty(existing); }
            EditorSceneManager.CloseScene(lab, true);
            var run = new GameObject("Run flow - existing authority").AddComponent<RunDriver>();
            hero.GetComponent<ProximityRepair>().Configure(wagons[0].Doors[0], player); hero.GetComponent<PlayerMotor>().SetMovementArea(wagons[0].Area); hero.transform.position = new Vector3(0, .05f, 0);
            run.Configure(wagons, player, hero.GetComponent<PlayerMotor>(), hero.GetComponent<MoveInput>(), hero.GetComponent<AutoAim>(), hero.GetComponent<HitscanWeapon>(), hero.GetComponent<ProximityRepair>());
            int initial = count / 2; run.ConfigureInitialWagon(initial);
            hero.transform.position = wagons[initial].transform.position + Vector3.up * .05f;
            hero.GetComponent<PlayerMotor>().SetMovementArea(wagons[initial].Area);
            var spawner = new GameObject("Bounded station spawner").AddComponent<StationSpawner>(); spawner.Configure(run);
            var presentation = new GameObject("Journey presentation - single camera owner").AddComponent<RunPresentation>(); presentation.Configure(run, track, camera); presentation.ConfigureJourney(station, scenery); presentation.ConfigurePlayerFollow(true);
            presentation.ConfigureFraming(new Bounds(new Vector3(0, .4f, 0), new Vector3(layouts.Max(x => x.length) + 3, 2.4f, maxWidth + 4)));
            Bounds previewVolume = presentation.FramingVolume; previewVolume.Expand(new Vector3(1.3f, 0, 0));
            float previewDistance = WagonCameraFraming.Distance(previewVolume, camera.transform.rotation, camera.fieldOfView, camera.aspect, WagonCameraFraming.ProtectedViewport);
            camera.transform.position = wagons[initial].transform.position - camera.transform.forward * previewDistance;
            for (float x = start - 20; x < end + 22; x += 1)
                Cube("Sleeper", track, new Vector3(x, -.3f, 0), new Vector3(.16f, .1f, maxWidth + .5f), trim, false);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Cube("Rail", track, new Vector3((start + end) * .5f, -.22f, sign * 1.45f), new Vector3(end - start + 45, .1f, .09f), rail, false);
                for (float x = start - 24; x < end + 44; x += 4)
                {
                    Cube("Journey ground", scenery, new Vector3(x, -.4f, sign * 8), new Vector3(4, .2f, 12), ground, false);
                    Cube("Passing scenery marker", scenery, new Vector3(x, -.25f, sign * 9), new Vector3(.25f, .08f, 1.2f), trim, false);
                }
            }
            new GameObject("Integration HUD and checks").AddComponent<TrainIntegrationController>().Configure(run, spawner, presentation);
            EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.SaveAssets(); Selection.activeGameObject = hero;
            Debug.Log($"[TrainIntegration] Built {count} scale-reference wagons; doors {string.Join(",", wagons.Select(w => w.Doors.Length))}; bilateral geometry freshly baked.");
        }

        private static void ReplaceActorVisual(GameObject actor, Material material, bool player)
        {
            actor.transform.localScale = Vector3.one;
            // Keep the reusable cosmetic muzzle child; remove only the old body/animation hierarchy.
            foreach (Transform child in actor.transform.Cast<Transform>().ToArray())
                if (child.GetComponentInChildren<ParticleSystem>() == null) Object.DestroyImmediate(child.gameObject);
            foreach (var renderer in actor.GetComponents<Renderer>()) Object.DestroyImmediate(renderer);
            foreach (var filter in actor.GetComponents<MeshFilter>()) Object.DestroyImmediate(filter);
            foreach (var animator in actor.GetComponents<Animator>()) Object.DestroyImmediate(animator);
            var visual = new GameObject("Replaceable actor visuals - no colliders").transform; visual.SetParent(actor.transform, false);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name = "Body 1.7m x 0.6m"; body.transform.SetParent(visual, false);
            body.transform.localPosition = new Vector3(0, .85f, 0); body.transform.localScale = new Vector3(.6f, .85f, .6f); body.GetComponent<Renderer>().sharedMaterial = material; Object.DestroyImmediate(body.GetComponent<Collider>());
            Cube("Facing nose", visual, new Vector3(0, 1.1f, .38f), new Vector3(.22f, .16f, .3f), player ? yellow : red, false);
            Cube("Forward stripe", visual, new Vector3(0, 1.71f, .1f), new Vector3(.1f, .035f, .4f), yellow, false);
            if (player)
            {
                // Four corner marks keep the player readable without changing collision radius.
                for (int sign = -1; sign <= 1; sign += 2)
                    Cube("Player ground marker", visual, new Vector3(sign * .4f, .015f, 0), new Vector3(.06f, .025f, .65f), material, false);
            }
        }
        private static WagonLayoutDefinition Layout(string name, DoorSlots openings, float health)
        {
            EnsureFolder(LayoutFolder); string path = LayoutFolder + "/" + name + ".asset";
            var value = AssetDatabase.LoadAssetAtPath<WagonLayoutDefinition>(path); if (value != null) return value;
            value = ScriptableObject.CreateInstance<WagonLayoutDefinition>(); value.openings = openings; value.doorHealth = health; AssetDatabase.CreateAsset(value, path); return value;
        }
        private static void PrepareMaterials()
        {
            EnsureFolder(ArtFolder);
            floor = Material("Floor", new Color(.23f, .27f, .3f)); wall = Material("Wall", new Color(.3f, .35f, .39f)); trim = Material("Trim", new Color(.12f, .16f, .19f));
            red = Material("Door", new Color(.6f, .08f, .065f)); glass = Material("Glass", new Color(.12f, .3f, .37f)); ground = Material("Platform", new Color(.075f, .095f, .115f));
            rail = Material("Rail", new Color(.38f, .43f, .48f)); yellow = Material("Marker", new Color(.95f, .62f, .12f), true);
            heroMat = Material("Player", new Color(.2f, .8f, .42f)); enemyMat = Material("Enemy", new Color(.68f, .39f, .27f));
        }
        private static Material Material(string name, Color color, bool emission = false)
        {
            string path = ArtFolder + "/" + name + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(path); if (material != null) return material;
            material = new Material(Shader.Find("Standard")) { color = color }; material.SetFloat("_Glossiness", .18f);
            if (emission) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * .5f); }
            AssetDatabase.CreateAsset(material, path); return material;
        }
        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder("Assets/Game/Content", path.Substring(path.LastIndexOf('/') + 1));
        }
        private static int EnsureAgentType()
        {
            for (int i = 0; i < NavMesh.GetSettingsCount(); i++) { int id = NavMesh.GetSettingsByIndex(i).agentTypeID; if (NavMesh.GetSettingsNameFromID(id) == "Train Zombie") return id; }
            throw new System.InvalidOperationException("Train Zombie agent type is missing; restore the checked-in NavMeshAreas settings.");
        }
        private static Transform Anchor(string name, Transform parent, Vector3 local)
        { var a = new GameObject(name).transform; a.SetParent(parent, false); a.localPosition = local; return a; }
        private static GameObject Cube(string name, Transform parent, Vector3 local, Vector3 size, Material material, bool collider)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.name = name; cube.transform.SetParent(parent, false); cube.transform.localPosition = local; cube.transform.localScale = size;
            if (material != null) cube.GetComponent<Renderer>().sharedMaterial = material; else cube.GetComponent<Renderer>().enabled = false;
            if (!collider) Object.DestroyImmediate(cube.GetComponent<Collider>()); return cube;
        }
        private static void SetHealth(HealthComponent health, float max, Team team)
        { var s = new SerializedObject(health); s.FindProperty("startingMaxHealth").floatValue = max; s.FindProperty("team").intValue = (int)team; s.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
