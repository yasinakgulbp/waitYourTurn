using System.Collections.Generic;
using UnityEngine;
using WaitYourTurn.Economy;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Replaceable blockout UI; catalog/effects stay independent of the layout.</summary>
    public sealed class ShopHud : MonoBehaviour
    {
        [SerializeField] private RunEconomy economy;
        [SerializeField] private MoveInput input;
        private bool open;
        private ulong context;
        private string message = "";
        private GUIStyle card, title, body;
        private ShopCatalog displayedCatalog;
        private string[] captions;
        private int[][] groups;
        private readonly Vector2[] scroll = new Vector2[3];
        private int shownCoins = -1;
        private string money = "";
        public void Configure(RunEconomy owner, MoveInput controls) { economy = owner; input = controls; }
        private void Update()
        {
            if (open && (!economy.CanShop || context != economy.Context)) Close();
        }
        private void OnDisable() => Close();
        private void Close() { open = false; if (input != null) input.ModalBlockedScreenArea = default; }
        private void CacheCatalog()
        {
            if (displayedCatalog == economy.Catalog) return;
            displayedCatalog = economy.Catalog;
            var items = displayedCatalog.items;
            captions = new string[items.Length];
            var weapons = new List<int>(); var supplies = new List<int>(); var defenses = new List<int>();
            for (int i = 0; i < items.Length; i++)
            {
                captions[i] = items[i].label + "\n$ " + items[i].price;
                switch (items[i].effect)
                {
                    case ShopEffect.Weapon: weapons.Add(i); break;
                    case ShopEffect.Turret: case ShopEffect.Drone: defenses.Add(i); break;
                    default: supplies.Add(i); break;
                }
            }
            groups = new[] { weapons.ToArray(), supplies.ToArray(), defenses.ToArray() };
        }
        private void OnGUI()
        {
            if (economy == null) return;
            if (!economy.CanShop) { Close(); return; }
            CacheCatalog();
            card ??= new GUIStyle(GUI.skin.button) { fontSize = 14, wordWrap = true };
            title ??= new GUIStyle(GUI.skin.label) { fontSize = 17 };
            body ??= new GUIStyle(GUI.skin.label) { wordWrap = true };
            if (shownCoins != economy.Wallet.Balance) { shownCoins = economy.Wallet.Balance; money = "$ " + shownCoins; }
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960f, safe.height / 540f));
            Vector2 offset = new Vector2(safe.x, Screen.height - safe.yMax);
            float width = safe.width / scale, height = safe.height / scale;
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale);
            bool survival = economy.Run.UsesOpenTrainSurvival;
            if (!survival) GUI.Label(new Rect(width - 285, 48, 140, 28), money, title);
            if (GUI.Button(new Rect(width - 98, 48, 86, 32), open ? "Close shop" : "Shop"))
            {
                if (open) Close();
                else { open = true; context = economy.Context; message = "The game continues while shopping."; }
            }
            if (open && context == economy.Context && economy.CanShop)
            {
                Rect panel = survival ? new Rect(width - 662, 95, 650, height - 160) :
                    new Rect(width - 285, 95, 273, Mathf.Min(height - 160, captions.Length * 40 + 105));
                input.ModalBlockedScreenArea = new Rect(offset.x + panel.x * scale, offset.y + panel.y * scale, panel.width * scale, panel.height * scale);
                GUI.Box(panel, "");
                GUI.Label(new Rect(panel.x + 12, panel.y + 8, panel.width - 24, 25), "SHOP   " + money, title);
                if (survival)
                {
                    float left = panel.x + 12, right = panel.x + 225, groupWidth = panel.width - 237;
                    float usable = panel.height - 75, suppliesHeight = usable * .52f;
                    DrawGroup(0, "Weapons", new Rect(left, panel.y + 37, 201, usable), 1);
                    DrawGroup(1, "Health / Doors", new Rect(right, panel.y + 37, groupWidth, suppliesHeight), 2);
                    DrawGroup(2, "Defenses", new Rect(right, panel.y + 43 + suppliesHeight, groupWidth, usable - suppliesHeight - 6), 3);
                }
                else
                {
                    Rect region = new Rect(panel.x + 10, panel.y + 36, panel.width - 20, panel.height - 80);
                    scroll[0] = GUI.BeginScrollView(region, scroll[0], new Rect(0, 0, region.width - 18, captions.Length * 40));
                    for (int i = 0; i < captions.Length; i++) DrawProduct(i, new Rect(0, i * 40, region.width - 18, 36));
                    GUI.EndScrollView();
                }
                GUI.Label(new Rect(panel.x + 12, panel.yMax - 32, panel.width - 24, 30), message, body);
            }
            else input.ModalBlockedScreenArea = default;
            GUI.enabled = true; GUI.matrix = old;
        }
        private void DrawGroup(int group, string heading, Rect area, int columns)
        {
            GUI.Label(new Rect(area.x, area.y, area.width, 20), heading, body);
            var viewport = new Rect(area.x, area.y + 23, area.width, area.height - 23);
            int rows = (groups[group].Length + columns - 1) / columns;
            float contentWidth = viewport.width - 18, cellWidth = contentWidth / columns;
            scroll[group] = GUI.BeginScrollView(viewport, scroll[group], new Rect(0, 0, contentWidth, rows * 66));
            for (int n = 0; n < groups[group].Length; n++)
                DrawProduct(groups[group][n], new Rect(n % columns * cellWidth, n / columns * 66, cellWidth - 5, 61));
            GUI.EndScrollView();
        }
        private void DrawProduct(int index, Rect area)
        {
            var item = economy.Catalog.items[index];
            GUI.enabled = economy.CanApply(item.Product) && economy.Wallet.Available >= item.price;
            if (GUI.Button(area, captions[index], card))
            {
                var result = economy.Buy(index, context, economy.NextRequest());
                message = result == PurchaseResult.Success ? "Purchased: " + item.label : result.ToString();
            }
            GUI.enabled = true;
        }
    }
}
