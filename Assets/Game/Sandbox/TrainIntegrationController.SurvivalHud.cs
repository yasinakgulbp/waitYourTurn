using UnityEngine;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    public sealed partial class TrainIntegrationController
    {
        private RunEconomy hudEconomy;
        private MoveInput hudInput;
        private GUIStyle hudLabel, hudButton, hudStat;
        private string hudStation = "", hudPhase = "", hudAmmo = "", hudReserve = "", hudHealth = "", hudCoins = "";
        private string[] hudGuns;
        private float hudRefresh;
        private void RefreshSurvivalHud()
        {
            if (run.Flow == null || Time.unscaledTime < hudRefresh) return;
            hudRefresh = Time.unscaledTime + .12f;
            hudEconomy ??= FindAnyObjectByType<RunEconomy>();
            hudInput ??= run.Player.GetComponent<MoveInput>();
            if (hudGuns == null)
            {
                hudGuns = new string[run.Weapon.WeaponCount];
                for (int i = 0; i < hudGuns.Length; i++) hudGuns[i] = run.Weapon.WeaponName(i);
            }
            hudStation = "SURVIVAL / STATION " + run.Flow.Station;
            hudPhase = run.Flow.Phase switch
            {
                RunPhase.Defense => "Defend the train", RunPhase.Approach => "Arriving at station",
                RunPhase.Departing => "Departing", RunPhase.Cruising => "On the move — repair and prepare",
                RunPhase.GameOver => "Run ended", _ => run.Flow.Phase.ToString()
            };
            if (run.Repair.Progress > 0) hudPhase += $" / Repair {run.Repair.Progress:P0}";
            hudAmmo = "III " + run.Weapon.Rounds;
            hudReserve = run.Weapon.State.Spec.InfiniteReserve ? "---" : run.Weapon.Reserve.ToString();
            hudHealth = "+ " + Mathf.CeilToInt(run.Player.Current);
            hudCoins = "$ " + (hudEconomy != null ? hudEconomy.Wallet.Balance : 0);
        }
        private void DrawSurvivalHud()
        {
            if (hudInput == null || hudGuns == null) return;
            hudLabel ??= new GUIStyle(GUI.skin.label) { fontSize = 16 };
            hudButton ??= new GUIStyle(GUI.skin.button) { fontSize = 14, wordWrap = true };
            hudStat ??= new GUIStyle(GUI.skin.box) { fontSize = 17, alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1, .88f, .18f) } };
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960f, safe.height / 540f));
            Vector2 offset = new Vector2(safe.x, Screen.height - safe.yMax);
            float width = safe.width / scale, height = safe.height / scale;
            hudInput.BlockedScreenArea = new Rect(offset.x, offset.y, safe.width, 85 * scale);
            hudInput.SecondaryBlockedScreenArea = new Rect(offset.x + safe.width * .28f,
                offset.y + safe.height - 100 * scale, safe.width * .72f, 100 * scale);
            hudInput.StatsBlockedScreenArea = new Rect(offset.x, offset.y + safe.height - 42 * scale, 282 * scale, 42 * scale);
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale);
            GUI.Label(new Rect(12, 8, width - 200, 24), hudStation, hudLabel);
            GUI.Label(new Rect(12, 32, width - 120, 28), hudPhase, hudLabel);
            GUI.enabled = !checking;
            if (GUI.Button(new Rect(width - 180, 8, 76, 25), "Restart"))
            { spawner.enabled = true; run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true; run.Restart(); }
            if (GUI.Button(new Rect(width - 98, 8, 86, 25), "Check (F10)")) StartCheck();
            float total = hudGuns.Length * 86;
            float gunStart = Mathf.Max(300, (width - total) * .5f);
            for (int i = 0; i < hudGuns.Length; i++)
            {
                GUI.enabled = !checking && run.Weapon.IsOwned(i);
                if (GUI.Button(new Rect(gunStart + i * 86, height - 55, 80, 43), hudGuns[i], hudButton)) run.Weapon.Equip(i);
            }
            GUI.enabled = true;
            GUI.Box(new Rect(12, height - 38, 68, 27), hudAmmo, hudStat);
            GUI.Box(new Rect(80, height - 38, 52, 27), hudReserve, hudStat);
            GUI.Box(new Rect(132, height - 38, 68, 27), hudHealth, hudStat);
            GUI.Box(new Rect(200, height - 38, 82, 27), hudCoins, hudStat);
            if (run.Failure != null || spawner.Diagnostic != null)
                GUI.Label(new Rect(12, 62, width - 24, 40), run.Failure ?? spawner.Diagnostic, hudLabel);
            GUI.matrix = old;
        }
    }
}
