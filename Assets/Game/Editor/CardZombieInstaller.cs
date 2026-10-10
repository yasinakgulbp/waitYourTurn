using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using WaitYourTurn.Enemies;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    /// <summary>Survival visual adapter; shared enemy/nav/combat authorities remain untouched.</summary>
    public static class CardZombieInstaller
    {
        private const string Folder = "Assets/Game/Art/CardZombies";
        [Serializable] private sealed class Payload { public Bone[] bones; public Part[] parts; }
        [Serializable] private sealed class Bone { public string name; public int parent; public float[] position; }
        [Serializable] private sealed class Part
        { public string name; public float scale; public float[] vertices, normals, colors, blends; public int[] weights, secondWeights, triangles; }
        [InitializeOnLoadMethod] private static void Listen()
        { EditorApplication.update -= Consume; EditorApplication.update += Consume; }
        private static void Consume()
        {
            const string request = "Temp/card-zombies.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            string command;
            try { command = File.ReadAllText(request).Trim(); File.Delete(request); } catch (IOException) { return; }
            try
            {
                if (command == "install") Install();
                else if (command == "check") CardZombieChecks.Start();
                else if (command == "combatcheck") CardContactChecks.Start();
                else throw new InvalidOperationException("Unknown card zombie request");
            }
            catch (Exception e) { File.WriteAllText("docs/generated/card-zombies-error.txt", e.ToString()); Debug.LogException(e); }
        }
        [MenuItem("Wait Your Turn/Survival/Install Card Zombies")]
        public static void Install()
        {
            var scene = SceneManager.GetActiveScene();
            if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != TrainIntegrationBuilder.SurvivalScenePath)
                throw new InvalidOperationException("Stopped Survival scene required");
            string physics = MaquetteArtInstaller.GameplayKey(scene);
            var data = JsonUtility.FromJson<Payload>(File.ReadAllText(Folder + "/SourceData/CardZombies.json"));
            if (data.bones.Length != 15 || data.parts.Length != 4) throw new InvalidOperationException("Invalid card rig");
            Directory.CreateDirectory(Folder + "/Meshes"); AssetDatabase.Refresh();
            var meshes = data.parts.ToDictionary(p => p.name, p => MeshAsset(p, data.bones));
            if (AssetDatabase.IsValidFolder(Folder + "/__MeshRebuild")) AssetDatabase.DeleteAsset(Folder + "/__MeshRebuild");
            var template = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<EnemyBrain>(true)).Single();
            foreach (var r in template.GetComponentsInChildren<MeshRenderer>(true)) r.enabled = false;
            var root = template.transform.Find("Animated card body - visual only");
            if (root != null) UnityEngine.Object.DestroyImmediate(root.gameObject);
            var art = CreateArt(template.transform, meshes["Normal"], data.bones, out var skin, out var skeleton);
            art.gameObject.AddComponent<CardZombieVisual>().Configure(template, skin, skeleton, meshes["Normal"], meshes["Fast"], meshes["Tough"]);
            // Boss has an editable model/rig; no new boss wave or hitbox is invented here.
            var film = Camera.main.GetComponent<MaquetteCinema>(); film.BlurAmount = .70f; EditorUtility.SetDirty(film);
            if (MaquetteArtInstaller.GameplayKey(scene) != physics) throw new InvalidOperationException("Physics changed; do not save");
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            File.WriteAllText("docs/generated/card-zombies-install.txt", DateTime.UtcNow.ToString("O") +
                "\nPASS: continuous faceted card monsters; 15 bones, two-weight joints and one material/SkinnedMeshRenderer per actor.\n" +
                string.Join("\n", data.parts.Select(p => p.name + ": " + p.triangles.Length / 3 + " triangles; visual scale " + p.scale)) +
                "\nSurvival template installed; Normal/Intro, Fast and Tough retain existing profiles. Boss art prepared only.\n" +
                "All existing collider/agent/obstacle state and transforms unchanged. No root motion, extra lights, textures or Animator.\n" +
                "Type-specific gait/anticipation/contact/recovery/hit; three bounded collider-free death skins per wagon.\n" +
                "Player-contact reach: Fast .82m, Normal .92m, Tough 1.02m; door reservation range retained.\n");
            Debug.Log("[CardZombie] Installed faceted monsters and contact-driven animation.");
        }
        private static Vector3 V(float[] a, int i = 0) => new Vector3(a[i], a[i + 1], a[i + 2]);
        private static Mesh MeshAsset(Part part, Bone[] bones)
        {
            int count = part.vertices.Length / 3;
            if (count > 65535 || part.weights.Length != count || part.normals.Length != count * 3 || part.colors.Length != count * 4)
                throw new InvalidOperationException("Invalid mesh " + part.name);
            var mesh = new Mesh { name = part.name };
            var v = new Vector3[count]; var n = new Vector3[count]; var c = new Color32[count]; var w = new BoneWeight[count];
            for (int i = 0; i < count; i++)
            {
                v[i] = V(part.vertices, i * 3); n[i] = V(part.normals, i * 3);
                c[i] = new Color(part.colors[i * 4], part.colors[i * 4 + 1], part.colors[i * 4 + 2], 1);
                if (part.weights[i] < 0 || part.weights[i] >= bones.Length) throw new InvalidOperationException("Invalid hinge weight");
                w[i] = new BoneWeight { boneIndex0 = part.weights[i], weight0 = 1 };
                if (part.secondWeights != null && part.secondWeights.Length == count && part.blends != null && part.blends.Length == count)
                    w[i] = new BoneWeight { boneIndex0 = part.weights[i], weight0 = part.blends[i], boneIndex1 = part.secondWeights[i], weight1 = 1 - part.blends[i] };
            }
            mesh.vertices = v; mesh.normals = n; mesh.colors32 = c; mesh.triangles = part.triangles;
            mesh.boneWeights = w; mesh.bindposes = bones.Select(b => Matrix4x4.Translate(V(b.position)).inverse).ToArray();
            mesh.RecalculateBounds();
            string path = Folder + "/Meshes/" + part.name + ".asset";
            if (!File.Exists(path))
            {
                AssetDatabase.CreateAsset(mesh, path);
                mesh.UploadMeshData(true); EditorUtility.SetDirty(mesh); return mesh;
            }
            // CopySerialized cannot replace native buffers of an already uploaded unreadable mesh.
            // Serialize a fresh mesh and replace only the asset data; its .meta/GUID stays intact.
            if (!AssetDatabase.IsValidFolder(Folder + "/__MeshRebuild")) AssetDatabase.CreateFolder(Folder, "__MeshRebuild");
            string staging = Folder + "/__MeshRebuild/" + part.name + ".asset";
            AssetDatabase.CreateAsset(mesh, staging);
            mesh.UploadMeshData(true); EditorUtility.SetDirty(mesh); AssetDatabase.SaveAssets();
            File.Copy(staging, path, true);
            AssetDatabase.DeleteAsset(staging);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh.vertexCount != count) throw new InvalidOperationException("Native mesh buffer mismatch: " + part.name);
            return mesh;
        }
        private static Transform CreateArt(Transform parent, Mesh mesh, Bone[] bones, out SkinnedMeshRenderer skin, out Transform[] skeleton)
        {
            var root = new GameObject("Animated card body - visual only").transform; root.SetParent(parent, false);
            skeleton = new Transform[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                var b = new GameObject(bones[i].name).transform;
                b.SetParent(bones[i].parent < 0 ? root : skeleton[bones[i].parent], false);
                b.localPosition = V(bones[i].position) - (bones[i].parent < 0 ? Vector3.zero : V(bones[bones[i].parent].position));
                skeleton[i] = b;
            }
            skin = root.gameObject.AddComponent<SkinnedMeshRenderer>(); skin.sharedMesh = mesh;
            skin.bones = skeleton; skin.rootBone = skeleton[0]; skin.quality = SkinQuality.Bone2;
            const string materialPath = Folder + "/ZombieCard.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Art/Maquette/Materials/ModelCard.mat"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            // Actor fill keeps close attackers readable outside the flashlight, as agreed.
            material.SetColor("_Ambient", new Color(.55f, .52f, .56f)); EditorUtility.SetDirty(material);
            skin.sharedMaterial = material;
            skin.shadowCastingMode = ShadowCastingMode.Off; skin.receiveShadows = true; skin.updateWhenOffscreen = false;
            // Bounds are rootBone-local, padded for in-place arms/legs without endless culling extents.
            skin.localBounds = new Bounds(new Vector3(0, .12f, .05f), new Vector3(1.4f, 2.15f, 1.9f));
            return root;
        }
    }
}
