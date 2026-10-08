using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;
using WaitYourTurn.Economy;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class TurretContentBuilder
    {
        private const string Folder = "Assets/Game/Content/Defenses";
        [MenuItem("Wait Your Turn/Defenses/Install Turrets In Integration %#F2")]
        public static void InstallCurrent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != TrainIntegrationBuilder.ScenePath) return;
            var run = Object.FindAnyObjectByType<RunDriver>();
            if (run == null || Object.FindAnyObjectByType<RunDefenses>() != null) return;
            Install(run, Object.FindAnyObjectByType<TargetRegistry>(), Object.FindAnyObjectByType<RunEconomy>(),
                Object.FindAnyObjectByType<StationSpawner>());
            EditorSceneManager.SaveScene(run.gameObject.scene); AssetDatabase.SaveAssets();
            Debug.Log("[Turrets] Installed authored mounts and shop products; existing geometry and NavMesh preserved.");
        }
        public static void Install(RunDriver run, TargetRegistry registry, RunEconomy economy, StationSpawner spawner)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "Defenses");
            var types = new[] { Type("Normal", 50, 1, 9, 12, 1.8f, Color.cyan),
                Type("Advanced", 80, .4f, 10, 18, 2, new Color(1, .55f, .1f)) };
            var racks = new TurretRack[run.Wagons.Length];
            var metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/IntegrationBlockout/Trim.mat");
            if (metal == null) metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Tracer.mat");
            var tracerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Tracer.mat");
            for (int w = 0; w < racks.Length; w++)
            {
                var wagon = run.Wagons[w];
                var slots = new TurretMount[4]; // Two of each type, independent of door count.
                for (int d = 0; d < slots.Length; d++)
                {
                    var pad = new GameObject("Turret pool slot " + (d + 1)); pad.transform.SetParent(wagon.transform, false);
                    var actor = new GameObject("Reusable turret actor"); actor.transform.SetParent(pad.transform, false);
                    actor.transform.localPosition = Vector3.up * .05f; actor.SetActive(false);
                    var health = actor.AddComponent<HealthComponent>();
                    var body = actor.AddComponent<BoxCollider>(); body.center = Vector3.up * .3f; body.size = new Vector3(.22f, .6f, .22f); body.enabled = false;
                    actor.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
                    var obstacle = actor.AddComponent<NavMeshObstacle>(); obstacle.shape = NavMeshObstacleShape.Box;
                    obstacle.center = Vector3.up * .3f; obstacle.size = new Vector3(.24f, .7f, .24f);
                    obstacle.carving = true; obstacle.carveOnlyStationary = false; obstacle.enabled = false;
                    var weapon = actor.AddComponent<HitscanWeapon>(); weapon.SetHitMask(run.Weapon.HitMask);
                    var visual = new GameObject("Replaceable turret visual"); visual.transform.SetParent(actor.transform, false);
                    Shape("Base", PrimitiveType.Cylinder, visual.transform, new Vector3(0, .18f, 0), new Vector3(.5f, .18f, .5f), metal);
                    Shape("Stem", PrimitiveType.Cube, visual.transform, new Vector3(0, .55f, 0), new Vector3(.14f, .45f, .14f), metal);
                    var head = new GameObject("Aiming head").transform; head.SetParent(visual.transform, false); head.localPosition = Vector3.up * .85f;
                    var accent = Shape("Head", PrimitiveType.Cube, head, Vector3.zero, new Vector3(.35f, .2f, .35f), metal).GetComponent<Renderer>();
                    Shape("Barrel - cosmetic muzzle at root + Y 0.9", PrimitiveType.Cube, head, new Vector3(0, .05f, .3f), new Vector3(.08f, .08f, .6f), metal);
                    var line = actor.AddComponent<LineRenderer>(); line.positionCount = 2; line.useWorldSpace = true;
                    line.startWidth = line.endWidth = .035f; line.sharedMaterial = tracerMat; line.enabled = false;
                    var burst = Shape("Reusable exhaustion burst - cosmetic", PrimitiveType.Sphere, actor.transform,
                        new Vector3(0, .5f, 0), Vector3.one * .2f, tracerMat).transform; burst.gameObject.SetActive(false);
                    var view = actor.AddComponent<TurretPresentation>(); view.Configure(weapon, visual, accent, line, burst);
                    var controller = actor.AddComponent<TurretController>(); controller.Configure(health, weapon, body, obstacle, head, view);
                    slots[d] = pad.AddComponent<TurretMount>(); slots[d].Configure(wagon.Id + "-pool-" + (d + 1), controller);
                }
                racks[w] = wagon.gameObject.AddComponent<TurretRack>(); racks[w].Configure(slots, wagon.Geometry.Interior);
            }
            var manager = new GameObject("Run defenses - wagon mounts and gameplay gate").AddComponent<RunDefenses>();
            manager.Configure(run, registry, racks, types); economy.ConfigureDefenses(manager);
            AddProduct(economy.Catalog, "turret-normal", "Normal turret", 90, 0);
            AddProduct(economy.Catalog, "turret-advanced", "Advanced turret", 160, 1);
            new GameObject("Turret acceptance - F12").AddComponent<TurretAcceptance>().Configure(run, economy, spawner, manager, registry);
            Export(racks, run);
        }
        private static TurretDefinition Type(string name, int ammo, float interval, float range, float blast, float radius, Color color)
        {
            string path = Folder + "/" + name + ".asset";
            var type = AssetDatabase.LoadAssetAtPath<TurretDefinition>(path); if (type != null) return type;
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>(); weapon.displayName = name + " turret";
            weapon.damage = 4; weapon.fireInterval = interval; weapon.range = range;
            weapon.magazineSize = ammo; weapon.infiniteReserve = false; weapon.initialReserve = 0; weapon.tracerColor = color;
            AssetDatabase.CreateAsset(weapon, Folder + "/" + name + "Weapon.asset");
            type = ScriptableObject.CreateInstance<TurretDefinition>(); type.id = name.ToLowerInvariant(); type.weapon = weapon;
            type.color = color; type.blastDamage = blast; type.blastRadius = radius; AssetDatabase.CreateAsset(type, path); return type;
        }
        private static GameObject Shape(string name, PrimitiveType shape, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var obj = GameObject.CreatePrimitive(shape); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            Object.DestroyImmediate(obj.GetComponent<Collider>()); obj.GetComponent<Renderer>().sharedMaterial = material; return obj;
        }
        private static void AddProduct(ShopCatalog catalog, string id, string label, int price, int type)
        {
            if (catalog.items.Any(i => i.id == id)) return;
            catalog.items = catalog.items.Concat(new[] { new ShopItem { id = id, label = label, price = price,
                effect = ShopEffect.Turret, turretIndex = type } }).ToArray(); EditorUtility.SetDirty(catalog);
        }
        private static void Export(TurretRack[] racks, RunDriver run)
        {
            var csv = new System.Text.StringBuilder("wagon,poolSlot,placement,bodyWidth,bodyHeight,bodyDepth,muzzleFloorY,navWidth,navHeight,navDepth\n");
            for (int w = 0; w < racks.Length; w++) foreach (var mount in racks[w].Mounts)
            {
                csv.Append(run.Wagons[w].Id).Append(',').Append(mount.Id).Append(",buyerXZ_at_purchase");
                foreach (float v in new[] { .22f, .6f, .22f, .95f, .24f, .7f, .24f }) csv.Append(',').Append(v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                csv.Append('\n');
            }
            System.IO.Directory.CreateDirectory("docs/generated"); System.IO.File.WriteAllText("docs/generated/turret-mount-dimensions.csv", csv.ToString());
        }
    }
}
