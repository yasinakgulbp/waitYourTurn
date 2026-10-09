using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    /// <summary>Short, isolated acceptance for the new art's shot geometry and portal bindings.</summary>
    public static class SurvivalTrainArtChecks
    {
        private static bool checking;
        [MenuItem("Wait Your Turn/Survival/Check Train Art %#&t")]
        public static void Start()
        {
            if (!EditorApplication.isPlaying || checking) return;
            var run = UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if (run == null || !run.UsesOpenTrainSurvival) return;
            run.StartCoroutine(Check(run));
        }
        private static IEnumerator Check(RunDriver run)
        {
            checking = true;
            var save = UnityEngine.Object.FindAnyObjectByType<RunPersistence>();
            var spawner = UnityEngine.Object.FindAnyObjectByType<StationSpawner>();
            string path = save.SavePath;
            int doors = 0, windows = 0;
            try
            {
                save.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "train-art.json"));
                run.Restart(); run.ControlsAllowed = run.AimAllowed = false;
                run.Repair.enabled = false; spawner.SpawningAllowed = false; run.Player.Invulnerable = true;
                yield return null;
                Physics.SyncTransforms();
                foreach (var wagon in run.Wagons)
                {
                    Require(wagon.transform.Find("Textured train art - visuals only") != null, "Installed body " + wagon.Id);
                    foreach (var door in wagon.Doors)
                    {
                        var art = door.transform.Find("Textured door art - same portal");
                        Require(art != null && art.gameObject.activeSelf, "Closed red leaf " + door.name);
                        DoorRay(0, .9f, false); DoorRay(0, .35f, true);
                        DoorRay(.66f, 1.3f, true); DoorRay(0, 1.44f, true);
                        door.Durability.TryApplyDamage(new DamageContext(9999, default, Team.Enemy, 97001, door.Durability.LifeVersion));
                        Require(door.Portal.IsOpen && !art.gameObject.activeSelf, "Broken leaf hides " + door.name);
                        DoorRay(.66f, 1.3f, false);
                        Require(door.TryRepair() && !door.Portal.IsOpen && art.gameObject.activeSelf, "Repair restores leaf " + door.name);
                        DoorRay(.66f, 1.3f, true); doors++;
                        void DoorRay(float x, float y, bool blocked)
                        {
                            Physics.SyncTransforms();
                            Require(Physics.Linecast(door.transform.TransformPoint(new Vector3(x,y,.45f)),
                                door.transform.TransformPoint(new Vector3(x,y,-.45f)), out _,
                                ~(1<<LayerMask.NameToLayer("ShotTransparent")), QueryTriggerInteraction.Ignore) == blocked,
                                "Door glass/metal/open ray " + door.name + " at " + x + "/" + y);
                        }
                    }
                    foreach (var pane in wagon.GetComponentsInChildren<WagonWindow>())
                    {
                        float sign = Mathf.Sign(pane.transform.position.z - wagon.transform.position.z);
                        Vector3 p = pane.transform.position;
                        WindowRay(p, false);
                        p.y = .375f; WindowRay(p, true);
                        p.y = 1.825f; WindowRay(p, true);
                        p = pane.transform.position + Vector3.right*(pane.ClearSize.x*.5f + .1f); WindowRay(p, true); windows++;
                        void WindowRay(Vector3 point, bool blocked) => Require(Physics.Linecast(point-Vector3.forward*sign*.45f,
                            point+Vector3.forward*sign*.45f, out _, ~(1<<LayerMask.NameToLayer("ShotTransparent")),
                            QueryTriggerInteraction.Ignore) == blocked, "Side glass/metal ray " + pane.name);
                    }
                }
                var nav = new NavMeshPath(); var pool = run.Wagons[0].Enemies;
                Require(NavMesh.CalculatePath(run.Wagons[0].transform.position, run.Wagons[2].transform.position,
                    new NavMeshQueryFilter { agentTypeID=pool.AgentTypeId, areaMask=pool.AreaMask }, nav) &&
                    nav.status == NavMeshPathStatus.PathComplete, "Rear-to-head navigation through both gangways");
                var motor = run.Player.GetComponent<PlayerMotor>(); motor.ConfigureWorldControl(run.Player,3.5f);
                foreach (var passage in run.Solo.Connections)
                {
                    float x=passage.transform.position.x;
                    motor.Place(new Vector3(x-1.5f,.05f,0)); motor.SetWorldMove(Vector3.right);
                    yield return new WaitForSeconds(1.1f); motor.SetWorldMove(Vector3.zero);
                    Require(run.Player.transform.position.x>x+1, "Physical open gangway " + x);
                }
                motor.Configure(run.Player.GetComponent<MoveInput>(),run.Player,Camera.main);
                run.Restart();
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot("docs/generated/survival-train-game.png");
                string report=DateTime.UtcNow.ToString("O")+"\nPASS: "+doors+" doors glass/metal rays, break/hide/repair/restore; "+windows+
                    " side windows and upper/lower/post metal rays; complete nav and physical walking through both gangways.\n"+
                    "Isolated test save; normal save unchanged. Device FPS/thermal check pending.\n";
                File.WriteAllText("docs/generated/survival-train-art-pc-acceptance.txt",report); Debug.Log("[TrainArtCheck] "+report);
            }
            finally
            {
                var motor=run.Player.GetComponent<PlayerMotor>(); motor.SetWorldMove(Vector3.zero);
                motor.Configure(run.Player.GetComponent<MoveInput>(),run.Player,Camera.main);
                run.ControlsAllowed=run.AimAllowed=true; run.Repair.enabled=true; run.Player.Invulnerable=false;
                run.Restart(); spawner.SpawningAllowed=true; save.UseTestStore(path); checking=false;
            }
        }
        private static void Require(bool valid,string message)
        { if(!valid) { File.WriteAllText("docs/generated/survival-train-art-pc-acceptance.txt",DateTime.UtcNow.ToString("O")+"\nFAIL: "+message); throw new InvalidOperationException(message); } }
    }
}
