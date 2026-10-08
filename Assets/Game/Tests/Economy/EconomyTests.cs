using System;
using NUnit.Framework;
using WaitYourTurn.Combat;
using WaitYourTurn.Economy;

namespace WaitYourTurn.Tests
{
    public sealed class EconomyTests
    {
        private sealed class Effects : IShopEffects
        {
            public bool Available = true, Applies = true;
            public int Calls;
            public Action During;
            public bool CanApply(ShopProduct product) => Available;
            public bool TryApply(ShopProduct product) { Calls++; During?.Invoke(); return Applies; }
        }
        private static readonly ShopProduct Product = new ShopProduct("heal", 30, ShopEffect.Heal);
        [Test] public void DroneUsesExistingTransactionAndAvailabilityGuards()
        {
            var product = new ShopProduct("drone", 180, ShopEffect.Drone);
            Assert.That(product.Valid, Is.True);
            var wallet = new Wallet(); wallet.Reset(1000); var effects = new Effects();
            var shop = new ShopService(wallet, effects); shop.SetContext(1);
            Assert.That(shop.Buy(product, 1, 1, true), Is.EqualTo(PurchaseResult.Success));
            Assert.That(shop.Buy(product, 1, 1, true), Is.EqualTo(PurchaseResult.Duplicate));
            effects.Available = false;
            Assert.That(shop.Buy(product, 1, 2, true), Is.EqualTo(PurchaseResult.Unavailable));
            Assert.That(wallet.Balance, Is.EqualTo(820)); Assert.That(effects.Calls, Is.EqualTo(1));
        }
        [Test] public void TurretProductRequiresAnExplicitTypeAndUsesTransactionalGuard()
        {
            Assert.That(new ShopProduct("turret", 90, ShopEffect.Turret).Valid, Is.False);
            var product = new ShopProduct("turret", 90, ShopEffect.Turret, turretIndex: 0);
            Assert.That(product.Valid, Is.True);
            var wallet = new Wallet(); wallet.Reset(100); var effects = new Effects(); var shop = new ShopService(wallet, effects);
            shop.SetContext(1); Assert.That(shop.Buy(product, 1, 1, true), Is.EqualTo(PurchaseResult.Success));
            Assert.That(shop.Buy(product, 1, 1, true), Is.EqualTo(PurchaseResult.Duplicate));
            Assert.That(wallet.Balance, Is.EqualTo(10)); Assert.That(effects.Calls, Is.EqualTo(1));
        }
        [Test] public void WalletRejectsNegativeOverspendAndOverflow()
        {
            var wallet = new Wallet(); Assert.That(wallet.Reset(-1), Is.False);
            Assert.That(wallet.TryCredit(0), Is.False); Assert.That(wallet.TryCredit(-1), Is.False);
            Assert.That(wallet.TrySpend(-1), Is.False); Assert.That(wallet.TrySpend(1), Is.False);
            Assert.That(wallet.TryCredit(int.MaxValue)); Assert.That(wallet.TryCredit(1), Is.False);
            Assert.That(wallet.TrySpend(int.MaxValue)); Assert.That(wallet.Balance, Is.Zero);
        }
        [Test] public void SuccessfulCommandDebitsOnceAndOldCommandsNeverReplay()
        {
            var wallet = new Wallet(); wallet.Reset(100); var effects = new Effects(); var shop = new ShopService(wallet, effects);
            shop.SetContext(1);
            Assert.That(shop.Buy(Product, 1, 1, true), Is.EqualTo(PurchaseResult.Success));
            Assert.That(shop.Buy(Product, 1, 1, true), Is.EqualTo(PurchaseResult.Duplicate));
            Assert.That(shop.Buy(Product, 1, 2, true), Is.EqualTo(PurchaseResult.Success));
            Assert.That(shop.Buy(Product, 1, 1, true), Is.EqualTo(PurchaseResult.Duplicate));
            Assert.That(wallet.Balance, Is.EqualTo(40)); Assert.That(effects.Calls, Is.EqualTo(2));
        }
        [Test] public void RejectedPurchasesNeverChangeMoneyOrApplyEffects()
        {
            var wallet = new Wallet(); wallet.Reset(20); var effects = new Effects(); var shop = new ShopService(wallet, effects); shop.SetContext(2);
            Assert.That(shop.Buy(Product, 1, 1, true), Is.EqualTo(PurchaseResult.StaleContext));
            Assert.That(shop.Buy(Product, 2, 1, false), Is.EqualTo(PurchaseResult.Closed));
            Assert.That(shop.Buy(default, 2, 2, true), Is.EqualTo(PurchaseResult.InvalidProduct));
            effects.Available = false;
            Assert.That(shop.Buy(Product, 2, 3, true), Is.EqualTo(PurchaseResult.Unavailable)); effects.Available = true;
            Assert.That(shop.Buy(Product, 2, 4, true), Is.EqualTo(PurchaseResult.InsufficientFunds));
            Assert.That(effects.Calls, Is.Zero); Assert.That(wallet.Balance, Is.EqualTo(20));
        }
        [Test] public void FailedApplyReleasesFundsEvenWithIncomeDuringApplication()
        {
            var wallet = new Wallet(); wallet.Reset(100); var effects = new Effects { Applies = false };
            effects.During = () => { Assert.That(wallet.Available, Is.EqualTo(70)); Assert.That(wallet.TrySpend(71), Is.False); wallet.TryCredit(10); };
            var shop = new ShopService(wallet, effects); shop.SetContext(1);
            Assert.That(shop.Buy(Product, 1, 1, true), Is.EqualTo(PurchaseResult.ApplyFailed));
            Assert.That(wallet.Balance, Is.EqualTo(110)); Assert.That(wallet.Available, Is.EqualTo(110));
        }
        [Test] public void ReentrantCallbacksCannotPurchaseOrResetReservedWallet()
        {
            var wallet = new Wallet(); wallet.Reset(100); var effects = new Effects(); var shop = new ShopService(wallet, effects); shop.SetContext(1);
            effects.During = () => { Assert.That(shop.Buy(Product, 1, 2, true), Is.EqualTo(PurchaseResult.Busy)); Assert.That(wallet.Reset(1000), Is.False); };
            Assert.That(shop.Buy(Product, 1, 1, true), Is.EqualTo(PurchaseResult.Success));
            Assert.That(wallet.Balance, Is.EqualTo(70)); Assert.That(effects.Calls, Is.EqualTo(1));
        }
        [Test] public void ExceptionReleasesReservationWithoutCharging()
        {
            var wallet = new Wallet(); wallet.Reset(100); var effects = new Effects { During = () => throw new InvalidOperationException() };
            var shop = new ShopService(wallet, effects); shop.SetContext(1);
            Assert.Throws<InvalidOperationException>(() => shop.Buy(Product, 1, 1, true));
            Assert.That(wallet.Balance, Is.EqualTo(100)); Assert.That(wallet.Available, Is.EqualTo(100));
            effects.During = null; Assert.That(shop.Buy(Product, 1, 2, true), Is.EqualTo(PurchaseResult.Success));
        }
        [Test] public void AssignmentInvalidatesOldContextEvenForUnusedRequest()
        {
            var wallet = new Wallet(); wallet.Reset(100); var effects = new Effects(); var shop = new ShopService(wallet, effects);
            shop.SetContext(1); shop.SetContext(2);
            Assert.That(shop.Buy(Product, 1, 100, true), Is.EqualTo(PurchaseResult.StaleContext));
            Assert.That(shop.Buy(Product, 2, 1, true), Is.EqualTo(PurchaseResult.Success));
        }
        private static DeathNotice Death(uint life, EntityIdentity credit, Team source = Team.Player, bool killed = true) =>
            new DeathNotice(new EntityIdentity(7, life), new DamageContext(10, new EntityIdentity(999, 1), source, 1, life, rewardOwner: credit), new DamageResult(10, killed));
        [Test] public void OwnedTurretCreditWorksOncePerLifeAndOldLifeCannotReplay()
        {
            var wallet = new Wallet(); var rewards = new KillRewards(wallet); var owner = new EntityIdentity(1, 3); rewards.BeginRun(owner);
            Assert.That(rewards.Observe(Death(2, owner), 8, true));
            Assert.That(rewards.Observe(Death(2, owner), 8, true), Is.False);
            Assert.That(rewards.Observe(Death(3, owner), 10, true));
            Assert.That(rewards.Observe(Death(2, owner), 8, true), Is.False);
            Assert.That(wallet.Balance, Is.EqualTo(18)); Assert.That(rewards.TrackedActors, Is.EqualTo(1));
        }
        [Test] public void ForeignOrOldOwnerCannotClaimDeathLater()
        {
            var wallet = new Wallet(); var rewards = new KillRewards(wallet); var owner = new EntityIdentity(1, 3); rewards.BeginRun(owner);
            Assert.That(rewards.Observe(Death(1, new EntityIdentity(2, 3)), 8, true), Is.False);
            Assert.That(rewards.Observe(Death(1, owner), 8, true), Is.False);
            Assert.That(rewards.Observe(Death(2, new EntityIdentity(1, 2)), 8, true), Is.False);
            Assert.That(rewards.Observe(Death(3, owner, Team.Enemy), 8, true), Is.False);
            Assert.That(wallet.Balance, Is.Zero);
        }
        [Test] public void NonDeathPausedAndNewRunCannotLeakRewards()
        {
            var wallet = new Wallet(); var rewards = new KillRewards(wallet); var owner = new EntityIdentity(1, 3); rewards.BeginRun(owner);
            Assert.That(rewards.Observe(Death(1, owner, killed: false), 8, true), Is.False);
            Assert.That(rewards.Observe(Death(1, owner), 8, false), Is.False);
            Assert.That(wallet.Balance, Is.Zero); Assert.That(rewards.TrackedActors, Is.Zero);
            rewards.BeginRun(new EntityIdentity(1, 4));
            Assert.That(rewards.Observe(Death(2, owner), 8, true), Is.False);
            Assert.That(wallet.Balance, Is.Zero);
        }
        [Test] public void ThousandsOfPooledLivesKeepOneLedgerEntry()
        {
            var wallet = new Wallet(); var rewards = new KillRewards(wallet); var owner = new EntityIdentity(1, 1); rewards.BeginRun(owner);
            for (uint life = 1; life <= 10000; life++) Assert.That(rewards.Observe(Death(life, owner), 1, true));
            Assert.That(rewards.TrackedActors, Is.EqualTo(1)); Assert.That(wallet.Balance, Is.EqualTo(10000));
        }
    }
}
