using System.Collections.Generic;
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
    public static class ExplosivesContentBuilder
    {
        private const string LauncherPath = "Assets/Game/Content/Weapons/GrenadeLauncher.asset";
        [MenuItem("Wait Your Turn/Survival/Install Explosives %&e")]
        public static void InstallCurrent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != TrainIntegrationBuilder.SurvivalScenePath) return;
            var run = Object.FindAnyObjectByType<RunDriver>(); var economy = Object.FindAnyObjectByType<RunEconomy>();
            Install(run, Object.FindAnyObjectByType<TargetRegistry>(), economy);
            EditorSceneManager.MarkSceneDirty(run.gameObject.scene); EditorSceneManager.SaveScene(run.gameObject.scene); AssetDatabase.SaveAssets();
            Debug.Log("[Explosives] Survival launcher and 12 collider-free mines installed. Battle composition unchanged.");
        }
        public static void Install(RunDriver run, TargetRegistry registry, RunEconomy economy)
        {
            var launcher = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(LauncherPath);
            if (launcher == null)
            {
                launcher = ScriptableObject.CreateInstance<WeaponDefinition>(); launcher.displayName = "Grenade launcher";
                launcher.delivery = ShotDelivery.Grenade; launcher.damage = 90; launcher.fireInterval = 1.25f; launcher.range = 12;
                launcher.projectileSpeed = 24;
                launcher.magazineSize = 6; launcher.reloadSeconds = 2.5f; launcher.infiniteReserve = false; launcher.initialReserve = 42;
                launcher.tracerColor = new Color(1, .65f, .1f); AssetDatabase.CreateAsset(launcher, LauncherPath);
            }
            var definitions = new List<WeaponDefinition>();
            for (int i = 0; i < run.Weapon.WeaponCount; i++) definitions.Add(run.Weapon.DefinitionAt(i));
            int index = definitions.IndexOf(launcher);
            if (index < 0) { index = definitions.Count; definitions.Add(launcher); run.Weapon.ConfigureDefinitions(definitions.ToArray()); }
            var dark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/IntegrationBlockout/Trim.mat");
            var glow = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/CombatLab/Tracer.mat");
            const string grenadeMaterialPath = "Assets/Game/Content/IntegrationBlockout/GrenadeOrange.mat";
            var grenadeMaterial = AssetDatabase.LoadAssetAtPath<Material>(grenadeMaterialPath);
            if (grenadeMaterial == null)
            { grenadeMaterial = new Material(glow) { color = new Color(1, .65f, .1f) }; AssetDatabase.CreateAsset(grenadeMaterial, grenadeMaterialPath); }
            if (run.Weapon.Projectiles == null)
            {
                var poolRoot = new GameObject("Reusable grenade delivery - capacity 16"); poolRoot.SetActive(false);
                var views = new GrenadeView[16];
                for (int i = 0; i < views.Length; i++)
                {
                    var visual = Shape("Grenade " + i, PrimitiveType.Sphere, poolRoot.transform, Vector3.zero, Vector3.one * .2f, grenadeMaterial);
                    visual.gameObject.SetActive(false);
                    views[i] = new GrenadeView { body = visual, explosion = Pulse(poolRoot.transform, glow) };
                }
                var delivery = poolRoot.AddComponent<ExplosiveProjectiles>(); delivery.Configure(run.Weapon, run.Player, registry, views);
                poolRoot.SetActive(true);
            }
            if (economy.Mines == null)
            {
                const string minePath = "Assets/Game/Content/Defenses/Mine.asset";
                var mine = AssetDatabase.LoadAssetAtPath<MineDefinition>(minePath);
                if (mine == null) { mine = ScriptableObject.CreateInstance<MineDefinition>(); AssetDatabase.CreateAsset(mine, minePath); }
                var root = new GameObject("Run mine pool - capacity 12"); root.SetActive(false); var actors = new MineController[12];
                for (int i = 0; i < actors.Length; i++)
                {
                    var actor = new GameObject("Reusable mine " + i); actor.transform.SetParent(root.transform, false);
                    var body = Shape("Replaceable small mine body", PrimitiveType.Cylinder, actor.transform, Vector3.up * .055f, new Vector3(.3f, .045f, .3f), dark);
                    var diode = Shape("Blinking red indicator - no light", PrimitiveType.Sphere, actor.transform, Vector3.up * .115f, Vector3.one * .075f, glow);
                    // One shared red material, no dynamic Light component.
                    string redPath = "Assets/Game/Content/IntegrationBlockout/MineRed.mat";
                    var red = AssetDatabase.LoadAssetAtPath<Material>(redPath);
                    if (red == null) { red = new Material(glow) { color = Color.red }; AssetDatabase.CreateAsset(red, redPath); }
                    diode.GetComponent<Renderer>().sharedMaterial = red;
                    var source = actor.AddComponent<HealthComponent>();
                    actors[i] = actor.AddComponent<MineController>(); actors[i].Configure(source, body, diode, Pulse(actor.transform, glow));
                    body.gameObject.SetActive(false); diode.gameObject.SetActive(false);
                }
                var manager = root.AddComponent<RunMines>(); manager.Configure(run, registry, mine, actors);
                economy.ConfigureMines(manager); root.SetActive(true);
            }
            if (!economy.Catalog.items.Any(i => i.id == "launcher"))
                economy.Catalog.items = economy.Catalog.items.Concat(new[] { new ShopItem { id = "launcher", label = "Grenade launcher / refill",
                    price = 6000, effect = ShopEffect.Weapon, weaponIndex = index, requiresWeaponUnlock = false } }).ToArray();
            if (!economy.Catalog.items.Any(i => i.effect == ShopEffect.Mine))
                economy.Catalog.items = economy.Catalog.items.Concat(new[] { new ShopItem { id = "mine", label = "Proximity mine", price = 250, effect = ShopEffect.Mine } }).ToArray();
            ShopContentBuilder.AddAmmoProducts(economy.Catalog);
            EditorUtility.SetDirty(economy.Catalog); EditorUtility.SetDirty(economy); EditorUtility.SetDirty(run.Weapon);
            if (Object.FindAnyObjectByType<ExplosivesAcceptance>() == null)
                new GameObject("Explosives focused acceptance").AddComponent<ExplosivesAcceptance>().Configure(run, economy,
                    Object.FindAnyObjectByType<StationSpawner>(), Object.FindAnyObjectByType<RunPersistence>(), registry);
        }
        public static ExplosionPulse Pulse(Transform parent, Material material)
        {
            var root = new GameObject("Reusable explosion - visual only"); root.transform.SetParent(parent, false);
            var flash = Shape("Flash", PrimitiveType.Sphere, root.transform, Vector3.zero, Vector3.one * .2f, material); flash.gameObject.SetActive(false);
            var ring = new GameObject("Expanding blast ring").AddComponent<LineRenderer>(); ring.transform.SetParent(root.transform, false);
            ring.useWorldSpace = false; ring.loop = true; ring.positionCount = 32; ring.startWidth = ring.endWidth = .055f;
            ring.sharedMaterial = material; ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            for (int i = 0; i < 32; i++) { float a = i * Mathf.PI * 2 / 32; ring.SetPosition(i, new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a))); }
            ring.enabled = false; var pulse = root.AddComponent<ExplosionPulse>(); pulse.Configure(flash, ring); return pulse;
        }
        public static Transform Shape(string name, PrimitiveType kind, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var shape = GameObject.CreatePrimitive(kind); shape.name = name; shape.transform.SetParent(parent, false);
            shape.transform.localPosition = position; shape.transform.localScale = scale; Object.DestroyImmediate(shape.GetComponent<Collider>());
            var renderer = shape.GetComponent<Renderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false; return shape.transform;
        }
        [MenuItem("Wait Your Turn/Survival/Check Explosives %&i")]
        public static void Check()
        { if (!EditorApplication.isPlaying) return; var check = Object.FindAnyObjectByType<ExplosivesAcceptance>(); if (check != null) check.StartCoroutine(check.Check()); }
    }
}
