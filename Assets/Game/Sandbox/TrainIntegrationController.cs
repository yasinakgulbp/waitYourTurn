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
            Rect panel=new Rect(10,10,340,225);run.Player.GetComponent<MoveInput>().BlockedScreenArea=panel;
            GUILayout.BeginArea(panel,GUI.skin.box);
            GUILayout.Label($"{run.Wagons.Length} WAGONS / {run.CurrentWagon.Id} / STATION {run.Flow.Station}");
            GUILayout.Label($"{run.Flow.Phase} {run.Flow.Remaining:F1}s | HP {run.Player.Current:F0} | {run.Weapon.DisplayName} {run.Weapon.Rounds}");
            GUILayout.Label("6 doors / 2 platforms / prototype camera");
            GUILayout.Label(string.Join("  ",run.CurrentWagon.Doors.Select((d,i)=>$"{i+1}:{d.Durability.Current:F0}")));
            GUILayout.Label($"Repair {run.Repair.Progress:P0} | Enemies {run.CurrentWagon.Enemies.Active.Count}/12");
            GUI.enabled=!checking;
            GUILayout.BeginHorizontal();
            for(int i=0;i<run.Weapon.WeaponCount;i++)if(GUILayout.Button(run.Weapon.WeaponName(i)))run.Weapon.Equip(i);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Restart")){spawner.enabled=true;run.ControlsAllowed=run.AimAllowed=true;run.Repair.enabled=true;run.Restart();}
            if(GUILayout.Button("Check integration (F10)"))StartCoroutine(Check());
            GUILayout.EndHorizontal();GUI.enabled=true;GUILayout.Label(run.Failure??result);GUILayout.EndArea();
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
            if(Mathf.Abs(view.transform.position.x-run.Player.transform.position.x-presentation.FollowOffset.x)>.01f)
            {Finish(false,"Prototype camera does not follow player X");yield break;}
            // Every real opening: both sides select their own door, closed glass passes shots, walls block them.
            for(int i=0;i<6;i++)
            {
                if(!wagon.SpawnOutside(i)){Finish(false,$"Door {i+1}: no complete spawn-to-entry path");yield break;}
                var enemy=wagon.Enemies.Active[wagon.Enemies.Active.Count-1];enemy.SetPaused(true);
                var door=wagon.Doors[i];motor.Place(door.RepairPosition+Vector3.up*.05f);Physics.SyncTransforms();
                if(!run.Weapon.HasSight(enemy.Health)){Finish(false,$"Door {i+1}: glass blocks sight");yield break;}
                motor.Place(wagon.transform.TransformPoint(new Vector3(-1,.05f,0)));Physics.SyncTransforms();
                // Off-axis line through the adjacent solid side wall must not become a glass shortcut.
                var enemyMotor=enemy.GetComponent<WaitYourTurn.Navigation.AgentMotor>();
                Vector3 wallTarget=wagon.transform.TransformPoint(new Vector3(-1,0,i%2==0?-4.5f:4.5f));
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
            if(wagon.Enemies.Active.Count!=6||wagon.Enemies.Active.Any(e=>!e.IsInside||!e.OnBoard))
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
            if(!wagon.SpawnOutside(6)){Finish(false,"Departure outsider setup failed");yield break;}
            wagon.Enemies.Active[wagon.Enemies.Active.Count-1].SetPaused(true);
            if(!wagon.ResolveDeparture()||wagon.Enemies.Active.Count!=6||wagon.Enemies.DeathCount!=0)
            {Finish(false,"Departure did not retain interior / recycle exterior without kill reward");yield break;}
            yield return null;yield return null;
            if(wagon.Doors.Take(2).Any(d=>d.Portal.ClosePending||d.Portal.IsOpen)||wagon.Enemies.Active.Any(e=>!e.IsInside||e.NeedsSafeDeparture))
            {Finish(false,"Bilateral threshold survivors were not moved safely before door closure");yield break;}
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
            Finish(true,$"Six doors: repair, glass/walls/lower panels, player boundary, bilateral attacks/entry/threshold departure; player camera; 10 station cycles visiting all {visited.Count} wagons; HP/SMG/ammo/interior survivors/all door damage preserved; full {run.Wagons.Length*12}-enemy bilateral pool bound.");
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
