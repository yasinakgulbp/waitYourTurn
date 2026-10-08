using UnityEngine;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Replaceable PC/mobile test display. Does not produce gameplay outcomes.</summary>
    public sealed class BattleHud : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunMatch match;
        public void Configure(RunDriver owner, RunMatch participants) { run = owner; match = participants; }
        private void OnGUI()
        {
            if (match.Suppressed || run.Flow == null || run.Flow.Paused && !run.Flow.Terminal) return;
            Rect safe = Screen.safeArea; float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960f, safe.height / 540f));
            Vector2 offset = new Vector2(safe.x, Screen.height - safe.yMax); float width = safe.width / scale;
            Matrix4x4 previous = GUI.matrix; GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1));
            var style = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            if (match.Active && match.Roster != null)
            {
                GUI.Label(new Rect(12, 82, width - 310, 22), $"BATTLE | Survivors {match.Roster.Living}/{match.Roster.Count} | F7: bot checks", style);
                for (int i = 0; i < match.Bots.Length; i++)
                {
                    var bot = match.Bots[i]; int place = match.Roster.Place(i + 1);
                    GUI.Label(new Rect(12, 130 + i * 18, 260, 20), $"{bot.Nickname} [{bot.Profile.skill}] {(bot.Health.IsAlive ? $"{bot.Wagon?.Id} / HP {bot.Health.Current:F0}" : $"ELIMINATED #{place}")}", style);
                    if (bot.Health.IsAlive && Camera.main != null)
                    {
                        Vector3 p = Camera.main.WorldToScreenPoint(bot.transform.position + Vector3.up * 1.85f);
                        if (p.z > 0 && p.x > safe.x && p.x < safe.xMax && p.y > safe.y && p.y < safe.yMax)
                            GUI.Label(new Rect((p.x - offset.x) / scale - 50, (Screen.height - p.y - offset.y) / scale - 20, 110, 20), bot.Nickname + " [BOT]", style);
                    }
                }
                if (run.Flow.Terminal)
                    GUI.Box(new Rect(width / 2 - 160, 230, 320, 65), $"BATTLE {match.Result}\nYour place: {match.HumanPlace}/{match.Roster.Count}");
            }
            GUI.enabled = !run.Flow.Paused || run.Flow.Terminal;
            if (GUI.Button(new Rect(width - 285, 82, 273, 24), match.Mode == RunMode.Battle ? "Switch to SOLO (new run)" : "Switch to BATTLE (new run)"))
                match.SetMode(match.Mode == RunMode.Battle ? RunMode.Solo : RunMode.Battle);
            GUI.enabled = true; GUI.matrix = previous;
            run.Player.GetComponent<MoveInput>().SecondaryBlockedScreenArea = new Rect(offset.x, offset.y + safe.height - 62 * scale, safe.width, 62 * scale);
        }
    }
}
