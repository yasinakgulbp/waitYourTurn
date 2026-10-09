using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Economy;

namespace WaitYourTurn.Run
{
    /// <summary>Run composition adapter: pool death -> owner currency, shop effect -> existing gameplay APIs.</summary>
    public sealed class RunEconomy : MonoBehaviour, IShopEffects
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private ShopCatalog catalog;
        [SerializeField] private RunDefenses defenses;
        [SerializeField] private RunDrone drone;
        [SerializeField] private RunProfile profile;
        [SerializeField] private RunReinforcement reinforcement;
        public RunDriver Run => run;
        public RunReinforcement Reinforcement => reinforcement;
        public void ConfigureReinforcement(RunReinforcement strength) => reinforcement = strength;
        public RunProfile Profile => profile;
        public void ConfigureProfile(RunProfile permanent) => profile = permanent;
        public RunDrone Drone => drone;
        public void ConfigureDrone(RunDrone defense) => drone = defense;
        public RunDefenses Defenses => defenses;
        public void ConfigureDefenses(RunDefenses turrets) => defenses = turrets;
        private readonly Wallet wallet = new Wallet();
        private KillRewards rewards;
        private ShopService shop;
        private RunFlow observedFlow;
        private ulong context, nextRequest;
        private float nextPurchase;
        public Wallet Wallet => wallet;
        public ShopCatalog Catalog => catalog;
        public ulong Context => context;
        public int TrackedActors => rewards != null ? rewards.TrackedActors : 0;
        public string Diagnostic { get; private set; }
        public bool CanShop => isActiveAndEnabled && Diagnostic == null && run.Flow != null &&
            !run.Flow.Paused && run.Player.IsAlive;
        public void Configure(RunDriver owner, ShopCatalog products) { run = owner; catalog = products; }
        public ulong NextRequest() => ++nextRequest;
        public float PurchaseRemaining => Mathf.Max(0, nextPurchase - Time.unscaledTime);
        public void RestoreWallet(int coins, float remaining)
        { wallet.Reset(coins); rewards.BeginRun(run.Player.Identity); context++; shop.SetContext(context); nextPurchase = Time.unscaledTime + remaining; }
        private void OnEnable()
        {
            if (run == null || catalog == null || !catalog.Valid(run.Weapon.WeaponCount))
            { Diagnostic = "Invalid shop catalog/run binding."; Debug.LogError(Diagnostic, this); return; }
            Diagnostic = null;
            if (shop == null) { rewards = new KillRewards(wallet); shop = new ShopService(wallet, this); }
            run.Assigned += OnAssigned;
            foreach (var wagon in run.Wagons) wagon.Enemies.Killed += OnKilled;
            if (run.Flow != null) OnAssigned(run.CurrentWagon);
        }
        private void OnDisable()
        {
            if (run == null) return;
            run.Assigned -= OnAssigned;
            foreach (var wagon in run.Wagons) wagon.Enemies.Killed -= OnKilled;
        }
        private void OnAssigned(WagonRuntime wagon)
        {
            if (observedFlow != run.Flow)
            {
                observedFlow = run.Flow; wallet.Reset(catalog.startingCoins);
                rewards.BeginRun(run.Player.Identity); nextPurchase = 0;
            }
            context++; shop.SetContext(context);
        }
        private void OnKilled(DeathNotice death, int reward) => rewards.Observe(death, reward,
            isActiveAndEnabled && run.Flow != null && !run.Flow.Paused && run.Player.IsAlive);
        public PurchaseResult Buy(int index, ulong expectedContext, ulong request)
        {
            if (shop == null) return PurchaseResult.Closed;
            if (expectedContext != context) return PurchaseResult.StaleContext;
            if (catalog.items == null || index < 0 || index >= catalog.items.Length || catalog.items[index] == null)
                return PurchaseResult.InvalidProduct;
            if (CanShop && Time.unscaledTime < nextPurchase) return PurchaseResult.Busy;
            var result = shop.Buy(catalog.items[index].Product, expectedContext, request, CanShop);
            if (result == PurchaseResult.Success) nextPurchase = Time.unscaledTime + catalog.purchaseCooldown;
            return result;
        }
        public bool CanApply(ShopProduct product)
        {
            if (!CanShop) return false;
            switch (product.Effect)
            {
                case ShopEffect.Heal: return run.Player.Current < run.Player.Maximum;
                case ShopEffect.RepairWagon:
                    foreach (var door in run.CurrentWagon.Doors) if (door.NeedsRepair) return true;
                    return false;
                case ShopEffect.Weapon: return (profile == null || profile.WeaponUnlocked(product.WeaponIndex)) && run.Weapon.CanGrantOrRefill(product.WeaponIndex);
                case ShopEffect.Turret: return defenses != null && defenses.CanBuy(product.TurretIndex);
                case ShopEffect.Drone: return drone != null && drone.CanBuy;
                case ShopEffect.ReinforceDoors: return reinforcement != null && reinforcement.CanUpgrade(product.ReinforcementLevel);
                default: return false;
            }
        }
        bool IShopEffects.TryApply(ShopProduct product)
        {
            if (!CanApply(product)) return false;
            switch (product.Effect)
            {
                case ShopEffect.Heal: return run.Player.TryShopHeal() > 0;
                case ShopEffect.RepairWagon:
                    bool repaired = false;
                    foreach (var door in run.CurrentWagon.Doors) if (door.TryRepair()) repaired = true;
                    if (repaired) run.Repair.Cancel();
                    return repaired;
                case ShopEffect.Weapon: return run.Weapon.TryGrantOrRefill(product.WeaponIndex);
                case ShopEffect.Turret: return defenses != null && defenses.TryBuy(product.TurretIndex);
                case ShopEffect.Drone: return drone != null && drone.TryBuy();
                case ShopEffect.ReinforceDoors: return reinforcement != null && reinforcement.TryUpgrade(product.ReinforcementLevel);
                default: return false;
            }
        }
    }
}
