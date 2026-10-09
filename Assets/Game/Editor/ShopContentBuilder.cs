using UnityEditor;
using UnityEngine;
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
                var products = new System.Collections.Generic.List<ShopItem>(catalog.items);
                products.Add(new ShopItem { id = "wood", label = "Wood reinforcement", price = 80,
                    effect = ShopEffect.ReinforceDoors, reinforcementLevel = 1 });
                products.Add(new ShopItem { id = "wire", label = "Wire reinforcement", price = 160,
                    effect = ShopEffect.ReinforceDoors, reinforcementLevel = 2 });
                catalog.items = products.ToArray();
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
    }
}
