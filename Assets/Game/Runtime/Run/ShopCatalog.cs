using System;
using UnityEngine;
using WaitYourTurn.Economy;

namespace WaitYourTurn.Run
{
    [Serializable]
    public sealed class ShopItem
    {
        public string id, label;
        [Min(1)] public int price;
        public ShopEffect effect;
        public int weaponIndex = -1;
        public int turretIndex = -1;
        [Range(0, 2)] public int reinforcementLevel;
        public bool requiresWeaponUnlock = true;
        public ShopProduct Product => new ShopProduct(id, price, effect, weaponIndex, turretIndex, reinforcementLevel, requiresWeaponUnlock);
    }
    [CreateAssetMenu(menuName = "Wait Your Turn/Shop Catalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [Min(0)] public int startingCoins;
        [Min(0)] public float purchaseCooldown = .35f;
        public ShopItem[] items;
        public bool Valid(int weaponCount)
        {
            if (startingCoins < 0 || float.IsNaN(purchaseCooldown) || float.IsInfinity(purchaseCooldown) ||
                purchaseCooldown < 0 || items == null || items.Length == 0 || items.Length > 32) return false;
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null || !item.Product.Valid || string.IsNullOrWhiteSpace(item.label) ||
                    item.effect == ShopEffect.Weapon && item.weaponIndex >= weaponCount) return false;
                for (int j = 0; j < i; j++) if (items[j].id == item.id) return false;
            }
            return true;
        }
    }
}
