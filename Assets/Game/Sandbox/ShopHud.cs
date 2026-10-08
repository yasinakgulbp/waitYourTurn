using UnityEngine;
using WaitYourTurn.Economy;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Replaceable blockout UI; the economy still works with this component disabled.</summary>
    public sealed class ShopHud : MonoBehaviour
    {
        [SerializeField] private RunEconomy economy;
        [SerializeField] private MoveInput input;
        private bool open;
        private ulong context;
        private string message = "Kills earn coins. Shop stays in real time.";
        public void Configure(RunEconomy owner, MoveInput controls) { economy = owner; input = controls; }
        private void Update()
        {
            if (open && (!economy.CanShop || context != economy.Context)) Close();
        }
        private void OnDisable() => Close();
        private void Close() { open = false; if (input != null) input.ModalBlockedScreenArea = default; }
        private void OnGUI()
        {
            if (economy == null) return;
            if (!economy.CanShop) { Close(); return; }
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960f, safe.height / 540f));
            Vector2 offset = new Vector2(safe.x, Screen.height - safe.yMax);
            float width = safe.width / scale;
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1));
            GUI.Label(new Rect(width - 285, 48, 140, 28), $"Coins: {economy.Wallet.Balance}", new GUIStyle(GUI.skin.label) { fontSize = 17 });
            GUI.enabled = economy.CanShop;
            if (GUI.Button(new Rect(width - 98, 48, 86, 32), open ? "Close shop" : "Shop"))
            { if (open) Close(); else { open = true; context = economy.Context; message = "Real time: keep moving / repairing."; } }
            GUI.enabled = true;
            if (open && context == economy.Context && economy.CanShop)
            {
                Rect panel = new Rect(width - 285, 95, 273, 310);
                input.ModalBlockedScreenArea = new Rect(offset.x + panel.x * scale, offset.y + panel.y * scale, panel.width * scale, panel.height * scale);
                GUI.Box(panel, "SHOP - test prices");
                var items = economy.Catalog.items;
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    GUI.enabled = economy.CanApply(item.Product) && economy.Wallet.Available >= item.price;
                    if (GUI.Button(new Rect(panel.x + 10, panel.y + 30 + i * 45, panel.width - 20, 40), $"{item.label}  ({item.price})"))
                    {
                        var result = economy.Buy(i, context, economy.NextRequest());
                        message = result == PurchaseResult.Success ? "Purchased: " + item.label : result.ToString();
                    }
                }
                GUI.enabled = true;
                GUI.Label(new Rect(panel.x + 10, panel.y + 265, panel.width - 20, 40), message,
                    new GUIStyle(GUI.skin.label) { wordWrap = true });
            }
            else input.ModalBlockedScreenArea = default;
            if (economy.Diagnostic != null) GUI.Label(new Rect(12, 120, width - 24, 40), economy.Diagnostic);
            GUI.matrix = old;
        }
    }
}
