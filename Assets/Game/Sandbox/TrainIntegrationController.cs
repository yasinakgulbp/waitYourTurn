using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Representative acceptance fixture. Production modules do not depend on this HUD.</summary>
    public sealed partial class TrainIntegrationController : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPresentation presentation;
        private bool checking;
        private bool restoreStartingWeaponOnly;
        private bool restoreFixedSeed, restoreRandomInitial;
        private string result="WASD / left joystick; automatic fire. F10: integration checks.";
        public void Configure(RunDriver owner,StationSpawner spawn,RunPresentation visual)
        { run=owner;spawner=spawn;presentation=visual; }
        private void Update()
        {
            if (!checking && Input.GetKeyDown(KeyCode.F3) && !IsSurvival) StartCoroutine(CheckMovement());
            if (!checking && Input.GetKeyDown(KeyCode.F10)) StartCheck();
        }
        private bool IsSurvival => run.Solo != null && run.Solo.OpenTrainSurvival;
        private void StartCheck()
        {
            if (IsSurvival) { var check = FindAnyObjectByType<SurvivalAcceptance>(); check.StartCoroutine(check.Check()); }
            else StartCoroutine(Check());
        }
        private void OnGUI()
        {
            if(run.Flow==null||run.CurrentWagon==null)return;
            Rect safe=Screen.safeArea;
            float scale=Mathf.Max(.1f,Mathf.Min(safe.width/960f,safe.height/540f));
            Vector2 offset=new Vector2(safe.x,Screen.height-safe.yMax);
            float width=safe.width/scale,height=safe.height/scale;
            var input=run.Player.GetComponent<MoveInput>();
            input.BlockedScreenArea=new Rect(offset.x,offset.y,safe.width,110*scale);
            input.SecondaryBlockedScreenArea=new Rect(offset.x+safe.width*.28f,offset.y+safe.height-62*scale,safe.width*.72f,62*scale);
            Matrix4x4 oldMatrix=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,new Vector3(scale,scale,1));
            var label=new GUIStyle(GUI.skin.label){fontSize=16};
            GUI.Label(new Rect(12,8,width-220,24),$"{(IsSurvival ? "SURVIVAL / " : "")}{run.CurrentWagon.Id} / STATION {run.Flow.Station} / {run.CurrentWagon.Doors.Length} DOORS",label);
            string phaseText = IsSurvival ? run.Flow.Phase switch
            { RunPhase.Defense => spawner.WaveCompleted ? "Wave spawned / boarding" : "Defend the train",
                RunPhase.Approach => "Arriving at station", RunPhase.Departing => "Departing", RunPhase.Cruising => "On the move", _ => run.Flow.Phase.ToString() } :
                $"{run.Flow.Phase} {run.Flow.Remaining:F1}s";
            GUI.Label(new Rect(12,32,width-24,24),$"{phaseText} | HP {run.Player.Current:F0} | {run.Weapon.DisplayName} {run.Weapon.Rounds} | Repair {run.Repair.Progress:P0}",label);
            GUI.Label(new Rect(12,56,width-24,23),$"Doors: {string.Join("  ",run.CurrentWagon.Doors.Select((d,i)=>$"{i+1}:{d.Durability.Current:F0}"))} | Enemies {run.CurrentWagon.Enemies.Active.Count}/{run.CurrentWagon.Enemies.Capacity} | Train {spawner.TotalActive} | Queue {spawner.Pending} | F11: spawn checks",new GUIStyle(label){fontSize=12});
            GUI.enabled=!checking;
            if(GUI.Button(new Rect(width-180,8,76,25),"Restart")){spawner.enabled=true;run.ControlsAllowed=run.AimAllowed=true;run.Repair.enabled=true;run.Restart();}
            if(GUI.Button(new Rect(width-98,8,86,25),"Check (F10)"))StartCheck();
            float buttonWidth=80,total=run.Weapon.WeaponCount*(buttonWidth+6);
            for(int i=0;i<run.Weapon.WeaponCount;i++)
            {
                bool owned=run.Weapon.IsOwned(i);GUI.enabled=!checking&&owned;
                if(GUI.Button(new Rect((width-total)*.5f+i*(buttonWidth+6),height-55,buttonWidth,43),run.Weapon.WeaponName(i)+(owned?"":"\nLocked"),new GUIStyle(GUI.skin.button){fontSize=14,wordWrap=true}))run.Weapon.Equip(i);
            }
            GUI.enabled=true;
            if(checking||result.StartsWith("FAIL")||run.Failure!=null)
                GUI.Label(new Rect(12,82,width-24,45),run.Failure??(checking?"Checking integration…":result),label);
            if(spawner.Diagnostic!=null)GUI.Label(new Rect(12,82,width-24,45),spawner.Diagnostic,label);
            if(!checking&&!run.Flow.Paused&&run.Player.DamageProtectionRemaining>0)
                GUI.Label(new Rect(12,82,width-24,30),$"Arrival shield: {run.Player.DamageProtectionRemaining:F1}s",label);
            GUI.matrix=oldMatrix;
            if(presentation.Darkness>0)
            {
                GUI.color=new Color(0,0,0,presentation.Darkness);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
                if(run.Flow.Phase==RunPhase.Hidden)GUI.Label(new Rect(Screen.width/2-120,Screen.height/2,280,60),$"{(run.UsesSoloProgression ? "Sonraki istasyona yolculuk…" : "Vagon dağılımı hazırlanıyor…")} {Mathf.CeilToInt(run.Flow.Remaining)}");
            }
        }
        private IEnumerator Check()
        {
            restoreFixedSeed=run.UseFixedSeed;restoreRandomInitial=run.RandomInitialWagon;
            run.ConfigureRandomAssignments(true, false);
            restoreStartingWeaponOnly=run.Weapon.StartingWeaponOnly;run.Weapon.ConfigureInventory(false);
            run.Match?.SetSuppressed(true); checking = true;spawner.enabled=false;run.ControlsAllowed=run.AimAllowed=false;run.Repair.enabled=false;
            run.Restart(new RunTimings{initialApproach=.2f,defense=300});run.Player.Invulnerable=true;
            yield return new WaitForSeconds(.6f);
            var wagon=run.CurrentWagon;var motor=run.Player.GetComponent<PlayerMotor>();
            foreach(var door in wagon.Doors)
            {
                uint life=door.Durability.LifeVersion;
                motor.Place(door.RepairPosition+Vector3.up*.05f);
                door.Durability.TryApplyDamage(new DamageContext(1,default,Team.Enemy,800,life));
                run.Repair.SetDoor(door);run.Repair.Cancel();run.Repair.Tick(3.1f);
                if(door.Durability.Current!=door.Durability.Maximum||door.Durability.LifeVersion!=life)
                {Finish(false,"Partial repair failed on "+door.name);yield break;}
                Physics.SyncTransforms();
                if(!Physics.Linecast(door.RepairPosition+Vector3.up*.3f,door.Portal.OutsideApproach+Vector3.up*.3f,
                    out RaycastHit panelHit,~(1<<LayerMask.NameToLayer("ShotTransparent")),QueryTriggerInteraction.Ignore)||panelHit.collider.GetComponentInParent<WaitYourTurn.Train.DoorController>()!=door)
                {Finish(false,"Lower panel failed on "+door.name);yield break;}
                motor.MoveDisplacement(-door.Portal.transform.forward*5);
                if(!wagon.Geometry.Contains(run.Player.transform.position)){Finish(false,"Player escaped closed door");yield break;}
            }
            motor.Place(wagon.transform.TransformPoint(new Vector3(1,.05f,0)));yield return null;
            var view=Camera.main;
            float expectedX=presentation.ResponsiveFraming?wagon.transform.position.x+.65f:run.Player.transform.position.x+presentation.FollowOffset.x;
            if(Mathf.Abs(view.transform.position.x-expectedX)>.01f)
            {Finish(false,"Prototype camera does not follow player X");yield break;}
            // Every real opening: both sides select their own door, closed glass passes shots, walls block them.
            int doorCount=wagon.Doors.Length;
            for(int i=0;i<doorCount;i++)
            {
                if(!wagon.SpawnOutside(i)){Finish(false,$"Door {i+1}: no complete spawn-to-entry path");yield break;}
                var enemy=wagon.Enemies.Active[wagon.Enemies.Active.Count-1];enemy.SetPaused(true);
                var door=wagon.Doors[i];motor.Place(door.RepairPosition+Vector3.up*.05f);Physics.SyncTransforms();
                if(!run.Weapon.HasSight(enemy.Health)){Finish(false,$"Door {i+1}: glass blocks sight");yield break;}
                float jambX=wagon.Doors[0].transform.position.x-wagon.transform.position.x-.76f;
                motor.Place(wagon.transform.TransformPoint(new Vector3(jambX,.05f,0)));Physics.SyncTransforms();
                // The metal next to a doorway remains opaque even when wall windows pass shots.
                var enemyMotor=enemy.GetComponent<WaitYourTurn.Navigation.AgentMotor>();
                Vector3 wallTarget=wagon.transform.TransformPoint(new Vector3(jambX,0,Mathf.Sign(wagon.Geometry.StationSpawn(i).z-wagon.transform.position.z)*4.5f));
                if(!enemyMotor.TryPlace(wallTarget)){Finish(false,"Wall fixture cannot place enemy");yield break;}
                Physics.SyncTransforms();if(run.Weapon.HasSight(enemy.Health)){Finish(false,"Solid wagon wall passes shots");yield break;}
                enemyMotor.TryPlace(wagon.Geometry.StationSpawn(i));enemy.SetPaused(false);
            }
            motor.Place(wagon.transform.TransformPoint(new Vector3(0,.05f,0)));
            yield return new WaitForSeconds(4);
            if(wagon.Doors.Any(d=>d.Durability.Current==d.Durability.Maximum)||wagon.Enemies.Active.Any(e=>e.IsInside))
            {Finish(false,"Closed-door crowd failed to attack all six doors from both platforms");yield break;}
            foreach(var door in wagon.Doors)door.Durability.TryApplyDamage(new DamageContext(1000,default,Team.Enemy,900,door.Durability.LifeVersion));
            foreach(var door in wagon.Doors)
            {
                motor.Place(door.RepairPosition+Vector3.up*.05f);motor.MoveDisplacement(-door.Portal.transform.forward*5);
                if(!wagon.Geometry.Contains(run.Player.transform.position)){Finish(false,"Player escaped broken door");yield break;}
            }
            motor.Place(wagon.transform.TransformPoint(new Vector3(0,.05f,0)));
            yield return new WaitForSeconds(8);
            if(wagon.Enemies.Active.Count!=doorCount||wagon.Enemies.Active.Any(e=>!e.IsInside||!e.OnBoard))
            {Finish(false,"Not all six doorway routes entered the actual interior: "+string.Join("; ",wagon.Enemies.Active.Select(e=>$"{e.transform.position:F3} {e.State} inside={e.IsInside} onboard={e.OnBoard} path={e.GetComponent<UnityEngine.AI.NavMeshAgent>().pathStatus}")));yield break;}
            // Opposite platform is outside, despite being on the inward half-plane of the first door.
            if(wagon.Geometry.Contains(wagon.Geometry.StationSpawn(1))){Finish(false,"Opposite platform counted as interior");yield break;}
            for(int side=0;side<2;side++)
            {
                var survivor=wagon.Enemies.Active[side];survivor.SetPaused(true);
                if(!survivor.GetComponent<WaitYourTurn.Navigation.AgentMotor>().TryPlace(wagon.Doors[side].Portal.transform.TransformPoint(new Vector3(0,0,.15f))))
                {Finish(false,"Threshold survivor placement failed");yield break;}
                wagon.Doors[side].TryRepair();
                if(!wagon.Doors[side].Portal.ClosePending){Finish(false,"Occupied threshold closed prematurely");yield break;}
            }
            if(!wagon.SpawnOutside(doorCount)){Finish(false,"Departure outsider setup failed");yield break;}
            wagon.Enemies.Active[wagon.Enemies.Active.Count-1].SetPaused(true);
            if(!wagon.ResolveDeparture()||wagon.Enemies.Active.Count!=doorCount||wagon.Enemies.DeathCount!=0)
            {Finish(false,"Departure did not retain interior / recycle exterior without kill reward");yield break;}
            yield return null;yield return null;
            if(wagon.Doors.Take(2).Any(d=>d.Portal.ClosePending||d.Portal.IsOpen)||wagon.Enemies.Active.Any(e=>!e.IsInside||e.NeedsSafeDeparture))
            {Finish(false,"Bilateral threshold survivors were not moved safely before door closure");yield break;}
            // Exercise every authored 4/5/6-door variant, not only the first six-door wagon.
            foreach(var w in run.Wagons)
            {
                w.Enemies.ClearAlive();motor.SetMovementArea(w.Area);
                foreach(var d in w.Doors)d.TryRepair();
                for(int i=0;i<w.Doors.Length;i++)
                {
                    var d=w.Doors[i];uint life=d.Durability.LifeVersion;
                    d.Durability.TryApplyDamage(new DamageContext(1,default,Team.Enemy,903,life));
                    motor.Place(d.RepairPosition+Vector3.up*.05f);run.Repair.SetDoor(d);run.Repair.Cancel();run.Repair.Tick(3.1f);
                    if(d.Durability.Current!=d.Durability.Maximum||d.Durability.LifeVersion!=life||!w.SpawnOutside(i))
                    {Finish(false,"Variant repair/spawn failed: "+w.Id);yield break;}
                    var enemy=w.Enemies.Active[w.Enemies.Active.Count-1];enemy.SetPaused(true);Physics.SyncTransforms();
                    if(!run.Weapon.HasSight(enemy.Health)){Finish(false,"Variant glass sight failed: "+w.Id);yield break;}
                }
                // Removed doors become window walls, retaining metal below the clear aperture.
                foreach(int sign in new[]{-1,1})
                {
                    if(w.Doors.Any(d=>Mathf.Abs(d.transform.position.x-w.transform.position.x)<.1f&&Mathf.Sign(d.transform.position.z-w.transform.position.z)==sign))continue;
                    Vector3 p=w.transform.position+Vector3.up*.35f;
                    if(!Physics.Linecast(p+Vector3.forward*sign*.4f,p+Vector3.forward*sign*(w.Geometry.Interior.extents.z+1),
                        out RaycastHit wallHit,~(1<<LayerMask.NameToLayer("ShotTransparent")),QueryTriggerInteraction.Ignore)||!wallHit.collider.name.Contains("Window lower metal"))
                    {Finish(false,"Removed center door left a wall gap: "+w.Id);yield break;}
                }
                w.Enemies.ClearAlive();
                if(!w.SpawnOutside(0)){Finish(false,"Window fixture spawn failed");yield break;}
                var windowEnemy=w.Enemies.Active[0];windowEnemy.SetPaused(true);
                var windowMotor=windowEnemy.GetComponent<WaitYourTurn.Navigation.AgentMotor>();
                var resolver=new HitscanResolver();int shotMask=~(1<<LayerMask.NameToLayer("ShotTransparent"));
                var windows=w.GetComponentsInChildren<WaitYourTurn.Train.WagonWindow>();
                float outerDoorX=w.Doors.Max(d=>Mathf.Abs(d.transform.position.x-w.transform.position.x));
                if(windows.Any(p=>Mathf.Abs(p.transform.position.x-w.transform.position.x)>outerDoorX))
                {Finish(false,"End bay must not contain windows: "+w.Id);yield break;}
                foreach(int end in new[]{-1,1})foreach(int side in new[]{-1,1})foreach(float slant in new[]{0f,.1f})
                {
                    Vector3 point=w.transform.position+new Vector3(end*(outerDoorX+w.Geometry.Interior.extents.x)*.5f,1.1f,side*w.Geometry.Interior.extents.z);
                    Vector3 direction=new Vector3(slant,0,side).normalized;
                    if(!resolver.Cast(point-direction,direction,2,shotMask,run.Player,out RaycastHit endMetal)||
                        endMetal.collider==null||endMetal.collider.name!="Solid wall collider")
                    {Finish(false,"End bay metal passes shots: "+w.Id);yield break;}
                }
                if(windows.Length==0||!windows.Any(p=>p.transform.position.z>w.transform.position.z)||!windows.Any(p=>p.transform.position.z<w.transform.position.z))
                {Finish(false,"Windows missing on one side: "+w.Id);yield break;}
                foreach(var aperture in windows)
                {
                    Vector3 center=aperture.transform.position;
                    float sign=Mathf.Sign(center.z-w.transform.position.z);
                    // Every gun uses the same real collider path: glass takes damage, either metal edge does not.
                    foreach(float offset in new[]{0f,-(aperture.ClearSize.x*.5f+aperture.FrameWidth*.5f),aperture.ClearSize.x*.5f+aperture.FrameWidth*.5f})
                    {
                        Vector3 inside=new Vector3(center.x+offset,.05f,center.z-sign);
                        Vector3 outside=new Vector3(center.x+offset,0,center.z+sign*1.6f);
                        motor.Place(inside);
                        if(!windowMotor.TryPlace(outside)){Finish(false,"Window target outside navigation");yield break;}
                        Physics.SyncTransforms();bool glass=offset==0;
                        for(int gun=0;gun<run.Weapon.WeaponCount;gun++)
                        {
                            windowEnemy.Health.ResetForSpawn(1000,Team.Enemy);
                            run.Weapon.ResetWeapon();run.Weapon.Equip(gun);
                            ShotNotice primary=default;System.Action<ShotNotice> capture=notice=>{if(notice.Pellet==0)primary=notice;};
                            run.Weapon.Fired+=capture;
                            bool sight=run.Weapon.HasSight(windowEnemy.Health);
                            bool fired=run.Weapon.TryFire(windowEnemy.transform.position+Vector3.up*.85f-run.Weapon.Muzzle);
                            run.Weapon.Fired-=capture;
                            // Shotgun side pellets may go around thin metal; the central ray must stop on it.
                            if(sight!=glass||!fired||(glass&&windowEnemy.Health.Current>=1000)||
                                (!glass&&(Mathf.Abs(primary.End.z-center.z)>.11f||(run.Weapon.Definition.pellets==1&&windowEnemy.Health.Current<1000))))
                            {Finish(false,$"Window/metal actual {run.Weapon.DisplayName} shot failed: {w.Id}/{aperture.name}/offset {offset}");yield break;}
                        }
                    }
                    // Lower/upper metal and oblique rays at both sides of each exact aperture edge.
                    foreach(float y in new[]{center.y-aperture.ClearSize.y*.5f-.1f,center.y+aperture.ClearSize.y*.5f+.1f})
                        if(!resolver.Cast(new Vector3(center.x,y,center.z-sign),Vector3.forward*sign,3,shotMask,run.Player,out RaycastHit metal)||metal.collider==null||!metal.collider.name.Contains("metal"))
                        {Finish(false,"Window horizontal metal passes shots: "+aperture.name);yield break;}
                    foreach(int edge in new[]{-1,1})foreach(float inset in new[]{-.02f,.02f})
                    {
                        float x=center.x+edge*(aperture.ClearSize.x*.5f-inset);
                        Vector3 direction=new Vector3(.1f,0,sign).normalized;
                        Vector3 point=new Vector3(x,center.y,center.z);
                        bool hit=resolver.Cast(point-direction, direction,2,shotMask,run.Player,out RaycastHit oblique);
                        // Target bodies are at normal aim height, away from this high-frame precision ray.
                        if((hit&&oblique.collider!=null&&oblique.collider.name.Contains("post"))!=(inset<0))
                        {Finish(false,$"Oblique window edge mismatch: {w.Id}/{aperture.name}/edge {edge}/inset {inset}/hit {(oblique.collider!=null?oblique.collider.name:"none")}");yield break;}
                    }
                    Vector3 boundaryStart=new Vector3(center.x,.85f,center.z-sign);
                    if(!Physics.Linecast(boundaryStart,boundaryStart+Vector3.forward*sign*2,out RaycastHit movement,Physics.AllLayers,QueryTriggerInteraction.Ignore)||
                        !movement.collider.name.Contains("movement boundary"))
                    {Finish(false,"Window lost its movement boundary: "+aperture.name);yield break;}
                    var query=new UnityEngine.AI.NavMeshQueryFilter{agentTypeID=windowEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>().agentTypeID,areaMask=UnityEngine.AI.NavMesh.AllAreas};
                    Vector3 navOutside=new Vector3(center.x,0,center.z+sign*1.3f),navInside=new Vector3(center.x,0,center.z-sign);
                    if(!UnityEngine.AI.NavMesh.SamplePosition(navOutside,out var navStart,.2f,query)||!UnityEngine.AI.NavMesh.SamplePosition(navInside,out var navEnd,.2f,query)||
                        !UnityEngine.AI.NavMesh.Raycast(navStart.position,navEnd.position,out _,query))
                    {Finish(false,"Window created a zombie navigation opening: "+aperture.name);yield break;}
                }
                w.Enemies.ClearAlive();
            }
            motor.SetMovementArea(wagon.Area);
            // Ten rapid station cycles across the authored layout, with persistent door damage and fixed navigation.
            run.Restart(new RunTimings{initialApproach=.2f,nextApproach=.2f,defense=.25f,departureWarning=.15f,departure=.2f,fadeOut=.15f,hidden=.2f,fadeIn=.15f});
            run.Player.Invulnerable=false;run.Player.TryApplyDamage(new DamageContext(1,default,Team.Enemy,902,run.Player.LifeVersion));run.Player.Invulnerable=true;
            run.Weapon.Equip(1);run.Weapon.TryFire(Vector3.up);
            int rounds=run.Weapon.Rounds,reserve=run.Weapon.Reserve;
            foreach(var w in run.Wagons)
            {
                foreach(var d in w.Doors)d.Durability.TryApplyDamage(new DamageContext(1,default,Team.Enemy,901,d.Durability.LifeVersion));
                if(!w.SpawnOutside(0)||!w.SpawnOutside(1)){Finish(false,"Multi-wagon spawn anchors failed");yield break;}
                var inside=w.Enemies.Active[0];var outside=w.Enemies.Active[1];
                inside.SetPaused(true);outside.SetPaused(true);
                if(!inside.GetComponent<WaitYourTurn.Navigation.AgentMotor>().TryPlace(w.Geometry.SafePosition(3)))
                {Finish(false,"Multi-wagon interior placement failed");yield break;}
            }
            Vector3[] fixedPositions=run.Wagons.Select(w=>w.Geometry.StationSpawn(0)).ToArray();
            var visited=new HashSet<string>{run.CurrentWagon.Id};
            float deadline=Time.realtimeSinceStartup+25;
            while(run.Assignments<10&&!run.Flow.Terminal&&Time.realtimeSinceStartup<deadline)
            {visited.Add(run.CurrentWagon.Id);yield return null;}
            visited.Add(run.CurrentWagon.Id);
            if(run.Flow.Terminal||run.Assignments<10||visited.Count!=run.Wagons.Length||run.Player.Current!=99||run.Weapon.EquippedIndex!=1||run.Weapon.Rounds!=rounds||run.Weapon.Reserve!=reserve||
                run.Wagons.Where((w,i)=>w.Geometry.StationSpawn(0)!=fixedPositions[i]||w.Enemies.CreatedCount!=12||w.Enemies.Active.Count!=1||!w.Enemies.Active[0].OnBoard||
                    w.Doors.Any(d=>d.Durability.Current!=d.Durability.Maximum-1)).Any())
            {Finish(false,"Persistent layout / bounded pools / 10-cycle flow failed: "+run.Failure);yield break;}
            run.Restart(new RunTimings{initialApproach=.2f,defense=300});run.Player.Invulnerable=true;
            yield return new WaitForSeconds(.5f);
            foreach(var w in run.Wagons)
            {
                for(int i=0;i<12;i++)if(!w.SpawnOutside(i)){Finish(false,"Pool could not fill its authored anchors");yield break;}
                if(w.SpawnOutside(12)){Finish(false,"Pool exceeded fixed capacity");yield break;}
            }
            yield return new WaitForSeconds(2);
            if(run.Wagons.Any(w=>w.Enemies.Active.Count!=12||w.Enemies.CreatedCount!=12||w.Enemies.Active.Any(e=>e.WagonId!=w.Id||!e.GetComponent<WaitYourTurn.Navigation.AgentMotor>().Ready)))
            {Finish(false,"Full bilateral crowd lost its navigation / wagon scope / capacity");yield break;}
            Finish(true,$"Door variants {string.Join(",",run.Wagons.Select(w=>w.Doors.Length))}: repair/glass; every bilateral window passes all four guns, metal edges/panels block straight/oblique rays, movement/nav remain closed; bilateral door entry/thresholds and player boundary; responsive camera; 10 cycles visiting {visited.Count} wagons; HP/SMG/ammo/survivors/door damage preserved; full {run.Wagons.Length*12}-enemy pool bound.");
        }
        private void Finish(bool pass,string detail)
        {
            result=(pass?"PASS: ":"FAIL: ")+detail;Directory.CreateDirectory("Logs");
            File.WriteAllText($"Logs/TrainIntegration-{run.Wagons.Length}.txt",System.DateTime.UtcNow.ToString("O")+"\n"+result);
            if(pass)Debug.Log("[TrainIntegration] "+result);else Debug.LogError("[TrainIntegration] "+result);
            checking=false;run.ConfigureRandomAssignments(restoreFixedSeed,restoreRandomInitial);run.Weapon.ConfigureInventory(restoreStartingWeaponOnly);run.Repair.enabled=true;spawner.enabled=true;run.ControlsAllowed=run.AimAllowed=true;run.Match?.SetSuppressed(false); run.Restart();
        }
    }
}
