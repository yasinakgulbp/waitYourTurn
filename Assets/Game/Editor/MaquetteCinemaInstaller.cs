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
    public static class MaquetteCinemaInstaller
    {
        private const string Request="Temp/maquette-cinema.request";
        private static bool budgetChecking;
        [InitializeOnLoadMethod] private static void Listen()
        { EditorApplication.update-=Consume;EditorApplication.update+=Consume; }
        private static void Consume()
        {
            if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            string action;
            try { action=File.ReadAllText(Request).Trim();File.Delete(Request); } catch(IOException){return;}
            try
            {
                if(action=="install")Install();
                else if(action=="audit")Audit("before");
                else if(action=="preview")Preview();
                else if(action=="after")Audit("after");
                else if(action=="check")Check();
                else if(action=="detail")StartBudget(true);
                else if(action=="markers")
                {
                    var handles=new System.Collections.Generic.List<Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle>();
                    Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetAvailable(handles);
                    File.WriteAllLines("Temp/cinema-markers.txt",handles.Select(h=>Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetDescription(h).Name)
                        .Where(n=>n.Contains("Physics")||n.Contains("NavMesh")||n.Contains("GUI")||n.Contains("Behaviour")||n.Contains("Render")||n.Contains("GC.")||n.Contains("PlayerLoop")||n.Contains("EditorLoop")||n.Contains("WaitFor")).OrderBy(n=>n));
                }
                else throw new InvalidOperationException("Unknown cinema command");
            }
            catch(Exception e){File.WriteAllText("docs/generated/cinema-error.txt",e.ToString());Debug.LogException(e);}
        }
        [MenuItem("Wait Your Turn/Survival/Install Maquette Cinema")]
        public static void Install()
        {
            RequireStopped();
            var scene=SceneManager.GetActiveScene();
            var run=UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if(scene.path!=TrainIntegrationBuilder.SurvivalScenePath||!run.UsesOpenTrainSurvival)throw new InvalidOperationException("Survival only");
            string before=MaquetteArtInstaller.GameplayKey(scene);
            Audit("before");
            int removed=0;
            // Keep all physics/portal transforms. Strip only superseded DISABLED graphics
            // components; the replacement bodies/doors/gangways already are single meshes.
            foreach(var root in scene.GetRootGameObjects())
                foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))
                    if(!r.enabled && (r.transform.root.name.StartsWith("Track visuals") ||
                        r.transform.root.name.StartsWith("Station visuals") ||
                        r.transform.root.name.StartsWith("Journey scenery") ||
                        r.transform.root.name.StartsWith("Incoming station") ||
                        IsLegacyTrainVisual(r.transform)))
                    {
                        var filter=r.GetComponent<MeshFilter>();
                        UnityEngine.Object.DestroyImmediate(r);
                        if(filter!=null)UnityEngine.Object.DestroyImmediate(filter);
                        removed++;
                    }
            var camera=Camera.main;
            var film=camera.GetComponent<MaquetteCinema>();if(film==null)film=camera.gameObject.AddComponent<MaquetteCinema>();
            film.Configure(Shader.Find("WaitYourTurn/MiniatureFilm"));
            camera.nearClipPlane=1;camera.farClipPlane=80;camera.allowHDR=false;
            UnityEngine.Object.FindAnyObjectByType<RunPresentation>().ConfigureCameraLag(.065f);
            foreach(string path in AssetDatabase.FindAssets("t:Material",new[]{"Assets/Game/Art/Maquette/Materials"}).Select(AssetDatabase.GUIDToAssetPath))
            {
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                mat.SetFloat("_Night",1);mat.SetColor("_Ambient",new Color(.13f,.16f,.20f));EditorUtility.SetDirty(mat);
            }
            var light=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Light>(true)).Single(l=>l.type==LightType.Directional);
            light.intensity=1.12f;light.color=new Color(1f,.96f,.9f);light.transform.rotation=Quaternion.Euler(38,-32,0);light.shadows=LightShadows.None;
            Camera.main.backgroundColor=new Color(.012f,.018f,.026f);
            if(MaquetteArtInstaller.GameplayKey(scene)!=before)throw new InvalidOperationException("Gameplay geometry changed; do not save");
            AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            Audit("after");
            File.WriteAllText("docs/generated/cinema-install.txt",DateTime.UtcNow.ToString("O")+
                $"\nSurvival only. Removed {removed} superseded disabled MeshRenderer/MeshFilter pairs, retained gameplay transforms/components.\n"+
                "Single body per wagon; single mesh per gangway; independent functional doors/windows.\n"+
                "Track-relative night gradient, directional studio light, contact edge shade. Quarter-resolution capped384px separable blur; protected train/near-enemy band.\n"+
                "No depth prepass, realtime shadows, HDR, AO, extra camera or pipeline change. Camera follows after PlayerMotor with65ms time-based lag.\n");
            Preview();
        }
        public static void Audit(string suffix)
        {
            var scene=SceneManager.GetActiveScene();var text=new System.Text.StringBuilder(DateTime.UtcNow.ToString("O"));
            foreach(var root in scene.GetRootGameObjects())
            {
                var renderers=root.GetComponentsInChildren<MeshRenderer>(true);
                int active=renderers.Count(r=>r.enabled&&r.gameObject.activeInHierarchy);
                if(renderers.Length>0)text.Append($"\n{root.name}: enabled/active={active}; totalRendererComponents={renderers.Length}; transforms={root.GetComponentsInChildren<Transform>(true).Length}");
            }
            text.Append("\nTrain static visual roots are single mesh renderers; inactive pools and physics transforms do not each issue a draw.\n");
            File.WriteAllText("docs/generated/cinema-audit-"+suffix+".txt",text.ToString());
        }
        public static void Preview()
        {
            RequireStopped();MaquetteArtInstaller.Preview();
            File.Copy("docs/generated/maquette-unity-preview.png","docs/generated/cinema-unity-preview.png",true);
        }
        private static void RequireStopped()
        { if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first"); }
        private static bool IsLegacyTrainVisual(Transform t)
        {
            for(var p=t;p!=null;p=p.parent)
                if(p.name=="Replaceable visuals - no gameplay ownership")return true;
            return t.name=="Internal end jamb"||t.name=="Internal end header"||t.name=="Lower panel"||
                t.name=="Shoot-through glass"||t.name=="Repair floor strip";
        }

        [MenuItem("Wait Your Turn/Survival/Check Moving Cinema Budget")]
        public static void Check()
        { StartBudget(false); }
        [MenuItem("Wait Your Turn/Survival/Check Cinema CPU Detail")]
        public static void CheckDetail()
        { StartBudget(true); }
        private static void StartBudget(bool detail)
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play first");
            var run=UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if(run==null||!run.UsesOpenTrainSurvival||budgetChecking)return;
            budgetChecking=true;run.StartCoroutine(MovingBudget(run,detail));
        }
        private static System.Collections.IEnumerator MovingBudget(RunDriver run,bool detail=false)
        {
            var selected=Selection.objects;
            string[] markerNames={"PlayerLoop","EditorLoop","BehaviourUpdate","LateBehaviourUpdate","Physics.Simulate","NavMesh.Update","Camera.Render","GameView.Render","InspectorWindow.Render","GUI.Repaint","Gfx.WaitForPresentOnGfxThread","WaitForTargetFPS"};
            var handles=new System.Collections.Generic.List<Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle>();
            Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetAvailable(handles);
            var counters=new System.Collections.Generic.List<Unity.Profiling.ProfilerRecorder>();
            var names=new System.Collections.Generic.List<string>();
            if(detail)foreach(var handle in handles)
            {
                var d=Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderHandle.GetDescription(handle);
                if(!markerNames.Contains(d.Name))continue;
                counters.Add(Unity.Profiling.ProfilerRecorder.StartNew(d.Category,d.Name,1,Unity.Profiling.ProfilerRecorderOptions.StartImmediately|Unity.Profiling.ProfilerRecorderOptions.WrapAroundWhenCapacityReached|Unity.Profiling.ProfilerRecorderOptions.SumAllSamplesInFrame));names.Add(d.Name);
            }
            var save=UnityEngine.Object.FindAnyObjectByType<RunPersistence>();string oldSave=save.SavePath;
            var spawn=UnityEngine.Object.FindAnyObjectByType<StationSpawner>();
            var motor=run.Player.GetComponent<WaitYourTurn.Player.PlayerMotor>();
            var film=Camera.main.GetComponent<MaquetteCinema>();
            var log=new System.Text.StringBuilder(DateTime.UtcNow.ToString("O")+"\nEditor movement stress, alternating same-run effect OFF/ON. Not Android acceptance.\n");
            try
            {
                save.UseTestStore(Path.Combine(Application.persistentDataPath,"acceptance-only","cinema.json"));
                run.Restart();spawn.SpawningAllowed=false;run.Player.Invulnerable=true;run.AimAllowed=false;
                yield return new WaitForSeconds(1);
                foreach(var wagon in run.Wagons)while(wagon.Enemies.WarmOne()){}
                var profile=spawn.Programs[0].bands[0].profile;
                int spawned=0;
                foreach(var wagon in run.Wagons)
                    for(int i=0;i<8;i++)
                        if(wagon.Enemies.TrySpawn(wagon.transform.position+new Vector3(-4.5f+(i%4)*3,0,(i<4?-1:1)*5.5f),profile,1))spawned++;
                foreach(var wagon in run.Wagons)foreach(var enemy in wagon.Enemies.Active)enemy.Health.Invulnerable=true;
                motor.ConfigureWorldControl(run.Player,3.5f);
                log.Append($"Spawned={spawned}; GameView={Screen.width}x{Screen.height}; Quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}\n");
                var frameTimes=new float[4096];
                using(var main=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Internal,"Main Thread"))
                using(var gc=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory,"GC Allocated In Frame"))
                using(var draws=Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Render,"Draw Calls Count"))
                {
                    for(int phase=0;phase<4;phase++)
                    {
                        if(detail&&phase>0)Selection.objects=Array.Empty<UnityEngine.Object>();
                        film.enabled=detail?phase>=2:(phase%2)==1;
                        yield return new WaitForSeconds(.5f);
                        var markerSums=new double[counters.Count];
                        float start=Time.unscaledTime;int count=0;double sum=0,mainSum=0,gcSum=0,drawSum=0;
                        while(Time.unscaledTime-start<4&&count<frameTimes.Length)
                        {
                            motor.SetWorldMove(Vector3.right*(Mathf.Sin((Time.unscaledTime-start)*3)>0?1:-1));
                            yield return null;
                            float dt=Time.unscaledDeltaTime*1000;frameTimes[count++]=dt;sum+=dt;
                            if(main.Valid)mainSum+=main.LastValue;if(gc.Valid)gcSum+=gc.LastValue;if(draws.Valid)drawSum+=draws.LastValue;
                            for(int j=0;j<counters.Count;j++)if(counters[j].Valid)markerSums[j]+=counters[j].LastValue;
                        }
                        Array.Sort(frameTimes,0,count);
                        log.Append(FormattableString.Invariant($"Effect={(film.enabled?"ON":"OFF")}; frames={count}; meanMs={sum/count:F2}; P95ms={frameTimes[(int)(count*.95)]:F2}; maxMs={frameTimes[count-1]:F2}; mainMs={mainSum/count/1e6:F2}; gcKB={gcSum/count/1024:F2}; draws={drawSum/count:F1}; drawCounterValid={draws.Valid}\n"));
                        if(detail){log.Append("InspectorSelection="+Selection.objects.Length+"; ");for(int j=0;j<counters.Count;j++)log.Append(FormattableString.Invariant($"{names[j]}={markerSums[j]/count/1e6:F3}ms(valid={counters[j].Valid}); "));log.Append('\n');}
                    }
                }
                film.enabled=true;motor.SetWorldMove(Vector3.zero);
                yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot("docs/generated/cinema-moving-game.png");
                File.WriteAllText("docs/generated/cinema-"+(detail?"cpu-detail":"moving-budget")+".txt",log.ToString());Debug.Log("[CinemaBudget] "+log);
            }
            finally
            {
                foreach(var counter in counters)counter.Dispose();Selection.objects=selected;
                budgetChecking=false;
                film.enabled=true;motor.SetWorldMove(Vector3.zero);
                motor.Configure(run.Player.GetComponent<WaitYourTurn.Player.MoveInput>(),run.Player,Camera.main);
                run.Player.Invulnerable=false;run.AimAllowed=true;
                run.Restart();spawn.SpawningAllowed=true;save.UseTestStore(oldSave);
            }
        }
    }
}
