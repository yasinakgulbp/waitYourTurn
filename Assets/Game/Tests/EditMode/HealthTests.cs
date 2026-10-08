using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Tests
{
    public sealed class HealthTests
    {
        [Test]
        public void TimedProtectionIsIndependentFromPauseFlagAndCannotLeakAcrossLives()
        {
            var body = new GameObject("Protection test").AddComponent<HealthComponent>();
            try
            {
                Assert.That(body.SetDamageProtection(2), Is.True);
                body.Invulnerable = true; body.Invulnerable = false;
                Assert.That(body.TryApplyDamage(new DamageContext(10, default, Team.Enemy, 1, body.LifeVersion)).Rejection,
                    Is.EqualTo(DamageRejection.Invulnerable));
                Assert.That(body.SetDamageProtection(float.NaN), Is.False);
                Assert.That(body.SetDamageProtection(-1), Is.False);
                Assert.That(body.DamageProtectionRemaining, Is.GreaterThan(0));
                body.ResetForSpawn(100, Team.Player);
                Assert.That(body.DamageProtectionRemaining, Is.Zero);
                Assert.That(body.TryApplyDamage(new DamageContext(10, default, Team.Enemy, 2, body.LifeVersion)).Applied, Is.True);
                body.SetDamageProtection(2); body.SetDamageProtection(0);
                Assert.That(body.TryApplyDamage(new DamageContext(10, default, Team.Enemy, 3, body.LifeVersion)).Applied, Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(body.gameObject); }
        }
        private static DamageContext Hit(Health target, float amount, Team sourceTeam = Team.Enemy) =>
            new DamageContext(amount, new EntityIdentity(123, 1), sourceTeam, 1, target.LifeVersion);

        [Test]
        public void DoorBreaksOnEightPlusTwoAndDeathCannotRepeat()
        {
            var door = new Health(10);
            Assert.That(door.TryApplyDamage(Hit(door, 8)).Killed, Is.False);
            Assert.That(door.Current, Is.EqualTo(2));
            DamageResult lethal = door.TryApplyDamage(Hit(door, 2));
            Assert.That(lethal.Killed, Is.True);
            Assert.That(lethal.AppliedAmount, Is.EqualTo(2));
            Assert.That(door.TryApplyDamage(Hit(door, 8)).Rejection, Is.EqualTo(DamageRejection.Dead));
            Assert.That(door.TryHeal(100), Is.Zero);
            Assert.That(door.ChangeMaxHealth(20, MaxHealthPolicy.HealAddedCapacity), Is.True);
            Assert.That(door.Current, Is.Zero);
        }

        [TestCase(-1f)] [TestCase(0f)] [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)] [TestCase(float.NegativeInfinity)]
        public void InvalidDamageHealAndMaximumCannotCorruptHealth(float invalid)
        {
            var health = new Health(100);
            Assert.That(health.TryApplyDamage(Hit(health, invalid)).Rejection, Is.EqualTo(DamageRejection.InvalidAmount));
            Assert.That(health.TryHeal(invalid), Is.Zero);
            Assert.That(health.ChangeMaxHealth(invalid), Is.False);
            Assert.That(health.ResetForSpawn(invalid, Team.Player), Is.False);
            Assert.That(health.Current, Is.EqualTo(100));
            Assert.That(health.Maximum, Is.EqualTo(100));
            Assert.That(health.LifeVersion, Is.EqualTo(1));
        }

        [TestCase(10f, 40f)] [TestCase(50f, 80f)] [TestCase(80f, 100f)]
        public void ShopHealUsesCurrentHealthAndCapsAtMaximum(float current, float expected)
        {
            var health = new Health(100);
            health.TryApplyDamage(Hit(health, 100 - current));
            Assert.That(health.TryShopHeal(), Is.EqualTo(expected - current));
            Assert.That(health.Current, Is.EqualTo(expected));
        }

        [Test]
        public void OverkillReportsActualDamageAndZeroIsTheLowerBound()
        {
            var health = new Health(10);
            Assert.That(health.TryApplyDamage(Hit(health, 1000)).AppliedAmount, Is.EqualTo(10));
            Assert.That(health.Current, Is.Zero);
        }

        [Test]
        public void MaximumChangeHasExplicitPolicyAndReductionClamps()
        {
            var health = new Health(100);
            health.TryApplyDamage(Hit(health, 50));
            health.ChangeMaxHealth(150);
            Assert.That(health.Current, Is.EqualTo(50));
            health.ChangeMaxHealth(200, MaxHealthPolicy.HealAddedCapacity);
            Assert.That(health.Current, Is.EqualTo(100));
            health.ChangeMaxHealth(30);
            Assert.That(health.Current, Is.EqualTo(30));
        }

        [Test]
        public void FriendlyFireAndInvulnerabilityAreSharedRules()
        {
            var health = new Health(100, Team.Player);
            Assert.That(health.TryApplyDamage(Hit(health, 10, Team.Player)).Rejection, Is.EqualTo(DamageRejection.FriendlyFire));
            health.Invulnerable = true;
            Assert.That(health.TryApplyDamage(Hit(health, 10)).Rejection, Is.EqualTo(DamageRejection.Invulnerable));
            health.Invulnerable = false;
            var allowed = new DamageContext(10, default, Team.Player, 2, health.LifeVersion, allowFriendlyFire: true);
            Assert.That(health.TryApplyDamage(allowed).AppliedAmount, Is.EqualTo(10));
        }

        [Test]
        public void NewLifeRejectsAnOldHitAndClearsInvulnerability()
        {
            var health = new Health(10, Team.Enemy);
            DamageContext old = Hit(health, 10, Team.Player);
            health.TryApplyDamage(old);
            health.Invulnerable = true;
            health.ResetForSpawn(20, Team.Enemy);
            Assert.That(health.TryApplyDamage(old).Rejection, Is.EqualTo(DamageRejection.StaleLife));
            Assert.That(health.Current, Is.EqualTo(20));
            Assert.That(health.Invulnerable, Is.False);
        }

        [Test]
        public void ComponentPublishesOneDeathAndCannotRespawnInsideItsDeathCallback()
        {
            var targetObject = new GameObject("health test");
            try
            {
                var target = targetObject.AddComponent<HealthComponent>();
                target.ResetForSpawn(10, Team.Enemy);
                int deaths = 0;
                DeathNotice recorded = default;
                target.Died += death =>
                {
                    deaths++;
                    recorded = death;
                    Assert.That(target.ResetForSpawn(10, Team.Enemy), Is.False);
                };
                var hit = new DamageContext(10, new EntityIdentity(42, 7), Team.Player, 99, target.LifeVersion);
                target.TryApplyDamage(hit);
                target.TryApplyDamage(hit);
                Assert.That(deaths, Is.EqualTo(1));
                Assert.That(recorded.Damage.Source.RuntimeId, Is.EqualTo(42));
                Assert.That(recorded.Damage.AttackId, Is.EqualTo(99));
                Assert.That(recorded.Target.LifeVersion, Is.EqualTo(hit.TargetLifeVersion));
                Assert.That(target.TryShopHeal(), Is.Zero);
                target.ResetForSpawn(20, Team.Enemy);
                Assert.That(target.TryApplyDamage(hit).Rejection, Is.EqualTo(DamageRejection.StaleLife));
            }
            finally { Object.DestroyImmediate(targetObject); }
        }
    }
}
