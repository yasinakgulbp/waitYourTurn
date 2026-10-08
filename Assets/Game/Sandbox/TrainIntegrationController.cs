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
    public sealed class TrainIntegrationController : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPresentation presentation;
        private bool checking;
        private string result="WASD / left joystick; automatic fire. F10: integration checks.";
        public void Configure(RunDriver owner,StationSpawner spawn,RunPresentation visual)
        { run=owner;spawner=spawn;presentation=visual; }
        private void Update() { if(!checking&&Input.GetKeyDown(KeyCode.F10))StartCoroutine(Check()); }
        private void OnGUI()
        {
            if(run.Flow==null||run.CurrentWagon==null)return;
            Rect safe=Screen.safeArea;
            float scale=Mathf.Max(.1f,Mathf.Min(safe.width/960f,safe.height/540f));
            Vector2 offset=new Vector2(safe.x,Screen.height-safe.yMax);
            float width=safe.width/scale,height=safe.height/scale;
            var input=run.Player.GetComponent<MoveInput>();
            input.BlockedScreenArea=new Rect(offset.x,offset.y,safe.width,90*scale);
            input.SecondaryBlockedScreenArea=new Rect(offset.x+safe.width*.28f,offset.y+safe.height-62*scale,safe.width*.72f,62*scale);
            Matrix4x4 oldMatrix=GUI.matrix;
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,new Vector3(scale,scale,1));
            var label=new GUIStyle(GUI.skin.label){fontSize=16};
            GUI.Label(new Rect(12,8,width-220,24),$"{run.CurrentWagon.Id} / STATION {run.Flow.Station} / {run.CurrentWagon.Doors.Length} DOORS",label);
            GUI.Label(new Rect(12,32,width-24,24),$"{run.Flow.Phase} {run.Flow.Remaining:F1}s | HP {run.Player.Current:F0} | {run.Weapon.DisplayName} {run.Weapon.Rounds} | Repair {run.Repair.Progress:P0}",label);
            GUI.Label(new Rect(12,56,width-24,23),$"Doors: {string.Join("  ",run.CurrentWagon.Doors.Select((d,i)=>$"{i+1}:{d.Durability.Current:F0}"))} | Enemies {run.CurrentWagon.Enemies.Active.Count}/12",new GUIStyle(label){fontSize=12});
            GUI.enabled=!checking;
            if(GUI.Button(new Rect(width-180,8,76,25),"Restart")){spawner.enabled=true;run.ControlsAllowed=run.AimAllowed=true;run.Repair.enabled=true;run.Restart();}
            if(GUI.Button(new Rect(width-98,8,86,25),"Check (F10)"))StartCoroutine(Check());
            float buttonWidth=80,total=run.Weapon.WeaponCount*(buttonWidth+6);
            for(int i=0;i<run.Weapon.WeaponCount;i++)
                if(GUI.Button(new Rect((width-total)*.5f+i*(buttonWidth+6),height-55,buttonWidth,43),run.Weapon.WeaponName(i),new GUIStyle(GUI.skin.button){fontSize=15}))run.Weapon.Equip(i);
            GUI.enabled=true;
            if(checking||result.StartsWith("FAIL")||run.Failure!=null)
                GUI.Label(new Rect(12,82,width-24,45),run.Failure??(checking?"Checking integration…":result),label);
            GUI.matrix=oldMatrix;
            if(presentation.Darkness>0)
            {
                GUI.color=new Color(0,0,0,presentation.Darkness);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
                if(run.Flow.Phase==RunPhase.Hidden)GUI.Label(new Rect(Screen.width/2-120,Screen.height/2,280,60),$"Vagon dağılımı hazırlanıyor… {Mathf.CeilToInt(run.Flow.Remaining)}");
            }
        }
        private IEnumerator Check()
        {
            checking=true;spawner.enabled=false;run.ControlsAllowed=run.AimAllowed=false;run.Repair.enabled=false;
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
                motor.Place(wagon.transform.TransformPoint(new Vector3(-1,.05f,0)));Physics.SyncTransforms();
                // Off-axis line through the adjacent solid side wall must not become a glass shortcut.
                var enemyMotor=enemy.GetComponent<WaitYourTurn.Navigation.AgentMotor>();
                Vector3 wallTarget=wagon.transform.TransformPoint(new Vector3(-1,0,Mathf.Sign(wagon.Geometry.StationSpawn(i).z-wagon.transform.position.z)*4.5f));
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
                // A removed center opening must be an actual solid wall, on either side.
                foreach(int sign in new[]{-1,1})
                {
                    if(w.Doors.Any(d=>Mathf.Abs(d.transform.position.x-w.transform.position.x)<.1f&&Mathf.Sign(d.transform.position.z-w.transform.position.z)==sign))continue;
                    Vector3 p=w.transform.position+Vector3.up*.85f;
                    if(!Physics.Linecast(p+Vector3.forward*sign*.4f,p+Vector3.forward*sign*(w.Geometry.Interior.extents.z+1),
                        out RaycastHit wallHit,~(1<<LayerMask.NameToLayer("ShotTransparent")),QueryTriggerInteraction.Ignore)||!wallHit.collider.name.Contains("Wall segment collider"))
                    {Finish(false,"Removed center door left a wall gap: "+w.Id);yield break;}
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
            Finish(true,$"Door variants {string.Join(",",run.Wagons.Select(w=>w.Doors.Length))}: repair/glass/solid removed openings; bilateral entry/thresholds and player boundary; responsive camera; 10 cycles visiting {visited.Count} wagons; HP/SMG/ammo/survivors/door damage preserved; full {run.Wagons.Length*12}-enemy pool bound.");
        }
        private void Finish(bool pass,string detail)
        {
            result=(pass?"PASS: ":"FAIL: ")+detail;Directory.CreateDirectory("Logs");
            File.WriteAllText($"Logs/TrainIntegration-{run.Wagons.Length}.txt",System.DateTime.UtcNow.ToString("O")+"\n"+result);
            if(pass)Debug.Log("[TrainIntegration] "+result);else Debug.LogError("[TrainIntegration] "+result);
            checking=false;run.Repair.enabled=true;spawner.enabled=true;run.ControlsAllowed=run.AimAllowed=true;run.Restart();
        }
    }
}
