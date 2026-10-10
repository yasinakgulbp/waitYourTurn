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
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    /// <summary>Survival-only visual authoring. No runtime logic or shared Battle content edits.</summary>
    public static class MaquetteArtInstaller
    {
        private const string Folder="Assets/Game/Art/Maquette";
        private const string Request="Temp/maquette-art.request";
        [Serializable] private sealed class Payload { public Part[] parts; }
        [Serializable] private sealed class Part
        { public string name; public float[] vertices,normals,uv,colors; public int[] triangles; }

        // Explicit local request is consumed once. Never installs on every import.
        [InitializeOnLoadMethod] private static void Listen()
        { EditorApplication.update-=Consume;EditorApplication.update+=Consume; }
        private static void Consume()
        {
            if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            string action;
            try { action=File.ReadAllText(Request).Trim();File.Delete(Request); }
            catch(IOException) { return; } // A writer can still hold the one-shot request this frame.
            try
            {
                if(action=="preview")Preview();
                else if(action=="install")Install();
                else throw new InvalidOperationException("Unknown maquette authoring request");
            }
            catch(Exception e){File.WriteAllText("docs/generated/maquette-error.txt",e.ToString());Debug.LogException(e);}
        }

        [MenuItem("Wait Your Turn/Survival/Install Architectural Maquette %#&m")]
        public static void Install()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play before maquette authoring.");
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=TrainIntegrationBuilder.SurvivalScenePath)throw new InvalidOperationException("Maquette targets SurvivalIntegration only.");
            var run=UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if(run==null||!run.UsesOpenTrainSurvival||run.Wagons.Length!=3)throw new InvalidOperationException("Expected three open Survival wagons.");
            string geometry=GameplayKey(scene);
            var surface=run.Wagons[0].transform.parent.GetComponent<NavMeshSurface>();
            var nav=surface.navMeshData;
            var navHash=AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(nav));
            var payload=JsonUtility.FromJson<Payload>(File.ReadAllText(Folder+"/SourceData/MaquetteMeshes.json"));
            Directory.CreateDirectory(Folder+"/Meshes");Directory.CreateDirectory(Folder+"/Materials");Directory.CreateDirectory(Folder+"/Textures");
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(Folder+"/Card.shader",ImportAssetOptions.ForceUpdate);
            var meshes=payload.parts.Where(p=>p.colors!=null).ToDictionary(p=>p.name,MeshAsset);
            var card=Card("ModelCard",Color.white,true);
            var landscape=Card("EnvironmentCard",Color.white,true);landscape.SetFloat("_Atmosphere",1);EditorUtility.SetDirty(landscape);
            foreach(var wagon in run.Wagons)
            {
                var art=wagon.transform.Find("Textured train art - visuals only");
                Apply(art.Find("Body"),meshes["Wagon"+wagon.Doors.Length],card);
                foreach(var door in wagon.Doors)Apply(door.transform.Find("Textured door art - same portal/Red leaves"),meshes["DoorLower"],card);
            }
            foreach(var passage in run.Solo.Connections)
                Apply(passage.transform.Find("Textured gangway - permanently open/Bellows and floor"),meshes["Connector"],card);
            var train=run.Wagons[0].transform.parent;
            var cab=train.Cast<Transform>().Single(t=>t.name.StartsWith("Locomotive -"));
            Apply(cab.Find("Textured control room - visual only/Closed locomotive"),meshes["Locomotive"],card);
            var presentation=UnityEngine.Object.FindAnyObjectByType<RunPresentation>();
            Replace(presentation.TrackVisuals,new[]{"TrackCard"},new[]{Vector3.zero});
            Replace(presentation.Scenery,new[]{"SceneryCard","SceneryInk"},new[]{-40f,-20f,0f,20f,40f,60f}.Select(x=>Vector3.right*x).ToArray());
            Replace(presentation.StationVisuals,new[]{"StationCard","StationInk"},new[]{Vector3.zero});
            var incoming=scene.GetRootGameObjects().Single(g=>g.name=="Incoming station - visual only");
            Replace(incoming.transform,new[]{"StationCard","StationInk"},new[]{Vector3.zero});

            // Scene-local template materials also carry through pooled instances.
            // Existing emissive tracers/LEDs and transparent glass stay functional.
            var materials=new Dictionary<Material,Material>();
            foreach(var root in scene.GetRootGameObjects())foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(renderer.sharedMaterial==null||renderer.sharedMaterial.shader.name!="Standard")continue;
                var source=renderer.sharedMaterial;
                string name=source.name;
                if(name=="Player")Assign(Card("PlayerCard",new Color(.36f,.63f,.53f),false));
                else if(name=="Enemy")Assign(Card("EnemyCard",new Color(.63f,.31f,.29f),false));
                else if(name=="Trim"||name=="Wall"||name=="SurvivalWood"||name=="SurvivalWire")
                {
                    if(!materials.TryGetValue(source,out var replacement))
                    { replacement=Card("Prop_"+name,name=="Wall"?new Color(.72f,.75f,.77f):new Color(.42f,.45f,.48f),false);materials.Add(source,replacement); }
                    Assign(replacement);
                }
                void Assign(Material mat){Undo.RecordObject(renderer,"Paper prop material");renderer.sharedMaterial=mat;}
            }
            var light=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Light>(true)).Single(l=>l.type==LightType.Directional);
            Undo.RecordObject(light,"Maquette studio key");Undo.RecordObject(light.transform,"Maquette studio direction");
            light.color=new Color(1f,.96f,.92f);light.intensity=.95f;light.shadows=LightShadows.None;
            light.transform.rotation=Quaternion.Euler(55,-35,0);
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.30f,.34f,.40f);
            RenderSettings.ambientEquatorColor=new Color(.16f,.20f,.25f);RenderSettings.ambientGroundColor=new Color(.09f,.11f,.14f);
            RenderSettings.reflectionIntensity=0;
            RenderSettings.fog=false; // Depth shading is limited to the card environment shader.
            Camera.main.backgroundColor=new Color(.018f,.025f,.034f);
            if(GameplayKey(scene)!=geometry||surface.navMeshData!=nav||AssetDatabase.GetAssetDependencyHash(AssetDatabase.GetAssetPath(nav))!=navHash)
                throw new InvalidOperationException("Visual authoring changed gameplay data; do not save.");
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            var triangles=payload.parts.ToDictionary(p=>p.name,p=>p.triangles.Length/3);
            int trainTriangles=triangles["Wagon6"]+triangles["Wagon5"]+triangles["Wagon4"]+15*triangles["DoorLower"]+2*triangles["Connector"]+triangles["Locomotive"];
            File.WriteAllText("docs/generated/maquette-install.txt",DateTime.UtcNow.ToString("O")+
                $"\nPASS: Survival-only maquette. All collider/agent/obstacle state and transforms unchanged; same NavMesh asset/hash.\nTrain opaque triangles {trainTriangles}. One directional light without realtime shadows; no postprocess or extra camera.\n"+
                "Combined, bounded 20m card scenery; existing RunPresentation owns station/track/scenery motion. Only 64px grain map; no reference imagery copied.\nBattle unchanged. User visual and Android acceptance pending.\n");
            Debug.Log("[Maquette] Installed original card model art; gameplay and NavMesh unchanged.");
            Preview();
            void Replace(Transform parent,string[] names,Vector3[] offsets)
            {
                // Never disable the roots: RunPresentation moves/activates them.
                foreach(var r in parent.GetComponentsInChildren<MeshRenderer>(true))r.enabled=false;
                var old=parent.Find("Architectural maquette visuals");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
                var group=new GameObject("Architectural maquette visuals").transform;group.SetParent(parent,false);
                foreach(var offset in offsets)foreach(var name in names)
                {
                    var go=new GameObject(name+" "+offset.x);go.transform.SetParent(group,false);go.transform.localPosition=offset;
                    go.AddComponent<MeshFilter>().sharedMesh=meshes[name];Setup(go.AddComponent<MeshRenderer>(),landscape);
                }
            }
        }

        [MenuItem("Wait Your Turn/Survival/Render Maquette Preview")]
        public static void Preview()
        {
            var camera=Camera.main;if(camera==null)throw new InvalidOperationException("No scene camera.");
            var previous=camera.targetTexture;var active=RenderTexture.active;var rect=camera.rect;
            Vector3 position=camera.transform.position;
            var target=new RenderTexture(1600,900,24){antiAliasing=4};var output=new Texture2D(1600,900,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;camera.rect=new Rect(0,0,1,1);camera.aspect=1600f/900;
                if(!EditorApplication.isPlaying)
                {
                    var presentation=UnityEngine.Object.FindAnyObjectByType<RunPresentation>();var run=UnityEngine.Object.FindAnyObjectByType<RunDriver>();
                    Bounds volume=presentation.FramingVolume;volume.Expand(new Vector3(1.3f,0,0));
                    float d=WagonCameraFraming.Distance(volume,camera.transform.rotation,camera.fieldOfView,camera.aspect,WagonCameraFraming.ProtectedViewport);
                    camera.transform.position=new Vector3(run.Player.transform.position.x,volume.center.y,run.Player.transform.position.z)-camera.transform.forward*d;
                }
                camera.Render();RenderTexture.active=target;output.ReadPixels(new Rect(0,0,1600,900),0,0);output.Apply();
                File.WriteAllBytes("docs/generated/maquette-unity-preview.png",output.EncodeToPNG());
            }
            finally
            {camera.targetTexture=previous;camera.rect=rect;camera.ResetAspect();camera.transform.position=position;RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(output);}
        }
        private static Mesh MeshAsset(Part p)
        {
            int count=p.vertices.Length/3;if(count>65535||p.normals.Length!=count*3||p.colors.Length!=count*4)throw new InvalidOperationException("Invalid maquette payload "+p.name);
            var mesh=new Mesh{name=p.name};var v=new Vector3[count];var n=new Vector3[count];var colors=new Color32[count];var uv=new Vector2[count];
            for(int i=0;i<count;i++)
            {v[i]=new Vector3(p.vertices[i*3],p.vertices[i*3+1],p.vertices[i*3+2]);n[i]=new Vector3(p.normals[i*3],p.normals[i*3+1],p.normals[i*3+2]);colors[i]=new Color(p.colors[i*4],p.colors[i*4+1],p.colors[i*4+2],1);uv[i]=new Vector2(p.uv[i*2],p.uv[i*2+1]);}
            mesh.vertices=v;mesh.normals=n;mesh.colors32=colors;mesh.uv=uv;mesh.triangles=p.triangles;mesh.RecalculateBounds();
            string path=Folder+"/Meshes/"+p.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old==null)AssetDatabase.CreateAsset(mesh,path);
            else{old.Clear();EditorUtility.CopySerialized(mesh,old);old.MarkModified();UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}
            mesh.UploadMeshData(true);EditorUtility.SetDirty(mesh);return mesh;
        }
        private static void Apply(Transform t,Mesh mesh,Material mat)
        {if(t==null)throw new InvalidOperationException("Expected installed train art");Undo.RecordObject(t.GetComponent<MeshFilter>(),"Maquette mesh color");t.GetComponent<MeshFilter>().sharedMesh=mesh;Setup(t.GetComponent<MeshRenderer>(),mat);}
        private static void Setup(MeshRenderer r,Material mat)
        {Undo.RecordObject(r,"Maquette renderer");r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;r.lightProbeUsage=LightProbeUsage.Off;r.reflectionProbeUsage=ReflectionProbeUsage.Off;}
        private static Material Card(string name,Color tint,bool vertex)
        {
            string path=Folder+"/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("WaitYourTurn/MaquetteCard")){name=name};AssetDatabase.CreateAsset(mat,path);}
            mat.color=tint;mat.SetFloat("_VertexTint",vertex?1:0);mat.SetFloat("_Atmosphere",0);mat.SetColor("_Ambient",new Color(.28f,.31f,.35f));mat.enableInstancing=true;
            string grainPath=Folder+"/Textures/CardGrain.png";
            if(!File.Exists(grainPath))
            {
                var grain=new Texture2D(64,64,TextureFormat.RGB24,false);var rng=new System.Random(817);
                var colors=new Color[4096];for(int i=0;i<colors.Length;i++){float g=(float)rng.NextDouble();colors[i]=new Color(g,g,g,1);}
                grain.SetPixels(colors);grain.Apply();File.WriteAllBytes(grainPath,grain.EncodeToPNG());UnityEngine.Object.DestroyImmediate(grain);AssetDatabase.ImportAsset(grainPath);
                var importer=(TextureImporter)AssetImporter.GetAtPath(grainPath);importer.sRGBTexture=false;importer.mipmapEnabled=true;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Repeat;importer.maxTextureSize=64;
                var android=importer.GetPlatformTextureSettings("Android");android.name="Android";android.overridden=true;android.maxTextureSize=64;android.format=TextureImporterFormat.ASTC_6x6;importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
            }
            mat.SetTexture("_Grain",AssetDatabase.LoadAssetAtPath<Texture2D>(grainPath));EditorUtility.SetDirty(mat);return mat;
        }
        private static string GameplayKey(Scene scene)
        {
            var data=new StringBuilder();
            foreach(var root in scene.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))
            {
                foreach(var component in t.GetComponents<Component>())
                    if(component is Collider||component is UnityEngine.AI.NavMeshAgent||component is UnityEngine.AI.NavMeshObstacle)
                        data.Append(component.GetEntityId().ToString()).Append(EditorJsonUtility.ToJson(component)).Append(t.localToWorldMatrix);
            }
            using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(data.ToString())));
        }
    }
}
