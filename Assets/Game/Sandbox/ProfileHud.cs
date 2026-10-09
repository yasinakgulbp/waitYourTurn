using UnityEngine;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Temporary result/profile display; production main-menu art comes later.</summary>
    public sealed class ProfileHud : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunProfile profile;
        [SerializeField] private MoveInput input;
        public void Configure(RunDriver owner, RunProfile permanent, MoveInput controls) { run = owner; profile = permanent; input = controls; }
        private void OnDisable() { if (input != null) input.ProfileBlockedScreenArea = default; }
        private void OnGUI()
        {
            input.ProfileBlockedScreenArea = default;
            if (run.Flow == null || !profile.Active || run.Flow.Paused && !run.Flow.Terminal) return;
            Rect safe = Screen.safeArea; float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960f, safe.height / 540f));
            Vector2 offset = new Vector2(safe.x, Screen.height - safe.yMax); float width = safe.width / scale;
            var old = GUI.matrix; GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1));
            GUI.Label(new Rect(width - 285, 110, 273, 25), $"Kalıcı jeton: {profile.Tokens} (deneme)");
            if (run.Flow.Phase == RunPhase.GameOver)
            {
                var panel = new Rect(width - 312, 144, 300, 356);
                input.ProfileBlockedScreenArea = new Rect(offset.x + panel.x * scale, offset.y + panel.y * scale, panel.width * scale, panel.height * scale);
                GUI.Box(panel, "PROFİL / KALICI SİLAHLAR");
                GUI.Label(new Rect(panel.x + 12, panel.y + 28, 275, 40), $"Ödül +{profile.LastReward} | Solo rekor: {profile.BestSoloStation}\nKoşu altını yeni oyunda sıfırlanır.");
                for (int i = 1; i < run.Weapon.WeaponCount; i++)
                {
                    bool unlocked = profile.WeaponUnlocked(i);
                    GUI.enabled = profile.CanUnlock(i);
                    if (GUI.Button(new Rect(panel.x + 12, panel.y + 78 + (i - 1) * 35, 276, 31),
                        run.Weapon.WeaponName(i) + (unlocked ? " — Kalıcı açık" : $" — Aç: {profile.WeaponPrice(i)} jeton"))) profile.TryUnlock(i);
                }
                UpgradeButton(PermanentUpgrade.Health, "Can", 185);
                UpgradeButton(PermanentUpgrade.Range, "Menzil", 220);
                GUI.enabled = true;
                GUI.Label(new Rect(panel.x + 12, panel.y + 255, 276, 47), profile.Status ?? "Kalıcı gelişim iki modda geçerlidir.", new GUIStyle(GUI.skin.label) { wordWrap = true });
                if (GUI.Button(new Rect(panel.x + 12, panel.y + 307, 276, 32), "Yeni koşu — en baştan")) run.Restart();
                void UpgradeButton(PermanentUpgrade kind, string label, float y)
                {
                    int level = profile.UpgradeLevel(kind), price = profile.UpgradePrice(kind);
                    float bonus = (profile.UpgradeMultiplier(kind) - 1) * 100;
                    GUI.enabled = profile.CanUpgrade(kind);
                    if (GUI.Button(new Rect(panel.x + 12, panel.y + y, 276, 31),
                        $"{label} +%{bonus:F0} [{level}/5] — " + (price > 0 ? $"{price} jeton" : "Tamamlandı"))) profile.TryUpgrade(kind);
                }
            }
            GUI.matrix = old;
        }
    }
}
