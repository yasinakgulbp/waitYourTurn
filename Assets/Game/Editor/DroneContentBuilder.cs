using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;
using WaitYourTurn.Economy;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class DroneContentBuilder
    {
        private const string Folder = "Assets/Game/Content/Defenses";
        [MenuItem("Wait Your Turn/Defenses/Install Drone In Integration %#F1")]
        public static void InstallCurrent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != TrainIntegrationBuilder.ScenePath) return;
            var run = Object.FindAnyObjectByType<RunDriver>();
            if (run == null || Object.FindAnyObjectByType<RunDrone>() != null) return;
            Install(run, Object.FindAnyObjectByType<TargetRegistry>(), Object.FindAnyObjectByType<RunEconomy>(), Object.FindAnyObjectByType<StationSpawner>());
            EditorSceneManager.SaveScene(run.gameObject.scene); AssetDatabase.SaveAssets();
            Debug.Log("[Drone] Installed one reusable companion and shop product; geometry and navigation preserved.");
        }
        public static void Install(RunDriver run, TargetRegistry registry, RunEconomy economy, StationSpawner spawner)
        {
            var data = AssetDatabase.LoadAssetAtPath<DroneDefinition>(Folder + "/Drone.asset");
            if (data == null)
            {
                var gun = ScriptableObject.CreateInstance<WeaponDefinition>(); gun.displayName = "Drone";
                gun.damage = 3; gun.fireInterval = .45f; gun.range = 9; gun.magazineSize = 60;
                gun.infiniteReserve = false; gun.initialReserve = 0; gun.tracerColor = Color.cyan;
                AssetDatabase.CreateAsset(gun, Folder + "/DroneWeapon.asset");
                data = ScriptableObject.CreateInstance<DroneDefinition>(); data.weapon = gun;
                AssetDatabase.CreateAsset(data, Folder + "/Drone.asset");
            }
            var actor = new GameObject("Reusable player drone - F8 acceptance"); actor.SetActive(false);
            var life = actor.AddComponent<HealthComponent>();
            var weapon = actor.AddComponent<HitscanWeapon>(); weapon.SetHitMask(run.Weapon.HitMask);
            var socket = new GameObject("Mechanical muzzle - root Y minus 0.18").transform;
            socket.SetParent(actor.transform, false); socket.localPosition = Vector3.down * .18f; weapon.ConfigureMuzzle(socket);
            var visual = new GameObject("Replaceable drone visual - bob only").transform; visual.SetParent(actor.transform, false);
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/IntegrationBlockout/Trim.mat");
            var light = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Tracer.mat");
            var accent = Shape("Body", PrimitiveType.Cube, visual, Vector3.zero, new Vector3(.3f, .14f, .3f), mat).GetComponent<Renderer>();
            var blades = new Transform[4];
            for (int i = 0; i < blades.Length; i++)
            {
                Vector3 at = new Vector3(i % 2 == 0 ? -.23f : .23f, .06f, i < 2 ? -.23f : .23f);
                Shape("Rotor arm", PrimitiveType.Cube, visual, at * .5f, new Vector3(.3f, .04f, .05f), mat).transform.localRotation = Quaternion.Euler(0, i % 2 == 0 ? -45 : 45, 0);
                blades[i] = Shape("Rotor " + i, PrimitiveType.Cube, visual, at, new Vector3(.2f, .025f, .045f), light).transform;
            }
            var head = new GameObject("Aiming head").transform; head.SetParent(visual, false);
            Shape("Barrel", PrimitiveType.Cube, head, new Vector3(0, -.18f, .13f), new Vector3(.07f, .07f, .26f), mat);
            var trace = actor.AddComponent<LineRenderer>(); trace.useWorldSpace = true; trace.positionCount = 2;
            trace.startWidth = trace.endWidth = .025f; trace.sharedMaterial = light; trace.enabled = false;
            var burst = Shape("Reusable kamikaze burst", PrimitiveType.Sphere, actor.transform, Vector3.zero, Vector3.one * .2f, light).transform;
            burst.gameObject.SetActive(false);
            var view = actor.AddComponent<DronePresentation>(); view.Configure(weapon, visual, head, blades, accent, trace, burst);
            var brain = actor.AddComponent<DroneController>(); brain.Configure(life, weapon, view);
            var manager = new GameObject("Run drone - owner companion and gameplay gate").AddComponent<RunDrone>();
            manager.Configure(run, registry, brain, data); economy.ConfigureDrone(manager);
            if (!economy.Catalog.items.Any(i => i.effect == ShopEffect.Drone))
            {
                economy.Catalog.items = economy.Catalog.items.Concat(new[] { new ShopItem { id = "drone", label = "Companion drone", price = 180, effect = ShopEffect.Drone } }).ToArray();
                EditorUtility.SetDirty(economy.Catalog);
            }
            new GameObject("Drone acceptance - F8").AddComponent<DroneAcceptance>().Configure(run, economy, spawner, manager);
            System.IO.Directory.CreateDirectory("docs/generated");
            System.IO.File.WriteAllText("docs/generated/drone-model-dimensions.csv", "unit,visualWidth,visualHeight,visualDepth,flightRadius,hoverRootY,muzzleRootY,muzzleFloorY,bobAmplitude,physicalCollider\nmetres,0.66,0.288,0.66,0.14,1.95,-0.18,1.77,0.05,none_swept_volume\n");
        }
        private static GameObject Shape(string name, PrimitiveType kind, Transform parent, Vector3 at, Vector3 scale, Material mat)
        {
            var obj = GameObject.CreatePrimitive(kind); obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = at; obj.transform.localScale = scale;
            Object.DestroyImmediate(obj.GetComponent<Collider>()); obj.GetComponent<Renderer>().sharedMaterial = mat; return obj;
        }
    }
}
