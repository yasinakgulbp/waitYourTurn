using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    public static class MaquetteLocalLightingInstaller
    {
        private const string Request="Temp/maquette-lighting.request";
        private const string CookiePath="Assets/Game/Art/Maquette/Textures/FlashlightCookie.png";
        [InitializeOnLoadMethod] private static void Listen()
        { EditorApplication.update-=Consume;EditorApplication.update+=Consume; }
        private static void Consume()
        {
            if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            string command;
            try {command=File.ReadAllText(Request).Trim();File.Delete(Request);}catch(IOException){return;}
            try
            {
                if(command=="install")Install();
                else if(command=="preview")Preview();
                else if(command=="check")SurvivalTrainArtChecks.Start();
                else if(command=="budget")StartBudget();
                else throw new InvalidOperationException("Unknown local lighting command");
            }
            catch(Exception e){File.WriteAllText("docs/generated/local-lighting-error.txt",e.ToString());Debug.LogException(e);}
        }
        [MenuItem("Wait Your Turn/Survival/Install Local Maquette Lights")]
        public static void Install()
        {
            var scene=SceneManager.GetActiveScene();var run=UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if(EditorApplication.isPlayingOrWillChangePlaymode||scene.path!=TrainIntegrationBuilder.SurvivalScenePath||run==null||!run.UsesOpenTrainSurvival)
                throw new InvalidOperationException("Stopped Survival scene required");
            string geometry=MaquetteArtInstaller.GameplayKey(scene);
            var shader=Shader.Find("WaitYourTurn/MaquetteCard");
            var messages=ShaderUtil.GetShaderMessages(shader);
            File.WriteAllLines("docs/generated/local-lighting-shader.txt",messages.Select(m=>m.severity+": "+m.message));
            if(ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Maquette shader has compiler errors; scene left unchanged");
            foreach(var light in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Light>(true)))
                if(light.type==LightType.Directional)light.enabled=false;
            // These materials belong to this visual theme; Battle keeps its existing materials.
            foreach(string path in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Game/Art/Maquette/Materials"}).Select(AssetDatabase.GUIDToAssetPath))
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                mat.SetFloat("_Night",0);mat.SetFloat("_Atmosphere",0);
                var fill=mat.name=="EnvironmentCard"?new Color(.12f,.15f,.20f):
                    mat.name=="PlayerCard"?new Color(.48f,.53f,.58f):
                    mat.name=="EnemyCard"?new Color(.55f,.50f,.50f):new Color(.28f,.32f,.39f);
                mat.SetColor("_Ambient",fill);EditorUtility.SetDirty(mat);
            }
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.12f,.15f,.20f);
            RenderSettings.reflectionIntensity=0;RenderSettings.fog=false;
            Camera.main.backgroundColor=new Color(.008f,.012f,.018f);
            var lampRoot=run.Player.transform.Find("Atmosphere flashlight - no gameplay ownership");
            if(lampRoot==null){lampRoot=new GameObject("Atmosphere flashlight - no gameplay ownership").transform;lampRoot.SetParent(run.Player.transform,false);}
            lampRoot.localPosition=new Vector3(0,1.25f,.32f);lampRoot.localRotation=Quaternion.Euler(16,0,0);
            var spot=lampRoot.GetComponent<Light>();if(spot==null)spot=lampRoot.gameObject.AddComponent<Light>();
            spot.cookie=Cookie();
            var flashlight=lampRoot.GetComponent<MaquetteFlashlight>();
            if(flashlight==null)flashlight=lampRoot.gameObject.AddComponent<MaquetteFlashlight>();
            flashlight.Configure(1.8f);
            // Small warm accents are vertex lights: no extra per-pixel passes or shadow maps.
            foreach(var wagon in run.Wagons)
            {
                var group=wagon.transform.Find("Warm card lamps - no gameplay ownership");
                if(group==null){group=new GameObject("Warm card lamps - no gameplay ownership").transform;group.SetParent(wagon.transform,false);}
                for(int i=0;i<2;i++)
                {
                    var t=group.Find("Warm lamp "+i);
                    if(t==null){t=new GameObject("Warm lamp "+i).transform;t.SetParent(group,false);}
                    t.localPosition=new Vector3(i==0?-4.5f:4.5f,1.7f,i==0?1.6f:-1.6f);
                    var l=t.GetComponent<Light>();if(l==null)l=t.gameObject.AddComponent<Light>();
                    l.type=LightType.Point;l.renderMode=LightRenderMode.ForceVertex;l.shadows=LightShadows.None;
                    l.range=4.5f;l.intensity=.85f;l.color=new Color(1,.74f,.48f);
                }
                foreach(var r in wagon.GetComponentsInChildren<MeshRenderer>(true))
                    if(r.sharedMaterial!=null&&r.sharedMaterial.shader.name=="WaitYourTurn/MaquetteCard")
                    {r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;}
            }
            foreach(var r in run.Player.GetComponentsInChildren<MeshRenderer>(true))r.shadowCastingMode=ShadowCastingMode.Off;
            // Enemy templates and pools also receive the flashlight; no additional enemy lights.
            foreach(var r in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)))
                if(r.sharedMaterial!=null&&r.sharedMaterial.shader.name=="WaitYourTurn/MaquetteCard")r.receiveShadows=true;
            if(MaquetteArtInstaller.GameplayKey(scene)!=geometry)throw new InvalidOperationException("Gameplay geometry changed; do not save");
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            File.WriteAllText("docs/generated/local-lighting-install.txt",DateTime.UtcNow.ToString("O")+
                "\nPASS: Survival geometry fingerprint unchanged. Sun disabled; no world-Z darkness band.\n"+
                "One 128px cookie spotlight attached to actor facing; one512px hard shadow. Six bounded vertex point lights.\n"+
                "No new camera, package, colliders, NavMesh bake, HDR, volumetric fog or pipeline migration. Android acceptance pending.\n");
            Preview();
        }
        private static Texture2D Cookie()
        {
            if(!File.Exists(CookiePath))
            {
                var texture=new Texture2D(128,128,TextureFormat.RGBA32,false,true);var pixels=new Color[128*128];
                for(int y=0;y<128;y++)for(int x=0;x<128;x++)
                {
                    float radius=new Vector2((x-63.5f)/63.5f,(y-63.5f)/63.5f).magnitude;
                    float a=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.45f,1,radius));
                    pixels[y*128+x]=new Color(1,1,1,a);
                }
                texture.SetPixels(pixels);texture.Apply();File.WriteAllBytes(CookiePath,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(CookiePath);
                var importer=(TextureImporter)AssetImporter.GetAtPath(CookiePath);
                importer.textureType=TextureImporterType.Cookie;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                settings.ApplyTextureType(TextureImporterType.Cookie);importer.SetTextureSettings(settings);
                importer.wrapMode=TextureWrapMode.Clamp;importer.mipmapEnabled=false;importer.sRGBTexture=false;
                importer.maxTextureSize=128;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(CookiePath);
        }
        public static void Preview()
        {
            // The older preview remains a historical comparison artifact.
            const string previous="docs/generated/maquette-unity-preview.png";
            byte[] backup=File.Exists(previous)?File.ReadAllBytes(previous):null;
            try {MaquetteArtInstaller.Preview();File.Copy(previous,"docs/generated/local-lighting-preview.png",true);}
            finally {if(backup!=null)File.WriteAllBytes(previous,backup);}
        }
        private static bool checking;
        private static void StartBudget()
        {
            if(!EditorApplication.isPlaying||checking)throw new InvalidOperationException("Play first; only one check at a time");
            var run=UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if(run==null||!run.UsesOpenTrainSurvival)throw new InvalidOperationException("Survival only");
            checking=true;run.StartCoroutine(Budget(run));
        }
        private static System.Collections.IEnumerator Budget(RunDriver run)
        {
            var save=UnityEngine.Object.FindAnyObjectByType<RunPersistence>();string oldSave=save.SavePath;
            var spawn=UnityEngine.Object.FindAnyObjectByType<StationSpawner>();
            var motor=run.Player.GetComponent<WaitYourTurn.Player.PlayerMotor>();
            var torch=UnityEngine.Object.FindAnyObjectByType<MaquetteFlashlight>();bool oldShadows=torch.CastMetalShadows;
            var log=new System.Text.StringBuilder(DateTime.UtcNow.ToString("O")+"\nEditor-only moving24-enemy flashlight shadow OFF/ON; not Android acceptance.\n");
            try
            {
                save.UseTestStore(Path.Combine(Application.persistentDataPath,"acceptance-only","local-lighting.json"));
                run.Restart();spawn.SpawningAllowed=false;run.Player.Invulnerable=true;run.AimAllowed=false;
                yield return new WaitForSeconds(1);
                // Stress actors belong at a stopped station, never on scrolling approach scenery.
                run.Flow.Tick(run.Flow.Remaining + .01f);
                foreach(var wagon in run.Wagons)while(wagon.Enemies.WarmOne()){}
                var profile=spawn.Programs[0].bands[0].profile;int spawned=0;
                foreach(var wagon in run.Wagons)for(int i=0;i<8;i++)
                    if(wagon.Enemies.TrySpawn(wagon.transform.position+new Vector3(-4.5f+(i%4)*3,0,(i<4?-1:1)*5.5f),profile,1))spawned++;
                foreach(var wagon in run.Wagons)foreach(var enemy in wagon.Enemies.Active)enemy.Health.Invulnerable=true;
                motor.ConfigureWorldControl(run.Player,3.5f);
                log.Append($"Spawned={spawned}; GameView={Screen.width}x{Screen.height}; Quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}\n");
                var times=new float[4096];
                using(var main=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Internal,"Main Thread"))
                using(var gc=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory,"GC Allocated In Frame"))
                using(var draws=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Render,"Draw Calls Count"))
                {
                    for(int phase=0;phase<2;phase++)
                    {
                        torch.CastMetalShadows=phase==1;yield return new WaitForSeconds(.5f);
                        int count=0;double sum=0,mainSum=0,gcSum=0,drawSum=0;float start=Time.unscaledTime;
                        while(Time.unscaledTime-start<4&&count<times.Length)
                        {
                            motor.SetWorldMove(Vector3.right*(Mathf.Sin((Time.unscaledTime-start)*3)>0?1:-1));yield return null;
                            float dt=Time.unscaledDeltaTime*1000;times[count++]=dt;sum+=dt;
                            if(main.Valid)mainSum+=main.LastValue;if(gc.Valid)gcSum+=gc.LastValue;if(draws.Valid)drawSum+=draws.LastValue;
                        }
                        Array.Sort(times,0,count);
                        log.Append(FormattableString.Invariant($"Shadows={(torch.CastMetalShadows?"ON":"OFF")}; frames={count}; meanMs={sum/count:F2}; P95ms={times[(int)(count*.95)]:F2}; mainMs={mainSum/count/1e6:F2}; gcKB={gcSum/count/1024:F2}; draws={drawSum/count:F1}; drawCounterValid={draws.Valid}\n"));
                    }
                }
                motor.SetWorldMove(Vector3.zero);torch.CastMetalShadows=oldShadows;
                yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("docs/generated/local-lighting-moving-game.png");
                File.WriteAllText("docs/generated/local-lighting-budget.txt",log.ToString());Debug.Log("[LocalLighting] "+log);
            }
            finally
            {
                torch.CastMetalShadows=oldShadows;motor.SetWorldMove(Vector3.zero);
                motor.Configure(run.Player.GetComponent<WaitYourTurn.Player.MoveInput>(),run.Player,Camera.main);
                run.Player.Invulnerable=false;run.AimAllowed=true;run.Restart();spawn.SpawningAllowed=true;
                save.UseTestStore(oldSave);checking=false;
            }
        }
    }
}
