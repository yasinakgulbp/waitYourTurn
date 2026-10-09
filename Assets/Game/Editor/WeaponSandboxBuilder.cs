using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    public static class WeaponSandboxBuilder
    {
        private const string Folder = "Assets/Game/Content/Weapons";
        [MenuItem("Wait Your Turn/Weapons/Prepare Five Wagon Weapons")]
        public static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            RunSandboxBuilder.OpenFive();
            if (EditorSceneManager.GetActiveScene().path != "Assets/Game/Scenes/TrainSandbox.unity") return;
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "Weapons");
            var definitions = new[] {
                Definition("Pistol", 2, .35f, 10, 8, 1.2f, true, 0, 1, 0, Color.yellow),
                Definition("SMG", 2, .12f, 10, 24, 1.5f, false, 168, 1, 0, Color.cyan),
                Definition("Rifle", 5, .28f, 14, 20, 1.8f, false, 140, 1, 0, new Color(1, .5f, .1f)),
                Definition("Shotgun", 2, .85f, 7, 6, 2, false, 42, 6, 9, new Color(1, .3f, .6f)) };
            RunDriver run = Object.FindAnyObjectByType<RunDriver>();
            run.Weapon.ConfigureDefinitions(definitions); EditorUtility.SetDirty(run.Weapon);
            string flashPath = Folder + "/MuzzleFlash.prefab";
            GameObject flashPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(flashPath);
            if (flashPrefab == null)
            {
                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hovl Studio/Toon projectiles/Prefabs/Flash 1.prefab");
                GameObject copy = Object.Instantiate(source); copy.SetActive(false);
                copy.name = "Reusable Hovl muzzle flash (cosmetic)";
                foreach (MonoBehaviour script in copy.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(script);
                foreach (Collider collider in copy.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
                foreach (Rigidbody body in copy.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(body);
                foreach (Light light in copy.GetComponentsInChildren<Light>(true)) Object.DestroyImmediate(light);
                foreach (ParticleSystem ps in copy.GetComponentsInChildren<ParticleSystem>(true))
                { var main = ps.main; main.stopAction = ParticleSystemStopAction.None; main.loop = false;
                    main.playOnAwake = false; main.maxParticles = Mathf.Min(main.maxParticles, 50); }
                copy.SetActive(true); flashPrefab = PrefabUtility.SaveAsPrefabAsset(copy, flashPath); Object.DestroyImmediate(copy);
            }
            if (run.Player.GetComponent<MuzzleFlash>() == null)
            {
                GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(flashPrefab, run.Player.transform);
                visual.transform.localScale = Vector3.one * .25f;
                run.Player.gameObject.AddComponent<MuzzleFlash>().Configure(run.Weapon, visual.GetComponent<ParticleSystem>());
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene()); AssetDatabase.SaveAssets();
            Debug.Log("[Weapons] Four test definitions assigned. Pistol unlimited reserve; other weapons finite. Cosmetic Hovl flash reused without hit scripts/colliders.");
        }
        private static WeaponDefinition Definition(string name, float damage, float interval, float range,
            int magazine, float reload, bool infinite, int reserve, int pellets, float spread, Color color)
        {
            string path = Folder + "/" + name + ".asset";
            WeaponDefinition definition = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (definition != null) return definition; // Preserve tuning on subsequent preparation.
            definition = ScriptableObject.CreateInstance<WeaponDefinition>(); definition.displayName = name;
            definition.damage = damage; definition.fireInterval = interval; definition.range = range;
            definition.magazineSize = magazine; definition.reloadSeconds = reload; definition.infiniteReserve = infinite;
            definition.initialReserve = reserve; definition.pellets = pellets; definition.spreadDegrees = spread;
            definition.tracerColor = color; AssetDatabase.CreateAsset(definition, path); return definition;
        }
    }
}
