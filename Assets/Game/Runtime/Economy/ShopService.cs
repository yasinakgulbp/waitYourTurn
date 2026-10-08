namespace WaitYourTurn.Economy
{
    public enum ShopEffect { Heal, RepairWagon, Weapon, Turret }
    public enum PurchaseResult { Success, Closed, StaleContext, Duplicate, InvalidProduct, Unavailable, InsufficientFunds, Busy, ApplyFailed }
    public readonly struct ShopProduct
    {
        public readonly string Id;
        public readonly int Price, WeaponIndex, TurretIndex;
        public readonly ShopEffect Effect;
        public ShopProduct(string id, int price, ShopEffect effect, int weaponIndex = -1, int turretIndex = -1)
        { Id = id; Price = price; Effect = effect; WeaponIndex = weaponIndex; TurretIndex = turretIndex; }
        public bool Valid => !string.IsNullOrWhiteSpace(Id) && Price > 0 &&
            (Effect == ShopEffect.Heal || Effect == ShopEffect.RepairWagon || Effect == ShopEffect.Weapon && WeaponIndex > 0 ||
                Effect == ShopEffect.Turret && TurretIndex >= 0);
    }
    public interface IShopEffects
    {
        bool CanApply(ShopProduct product);
        // Synchronous: false means no change. Network/ad callbacks must submit a new command, never apply here.
        bool TryApply(ShopProduct product);
    }
    /// <summary>No Unity, clock or UI dependency. Constant-size monotonic request guard, current-context only.</summary>
    public sealed class ShopService
    {
        private readonly Wallet wallet;
        private readonly IShopEffects effects;
        private ulong latestRequest;
        private bool busy;
        public ulong Context { get; private set; }
        public ShopService(Wallet currency, IShopEffects application) { wallet = currency; effects = application; }
        public void SetContext(ulong context) => Context = context;
        public PurchaseResult Buy(ShopProduct product, ulong context, ulong request, bool open)
        {
            if (busy) return PurchaseResult.Busy;
            if (context == 0 || context != Context) return PurchaseResult.StaleContext;
            if (request == 0 || request <= latestRequest) return PurchaseResult.Duplicate;
            latestRequest = request;
            if (!open) return PurchaseResult.Closed;
            if (!product.Valid) return PurchaseResult.InvalidProduct;
            if (!effects.CanApply(product)) return PurchaseResult.Unavailable;
            if (wallet.Available < product.Price) return PurchaseResult.InsufficientFunds;
            if (!wallet.Reserve(product.Price)) return PurchaseResult.Busy;
            busy = true; bool applied = false;
            try { applied = effects.TryApply(product); return applied ? PurchaseResult.Success : PurchaseResult.ApplyFailed; }
            finally { wallet.Finish(applied); busy = false; }
        }
    }
}
