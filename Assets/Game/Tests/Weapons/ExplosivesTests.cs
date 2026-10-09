using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;
using Object = UnityEngine.Object;

namespace WaitYourTurn.Tests
{
    public sealed class ExplosivesTests
    {
        private readonly List<Object> objects = new List<Object>();
        private readonly Vector3 origin = new Vector3(1000, 1000, 1000);
        private T Keep<T>(T value) where T : Object { objects.Add(value); return value; }
        private GameObject Root(string name) => Keep(new GameObject(name));
        [TearDown] public void Cleanup()
        { for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) Object.DestroyImmediate(objects[i]); objects.Clear(); Physics.SyncTransforms(); }
        private HealthComponent Body(Vector3 local, Team team, float health, bool collider = true)
        {
            var root = Root("Test combat body"); root.transform.position = origin + local;
            if (collider) { var box = root.AddComponent<BoxCollider>(); box.center = Vector3.up * .85f; box.size = new Vector3(.5f, 1.7f, .5f); }
            var body = root.AddComponent<HealthComponent>(); body.ResetForSpawn(health, team); return body;
        }
        private ExplosionPulse Pulse()
        {
            var root = Root("Test pulse"); var flash = Root("Flash").transform; flash.SetParent(root.transform, false);
            var ring = Root("Ring").AddComponent<LineRenderer>(); ring.transform.SetParent(root.transform, false);
            var pulse = root.AddComponent<ExplosionPulse>(); pulse.Configure(flash, ring); pulse.Clear(); return pulse;
        }
        private HitscanWeapon Launcher(TargetRegistry targets, out HealthComponent owner, out ExplosiveProjectiles delivery)
        {
            owner = Body(Vector3.zero, Team.Player, 100); var gun = owner.gameObject.AddComponent<HitscanWeapon>(); gun.Configure(owner);
            var pistol = Keep(ScriptableObject.CreateInstance<WeaponDefinition>());
            var grenade = Keep(ScriptableObject.CreateInstance<WeaponDefinition>()); grenade.delivery = ShotDelivery.Grenade;
            grenade.damage = 90; grenade.range = 14; grenade.infiniteReserve = false; grenade.initialReserve = 12;
            gun.ConfigureDefinitions(new[] { pistol, grenade }); gun.ConfigureInventory(true);
            delivery = Root("Projectile pool").AddComponent<ExplosiveProjectiles>();
            delivery.Configure(gun, owner, targets, new[] { new GrenadeView { body = Root("Grenade").transform, explosion = Pulse() } });
            Assert.That(gun.TryGrantOrRefill(1)); return gun;
        }
        private BoxCollider Cover(float x, int layer = 0)
        {
            var cover = Root("Thin metal"); cover.layer = layer; cover.transform.position = origin + new Vector3(x, .9f, 0);
            var box = cover.AddComponent<BoxCollider>(); box.size = new Vector3(.08f, 2, 4); return box;
        }
        [Test] public void GrenadeTravelsBeforeDamagingThenKillsClusterAndCreditsOwner()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var gun = Launcher(registry, out var owner, out var delivery);
            var normal = Body(Vector3.right * 3, Team.Enemy, 12); var medium = Body(new Vector3(3, 0, .8f), Team.Enemy, 70);
            registry.Register(normal); registry.Register(medium); EntityIdentity reward = default; normal.Died += death => reward = death.Damage.RewardOwner;
            Physics.SyncTransforms(); Assert.That(gun.TryFire(Vector3.right)); Assert.That(normal.Current, Is.EqualTo(12));
            Assert.That(gun.Rounds, Is.EqualTo(7)); delivery.Tick(.5f);
            Assert.That(normal.IsAlive, Is.False); Assert.That(medium.IsAlive, Is.False); Assert.That(delivery.Detonations, Is.EqualTo(1));
            Assert.That(reward, Is.EqualTo(owner.Identity)); Assert.That(owner.Current, Is.EqualTo(100));
        }
        [Test] public void GrenadeSweepsThinMetalOnSlowFrameAndDoesNotBlastThroughIt()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var gun = Launcher(registry, out _, out var delivery);
            var enemy = Body(Vector3.right * 2, Team.Enemy, 12); registry.Register(enemy); Cover(1); Physics.SyncTransforms();
            Assert.That(gun.HasSight(enemy), Is.False); Assert.That(gun.TryFire(Vector3.right)); delivery.Tick(1);
            Assert.That(enemy.Current, Is.EqualTo(12)); Assert.That(delivery.ActiveCount, Is.Zero); Assert.That(delivery.Detonations, Is.EqualTo(1));
        }
        [Test] public void ProjectileSnapshotRestoresFlightWithoutAnExtraShotAndRejectsCorruptDirection()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var gun = Launcher(registry, out _, out var delivery);
            Assert.That(gun.TryFire(Vector3.right)); delivery.Tick(.05f); var snapshot = gun.Capture();
            gun.ResetWeapon(); Assert.That(delivery.ActiveCount, Is.Zero); Assert.That(gun.Restore(snapshot));
            Assert.That(delivery.ActiveCount, Is.EqualTo(1)); Assert.That(gun.Rounds, Is.EqualTo(7));
            snapshot.projectiles[0].direction = Vector3.zero; Assert.That(gun.CanRestore(snapshot), Is.False);
        }
        private MineController Mine(TargetRegistry registry, HealthComponent owner)
        {
            var root = Root("Mine"); var source = root.AddComponent<HealthComponent>();
            var mine = root.AddComponent<MineController>(); mine.Configure(source, Root("Body").transform, Root("Diode").transform, Pulse());
            Assert.That(mine.TryDeploy(Keep(ScriptableObject.CreateInstance<MineDefinition>()), owner, registry, Physics.AllLayers, origin)); return mine;
        }
        [Test] public void LeadingShotInterceptsCrossingEnemyBeforeMaximumRange()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var gun = Launcher(registry, out _, out var delivery);
            gun.Definition.projectileSpeed = 24; gun.Definition.range = 12;
            var enemy = Body(Vector3.right * 6, Team.Enemy, 70); registry.Register(enemy);
            Vector3 velocity = Vector3.forward * 4;
            Physics.SyncTransforms(); Assert.That(gun.HasSight(enemy));
            Assert.That(gun.AimDirectionFor(enemy, velocity).z, Is.GreaterThan(.9f));
            Assert.That(gun.TryFireAt(enemy, velocity));
            for (int i = 0; i < 6 && delivery.ActiveCount > 0; i++)
            {
                enemy.transform.position += velocity * .05f; Physics.SyncTransforms(); delivery.Tick(.05f);
            }
            Assert.That(delivery.Detonations, Is.EqualTo(1)); Assert.That(enemy.IsAlive, Is.False);
        }
        [Test] public void LeadingAimFallsBackWhenItsPathCrossesMetalWindowPost()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var gun = Launcher(registry, out _, out _);
            gun.Definition.projectileSpeed = 24; gun.Definition.range = 12;
            var enemy = Body(Vector3.right * 6, Team.Enemy, 70);
            var post = Root("Window metal post"); post.transform.position = origin + new Vector3(3, .9f, .5f);
            post.AddComponent<BoxCollider>().size = new Vector3(.2f, 2, .25f); Physics.SyncTransforms();
            Assert.That(gun.HasSight(enemy));
            Vector3 direct = enemy.transform.position + Vector3.up * .85f - gun.Muzzle;
            Assert.That(gun.AimDirectionFor(enemy, Vector3.forward * 4), Is.EqualTo(direct));
        }
        [Test] public void HitscanStillAimsDirectlyAtMovingTarget()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var gun = Launcher(registry, out _, out _);
            var enemy = Body(Vector3.right * 6, Team.Enemy, 70); Assert.That(gun.Equip(0));
            Assert.That(gun.AimDirectionFor(enemy, Vector3.forward * 4),
                Is.EqualTo(enemy.transform.position + Vector3.up * .85f - gun.Muzzle));
        }
        [Test] public void InterceptRejectsUnreachableTargetAndKeepsStationaryAim()
        {
            Vector3 offset = Vector3.right * 6;
            Assert.That(ProjectileAim.TryLead(offset, Vector3.zero, 24, 12, out var aim)); Assert.That(aim, Is.EqualTo(offset));
            Assert.That(ProjectileAim.TryLead(offset, Vector3.right * 20, 10, 12, out _), Is.False);
            Assert.That(ProjectileAim.TryLead(Vector3.right * 13, Vector3.forward, 24, 12, out _), Is.False);
            Assert.That(ProjectileAim.TryLead(offset, new Vector3(float.NaN, 0, 0), 24, 12, out _), Is.False);
        }
        [Test] public void MineIgnoresPlayerAndFriendlyBodiesThenDetonatesOnlyOnceForEnemyCluster()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var owner = Body(Vector3.zero, Team.Player, 100, false);
            registry.Register(owner); var friendly = Body(new Vector3(.2f, 0, 0), Team.Player, 100); registry.Register(friendly);
            var mine = Mine(registry, owner); mine.Tick(1); Assert.That(mine.Deployed); Assert.That(owner.Current, Is.EqualTo(100));
            var normal = Body(new Vector3(.4f, 0, 0), Team.Enemy, 12); var medium = Body(new Vector3(.5f, 0, .2f), Team.Enemy, 70);
            registry.Register(normal); registry.Register(medium); Physics.SyncTransforms(); mine.Tick(.2f); mine.Tick(.2f);
            Assert.That(mine.Detonations, Is.EqualTo(1)); Assert.That(normal.IsAlive, Is.False); Assert.That(medium.IsAlive, Is.False);
            Assert.That(owner.Current, Is.EqualTo(100)); Assert.That(friendly.Current, Is.EqualTo(100)); Assert.That(mine.Deployed, Is.False);
        }
        [Test] public void MineDoesNotTriggerThroughMetalAndNewOwnerLifeClearsOldMine()
        {
            var registry = Root("Targets").AddComponent<TargetRegistry>(); var owner = Body(Vector3.zero, Team.Player, 100, false);
            var enemy = Body(Vector3.right * .5f, Team.Enemy, 12); registry.Register(enemy); Cover(.3f); Physics.SyncTransforms();
            var mine = Mine(registry, owner); mine.Tick(1); Assert.That(mine.Deployed); Assert.That(enemy.Current, Is.EqualTo(12));
            owner.ResetForSpawn(100, Team.Player); mine.Tick(.1f); Assert.That(mine.Deployed, Is.False); Assert.That(mine.Detonations, Is.Zero);
        }
        [Test] public void GrenadeRadiusPassesGlassMaskButMetalWindowPostStopsIt()
        {
            int glass = LayerMask.NameToLayer("ShotTransparent"); var pane = Cover(1, glass); Physics.SyncTransforms();
            var resolver = new HitscanResolver(); var mask = ~(1 << glass);
            Assert.That(resolver.Sweep(origin + Vector3.up * .9f, Vector3.right, 2, .1f, mask, null, out _), Is.False);
            pane.gameObject.layer = 0; Physics.SyncTransforms();
            Assert.That(resolver.Sweep(origin + Vector3.up * .9f, Vector3.right, 2, .1f, mask, null, out _));
        }
    }
}
