using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.AI.Navigation;
using WaitYourTurn.Run;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    /// <summary>One-time, repeatable authoring only. Never rebuilds physics/nav or modifies Battle.</summary>
    public static class SurvivalTrainArtInstaller
    {
        private const string Folder = "Assets/Game/Art/SurvivalTrain";
        [Serializable] private sealed class Payload { public Part[] parts = Array.Empty<Part>(); }
        [Serializable] private sealed class Part
        { public string name = string.Empty; public float[] vertices = Array.Empty<float>(), normals = Array.Empty<float>(), uv = Array.Empty<float>(); public int[] triangles = Array.Empty<int>(); }

        [MenuItem("Wait Your Turn/Survival/Install Textured Train Art %#&l")]
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play before installing art.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != TrainIntegrationBuilder.SurvivalScenePath) throw new InvalidOperationException("Open SurvivalIntegration; Battle is not an art installation target.");
            var run = UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if (run == null || run.Wagons.Length != 3 || !run.UsesOpenTrainSurvival) throw new InvalidOperationException("Expected three open Survival wagons.");
            var parent = run.Wagons[0].transform.parent;
            string before = GeometryKey(parent); int oldRenderers = EnabledRenderers(parent);
            var surface = parent.GetComponent<NavMeshSurface>(); var nav = surface.navMeshData;
            string navKey = AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(nav)).ToString();
            var data = JsonUtility.FromJson<Payload>(File.ReadAllText(Folder + "/SourceData/TrainMeshes.json"));
            Directory.CreateDirectory(Folder + "/Meshes"); Directory.CreateDirectory(Folder + "/Materials"); AssetDatabase.Refresh();
            var meshes = data.parts.ToDictionary(p => p.name, ImportMesh);
            var steel = Steel(); var glass = Glass("Glass", new Color(.12f,.24f,.27f,.12f));
            var doorGlass = Glass("DoorGlass", new Color(.24f,.13f,.12f,.12f));
            for (int i = 0; i < run.Wagons.Length; i++)
            {
                var wagon = run.Wagons[i]; string kind = "Wagon" + wagon.Doors.Length;
                UpdateShotApertures(wagon);
                var old = wagon.transform.Find("Replaceable visuals - no gameplay ownership");
                foreach (var r in old.GetComponentsInChildren<MeshRenderer>(true)) { Undo.RecordObject(r, "Replace blockout train renderer"); r.enabled = false; }
                foreach (var t in wagon.GetComponentsInChildren<Transform>(true))
                    if (t.name == "Internal end jamb" || t.name == "Internal end header")
                        if (t.TryGetComponent<MeshRenderer>(out var r)) { Undo.RecordObject(r, "Replace end frame renderer"); r.enabled = false; }
                var art = Fresh(wagon.transform, "Textured train art - visuals only");
                Model(art, "Body", meshes[kind], steel); Model(art, "Windows", meshes[kind + "Glass"], glass);
                foreach (var door in wagon.Doors)
                {
                    foreach (var name in new[] { "Lower panel", "Shoot-through glass", "Repair floor strip" })
                    {
                        var oldPanel = door.transform.Find(name);
                        if (oldPanel.TryGetComponent<MeshRenderer>(out var r)) { Undo.RecordObject(r,"Replace door renderer"); r.enabled = false; }
                    }
                    var binding = door.GetComponent<TrainDoorVisual>(); if (binding != null) UnityEngine.Object.DestroyImmediate(binding);
                    var visual = Fresh(door.transform, "Textured door art - same portal");
                    Model(visual, "Red leaves", meshes["DoorLower"], steel); Model(visual, "Clear shot window", meshes["DoorGlass"], doorGlass);
                    door.gameObject.AddComponent<TrainDoorVisual>().Configure(door.Portal, visual.gameObject);
                }
            }
            foreach (var connection in run.Solo.Connections)
            {
                foreach (var r in connection.GetComponentsInChildren<MeshRenderer>(true)) r.enabled = false;
                var art = Fresh(connection.transform, "Textured gangway - permanently open"); Model(art,"Bellows and floor",meshes["Connector"],steel);
            }
            var cab = parent.Cast<Transform>().Single(t => t.name.StartsWith("Locomotive -"));
            foreach (var r in cab.GetComponentsInChildren<MeshRenderer>(true)) r.enabled = false;
            Model(Fresh(cab,"Textured control room - visual only"),"Closed locomotive",meshes["Locomotive"],steel);

            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom; RenderSettings.customReflectionTexture = Reflection();
            RenderSettings.reflectionIntensity = .75f;
            // A small static environment texture, not a realtime reflection probe/light.
            if (GeometryKey(parent) != before || surface.navMeshData != nav ||
                AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(nav)).ToString() != navKey)
                throw new InvalidOperationException("Art installation altered movement/nav; do not save.");
            TrainIntegrationBuilder.ExportModelGuide(run.Wagons, true);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            int triangleCount = meshes["Wagon6"].triangles.Length/3 + meshes["Wagon5"].triangles.Length/3 + meshes["Wagon4"].triangles.Length/3 +
                meshes["Wagon6Glass"].triangles.Length/3 + meshes["Wagon5Glass"].triangles.Length/3 + meshes["Wagon4Glass"].triangles.Length/3 +
                15*(meshes["DoorLower"].triangles.Length+meshes["DoorGlass"].triangles.Length)/3 +
                2*meshes["Connector"].triangles.Length/3 + meshes["Locomotive"].triangles.Length/3;
            foreach(var mesh in meshes.Values) { mesh.UploadMeshData(true); EditorUtility.SetDirty(mesh); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene);
            string report = DateTime.UtcNow.ToString("O") + "\nPASS: unchanged movement collider/agent/obstacle geometry and nav asset; narrower shot apertures match art.\n" +
                $"Train rendered triangles: {triangleCount}; geometry renderers before: {oldRenderers}, after: {EnabledRenderers(parent)}.\n" +
                "Shared atlas albedo2048 ASTC6x6; normal1024 ASTC6x6; packed metal/smooth512 ASTC8x8. Mipmaps; Read/Write disabled.\n" +
                "Three bodies, shared door pair15 instances, two open gangways, closed non-gameplay control room. No lights, probes or navigation added.\n" +
                "Battle scene untouched. Device FPS/thermal acceptance remains pending.\n";
            File.WriteAllText("docs/generated/survival-train-art-install.txt",report); Debug.Log("[TrainArt] " + report);
        }

        private static Mesh ImportMesh(Part p)
        {
            var mesh = new Mesh { name = p.name };
            var vertices = new Vector3[p.vertices.Length/3]; var normals = new Vector3[vertices.Length]; var uv = new Vector2[vertices.Length];
            for (int i=0;i<vertices.Length;i++)
            { vertices[i]=new Vector3(p.vertices[i*3],p.vertices[i*3+1],p.vertices[i*3+2]); normals[i]=new Vector3(p.normals[i*3],p.normals[i*3+1],p.normals[i*3+2]); uv[i]=new Vector2(p.uv[i*2],p.uv[i*2+1]); }
            if (vertices.Length > 65535) throw new InvalidOperationException("Train mesh exceeds mobile UInt16 vertex budget.");
            mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.triangles=p.triangles;
            mesh.RecalculateBounds();mesh.RecalculateTangents(); MeshUtility.Optimize(mesh);
            string path=Folder+"/Meshes/"+p.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old==null) AssetDatabase.CreateAsset(mesh,path);
            else { EditorUtility.CopySerialized(mesh,old); UnityEngine.Object.DestroyImmediate(mesh); mesh=old; EditorUtility.SetDirty(mesh); }
            // Installation counts triangles, then finalizes all meshes with Read/Write disabled.
            return mesh;
        }
        private static Texture2D Texture(string name,int size,TextureImporterType type,TextureImporterFormat format)
        {
            string path=Folder+"/Textures/"+name;var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=type;importer.isReadable=false;importer.mipmapEnabled=true;importer.maxTextureSize=size;
            importer.npotScale=TextureImporterNPOTScale.ToLarger;
            importer.sRGBTexture=type!=TextureImporterType.NormalMap && name.Contains("Albedo");
            importer.wrapMode=TextureWrapMode.Clamp;importer.anisoLevel=2;
            var android=importer.GetPlatformTextureSettings("Android");android.name="Android";android.overridden=true;
            android.maxTextureSize=size;android.format=format;android.compressionQuality=50;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        private static Material Steel()
        {
            var m=Mat("WornMetroSteel","Standard"); m.color=Color.white;m.mainTexture=Texture("TrainAtlas_Albedo.jpg",2048,TextureImporterType.Default,TextureImporterFormat.ASTC_6x6);
            m.SetTexture("_MetallicGlossMap",Texture("TrainAtlas_MetalSmooth.png",512,TextureImporterType.Default,TextureImporterFormat.ASTC_8x8));m.EnableKeyword("_METALLICGLOSSMAP");
            m.SetTexture("_BumpMap",Texture("TrainAtlas_Normal.png",1024,TextureImporterType.NormalMap,TextureImporterFormat.ASTC_6x6));m.SetFloat("_BumpScale",.4f);m.EnableKeyword("_NORMALMAP");
            m.SetFloat("_GlossMapScale",.65f);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
        }
        private static Material Glass(string name,Color color)
        { var m=Mat(name,"WaitYourTurn/TrainGlass");m.color=color;m.enableInstancing=true;EditorUtility.SetDirty(m);return m; }
        private static Material Mat(string name,string shader)
        { string p=Folder+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(m==null){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,p);}return m; }
        private static Transform Fresh(Transform parent,string name)
        { var old=parent.Find(name);if(old!=null) UnityEngine.Object.DestroyImmediate(old.gameObject);var root=new GameObject(name);root.transform.SetParent(parent,false);return root.transform; }
        private static void Model(Transform parent,string name,Mesh mesh,Material material)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
            r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;
        }
        private static int EnabledRenderers(Transform root) => root.GetComponentsInChildren<MeshRenderer>(true).Count(r=>r.enabled && r.gameObject.activeInHierarchy);
        private static void UpdateShotApertures(WagonRuntime wagon)
        {
            foreach(var t in wagon.GetComponentsInChildren<Transform>(true))
            {
                if(t.name=="Window lower metal collider")
                { var p=t.localPosition;p.y=.375f;t.localPosition=p;var s=t.localScale;s.y=.75f;t.localScale=s; }
                if(t.name=="Window upper metal collider")
                { var p=t.localPosition;p.y=1.825f;t.localPosition=p;var s=t.localScale;s.y=.55f;t.localScale=s; }
            }
            foreach(var w in wagon.GetComponentsInChildren<WagonWindow>(true))
            { var p=w.transform.localPosition;p.y=1.15f;w.transform.localPosition=p;var s=w.ClearSize;s.y=.8f;w.Configure(s,w.FrameWidth); }
            foreach(var door in wagon.Doors)
            {
                var lower=door.transform.Find("Lower panel");lower.localPosition=new Vector3(0,.36f,0);lower.localScale=new Vector3(1.45f,.72f,.16f);
                var old=lower.Find("Authored leaf shot frame");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
                var frames=new GameObject("Authored leaf shot frame").transform;frames.SetParent(lower,false);
                foreach(var spec in new[] { new Vector4(-.66f,1.36f,.13f,1.28f), new Vector4(.66f,1.36f,.13f,1.28f), new Vector4(0,1.84f,1.45f,.32f), new Vector4(0,1.44f,.035f,.98f) })
                {
                    var go=new GameObject("Metal leaf frame collider");go.transform.SetParent(frames,false);
                    go.transform.position=door.transform.TransformPoint(new Vector3(spec.x,spec.y,0));go.transform.rotation=door.transform.rotation;
                    go.transform.localScale=new Vector3(spec.z/lower.localScale.x,spec.w/lower.localScale.y,1);
                    go.AddComponent<BoxCollider>();go.AddComponent<NavMeshModifier>().ignoreFromBuild=true;
                }
            }
        }
        private static string GeometryKey(Transform root)
        {
            var data=new StringBuilder();
            foreach(var c in root.GetComponentsInChildren<Collider>(true))
            {
                if(c.TryGetComponent<NavMeshModifier>(out var modifier)&&modifier.ignoreFromBuild)continue;
                data.Append(c.GetEntityId().ToString()).Append(EditorJsonUtility.ToJson(c)).Append(c.transform.localToWorldMatrix);
            }
            foreach(var c in root.GetComponentsInChildren<UnityEngine.AI.NavMeshObstacle>(true))data.Append(c.GetEntityId().ToString()).Append(EditorJsonUtility.ToJson(c));
            foreach(var c in root.GetComponentsInChildren<UnityEngine.AI.NavMeshAgent>(true))data.Append(c.GetEntityId().ToString()).Append(EditorJsonUtility.ToJson(c));
            using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(data.ToString())));
        }
        private static Cubemap Reflection()
        {
            string path=Folder+"/Materials/StaticSteelEnvironment.asset";var old=AssetDatabase.LoadAssetAtPath<Cubemap>(path);if(old!=null)return old;
            var cube=new Cubemap(64,TextureFormat.RGB24,true){name="StaticSteelEnvironment",wrapMode=TextureWrapMode.Clamp};
            for(int face=0;face<6;face++)
            {
                var colors=new Color[64*64];
                for(int y=0;y<64;y++)for(int x=0;x<64;x++)
                {
                    float t=y/63f;var c=Color.Lerp(new Color(.13f,.15f,.18f),new Color(.66f,.69f,.7f),t);
                    float highlight=Mathf.Pow(Mathf.Max(0,1-Mathf.Abs(x/63f-.42f)*6),4)*.35f;
                    if(face==2)c=new Color(.72f,.74f,.75f);if(face==3)c=new Color(.1f,.11f,.12f);
                    colors[y*64+x]=c+new Color(highlight,highlight*.92f,highlight*.8f);
                }
                cube.SetPixels(colors,(CubemapFace)face);
            }
            cube.Apply(true,true);AssetDatabase.CreateAsset(cube,path);return cube;
        }
    }
}
