using UnityEditor;
using UnityEngine;
using System.Linq;
using WaitYourTurn.Economy;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    public static class ShopContentBuilder
    {
        public static ShopCatalog EnsureCatalog(bool survival = false)
        {
            const string folder = "Assets/Game/Content/Economy";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Game/Content", "Economy");
            string path = folder + (survival ? "/SurvivalShop.asset" : "/RunShop.asset");
            var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(path);
            if (catalog != null) return catalog;
            if (survival)
            {
                catalog = Object.Instantiate(EnsureCatalog());
                catalog.startingCoins = 10000;
                foreach (var item in catalog.items)
                {
                    if (item.effect == ShopEffect.Weapon) item.requiresWeaponUnlock = false; // Temporary Survival equipment testing.
                    switch (item.id)
                    {
                        case "heal": item.price = 150; break;
                        case "smg": item.price = 300; break;
                        case "rifle": item.price = 2500; break;
                        case "shotgun": item.price = 1000; break;
                        case "turret-normal": item.price = 500; break;
                        case "turret-advanced": item.price = 900; break;
                    }
                }
                var products = new System.Collections.Generic.List<ShopItem>(catalog.items);
                products.Add(new ShopItem { id = "wood", label = "Wood reinforcement", price = 600,
                    effect = ShopEffect.ReinforceDoors, reinforcementLevel = 1 });
                products.Add(new ShopItem { id = "wire", label = "Wire reinforcement", price = 1200,
                    effect = ShopEffect.ReinforceDoors, reinforcementLevel = 2 });
                catalog.items = products.ToArray();
                AddAmmoProducts(catalog);
                AssetDatabase.CreateAsset(catalog, path); return catalog;
            }
            catalog = ScriptableObject.CreateInstance<ShopCatalog>();
            catalog.items = new[] {
                Item("heal", "Heal", 30, ShopEffect.Heal), Item("repair", "Repair wagon doors", 40, ShopEffect.RepairWagon),
                Item("smg", "SMG / refill", 60, ShopEffect.Weapon, 1),
                Item("rifle", "Rifle / refill", 100, ShopEffect.Weapon, 2),
                Item("shotgun", "Shotgun / refill", 90, ShopEffect.Weapon, 3) };
            AssetDatabase.CreateAsset(catalog, path); return catalog;
        }
        private static ShopItem Item(string id, string label, int price, ShopEffect effect, int weapon = -1) =>
            new ShopItem { id = id, label = label, price = price, effect = effect, weaponIndex = weapon };
        public static void AddAmmoProducts(ShopCatalog catalog)
        {
            var products = new System.Collections.Generic.List<ShopItem>(catalog.items);
            foreach (var weapon in catalog.items.Where(item => item.effect == ShopEffect.Weapon))
            {
                weapon.refillsOwnedWeapon = false; weapon.label = weapon.label.Replace(" / refill", "");
                string id = "ammo-" + weapon.id;
                if (products.Any(item => item.id == id)) continue;
                int price = weapon.weaponIndex switch { 1 => 50, 2 => 300, 3 => 125, 4 => 750, _ => Mathf.Max(25, weapon.price / 8) };
                products.Add(new ShopItem { id = id, label = weapon.label + " +1 magazine", price = price,
                    effect = ShopEffect.Ammo, weaponIndex = weapon.weaponIndex, requiresWeaponUnlock = false });
            }
            catalog.items = products.ToArray(); EditorUtility.SetDirty(catalog);
        }
    }
}
