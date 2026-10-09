using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Navigation;
using WaitYourTurn.Train;
using Object = UnityEngine.Object;

namespace WaitYourTurn.Tests
{
    public sealed class DoorRepairTests
    {
        private readonly List<GameObject> created = new List<GameObject>();
        private DoorController door;
        private HealthComponent player;
        private ProximityRepair repair;

        [SetUp] public void Before()
        {
            GameObject panel = New("Repair test door", new Vector3(1000, 0, 1000));
            var health = panel.AddComponent<HealthComponent>(); health.ResetForSpawn(10, Team.Neutral);
            var blocker = panel.AddComponent<BoxCollider>();
            var cut = panel.AddComponent<NavMeshObstacle>();
            var portal = panel.AddComponent<EntryPortal>();
            Transform anchor = New("Repair anchor", panel.transform.position + Vector3.forward * 2).transform;
            portal.Configure(cut, blocker, anchor, anchor);
            door = panel.AddComponent<DoorController>(); door.Configure(health, portal, anchor);
            GameObject hero = New("Repair test player", anchor.position);
            player = hero.AddComponent<HealthComponent>(); player.ResetForSpawn(100, Team.Player);
            repair = hero.AddComponent<ProximityRepair>(); repair.Configure(door, player);
            // Inactive objects let these EditMode checks control time and lifecycle explicitly.
        }
        [TearDown] public void After()
        { for (int i = created.Count - 1; i >= 0; i--) Object.DestroyImmediate(created[i]); created.Clear(); }

        [Test] public void DamagedIntactDoorRepairsAfterThreeSecondsWithoutNewLife()
        {
            Hit(door.Durability, 5); repair.Cancel(); uint life = door.Durability.LifeVersion;
            repair.Tick(2); Assert.That(door.Durability.Current, Is.EqualTo(5));
            repair.Tick(1); Assert.That(door.Durability.Current, Is.EqualTo(10));
            Assert.That(door.Durability.LifeVersion, Is.EqualTo(life)); Assert.That(door.TryRepair(), Is.False);
        }
        [Test] public void BreakingThenRepairingCreatesExactlyOneNewLife()
        {
            Hit(door.Durability, 10); repair.Cancel(); uint life = door.Durability.LifeVersion;
            repair.Tick(3); Assert.That(door.Durability.Current, Is.EqualTo(10));
            Assert.That(door.Durability.LifeVersion, Is.EqualTo(life + 1));
            repair.Tick(30); Assert.That(door.Durability.LifeVersion, Is.EqualTo(life + 1));
        }
        [Test] public void DoorHitOnCompletionFrameCancelsBeforeAnyHealing()
        {
            repair.SetDamageInterruption(true, false);
            Hit(door.Durability, 2); repair.Cancel(); repair.Tick(2.9f);
            Hit(door.Durability, 2); repair.Tick(.2f);
            Assert.That(repair.Progress, Is.Zero); Assert.That(door.Durability.Current, Is.EqualTo(6));
            repair.Tick(2); Assert.That(door.Durability.Current, Is.EqualTo(6));
            repair.Tick(1); Assert.That(door.Durability.Current, Is.EqualTo(10));
        }
        [Test] public void RepeatedDoorHitsCannotGrantInvulnerabilityOrFreeHealing()
        {
            repair.SetDamageInterruption(true, false);
            Hit(door.Durability, 1); repair.Cancel();
            for (int i = 0; i < 9; i++) { repair.Tick(.8f); Hit(door.Durability, 1); repair.Tick(.2f); }
            Assert.That(door.Durability.Current, Is.Zero); Assert.That(repair.Progress, Is.Zero);
            repair.Tick(3); Assert.That(door.Durability.Current, Is.EqualTo(10));
        }
        [Test] public void PlayerHitCancelsAndHealingInSameFrameDoesNotHideIt()
        {
            repair.SetDamageInterruption(false, true);
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(2);
            Hit(player, 1); player.TryHeal(1); repair.Tick(1);
            Assert.That(repair.Progress, Is.Zero); Assert.That(door.Durability.Current, Is.EqualTo(5));
        }
        [Test] public void RejectedHitDoesNotInterruptRepair()
        {
            repair.SetDamageInterruption(true, true);
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(2);
            door.Durability.Invulnerable = true;
            Assert.That(Hit(door.Durability, 1).Applied, Is.False);
            repair.Tick(1); Assert.That(door.Durability.Current, Is.EqualTo(10));
        }
        [Test] public void LeavingRangeCancelsWithoutPartialHealing()
        {
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(2);
            player.transform.position += Vector3.right * 3; repair.Tick(1);
            Assert.That(repair.Progress, Is.Zero); Assert.That(door.Durability.Current, Is.EqualTo(5));
            player.transform.position = door.RepairPosition; repair.Tick(2);
            Assert.That(door.Durability.Current, Is.EqualTo(5));
        }
        [Test] public void PausingFreezesProgressAndDeathCancelsIt()
        {
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(1);
            repair.Paused = true; repair.Tick(100); Assert.That(repair.Progress, Is.EqualTo(1f / 3).Within(.001f));
            repair.Paused = false; Hit(player, 1000); repair.Tick(2);
            Assert.That(repair.Progress, Is.Zero); Assert.That(door.Durability.Current, Is.EqualTo(5));
        }
        [Test] public void ReplacingDoorLifeOrTargetCannotKeepOldProgress()
        {
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(2);
            door.Durability.ResetForSpawn(10, Team.Neutral); Hit(door.Durability, 5); repair.Tick(1);
            Assert.That(repair.Progress, Is.Zero); Assert.That(door.Durability.Current, Is.EqualTo(5));
            repair.Tick(2); repair.SetDoor(null); Assert.That(repair.Progress, Is.Zero);
        }
        [Test] public void FullDoorDoesNotAccumulateRepairProgress()
        { repair.Tick(100); Assert.That(repair.Progress, Is.Zero); Assert.That(door.Durability.Current, Is.EqualTo(10)); }

        [Test] public void DefaultRepairContinuesThroughPlayerDamageWithoutBlockingHits()
        {
            Assert.That(repair.InterruptOnDoorDamage, Is.False); Assert.That(repair.InterruptOnActorDamage, Is.False);
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(1);
            Hit(player, 1); repair.Tick(1);
            Assert.That(repair.Progress, Is.EqualTo(2f / 3).Within(.001f));
            Assert.That(door.Durability.Current, Is.EqualTo(5)); Assert.That(player.Current, Is.EqualTo(99));
            repair.Tick(1); Assert.That(door.Durability.Current, Is.EqualTo(10));
        }
        [Test] public void OptionalUninterruptedRepairCanContinueThroughDoorBreaking()
        {
            repair.SetDamageInterruption(false, false);
            Hit(door.Durability, 4); repair.Cancel(); repair.Tick(2);
            uint life = door.Durability.LifeVersion; Hit(door.Durability, 6);
            Assert.That(door.Durability.IsAlive, Is.False);
            repair.Tick(1); Assert.That(door.Durability.Current, Is.EqualTo(10));
            Assert.That(door.Durability.LifeVersion, Is.EqualTo(life + 1));
        }
        [TestCase(false, false)] [TestCase(false, true)] [TestCase(true, false)] [TestCase(true, true)]
        public void DoorAndPlayerInterruptionPoliciesAreIndependent(bool doorHit, bool playerHit)
        {
            repair.SetDamageInterruption(doorHit, playerHit);
            Hit(door.Durability, 2); repair.Cancel(); repair.Tick(1);
            Hit(door.Durability, 1); repair.Tick(.5f);
            Assert.That(repair.Progress, Is.EqualTo(doorHit ? 0 : .5f).Within(.001f));
            repair.Cancel(); repair.Tick(1); Hit(player, 1); repair.Tick(.5f);
            Assert.That(repair.Progress, Is.EqualTo(playerHit ? 0 : .5f).Within(.001f));
        }
        [Test] public void PolicyChangeCancelsOldProgressWithoutHealing()
        {
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(2);
            repair.SetDamageInterruption(false, true);
            Assert.That(repair.Progress, Is.Zero); Assert.That(door.Durability.Current, Is.EqualTo(5));
        }
        [Test] public void ApplyingSamePolicyDoesNotRestartRepair()
        {
            Hit(door.Durability, 5); repair.Cancel(); repair.Tick(2);
            repair.SetDamageInterruption(false, false);
            Assert.That(repair.Progress, Is.EqualTo(2f / 3).Within(.001f));
        }

        private GameObject New(string name, Vector3 position)
        { var obj = new GameObject(name); obj.SetActive(false); obj.transform.position = position; created.Add(obj); return obj; }
        private static DamageResult Hit(HealthComponent health, float amount) =>
            health.TryApplyDamage(new DamageContext(amount, default, Team.Enemy, 1, health.LifeVersion));
    }
}
