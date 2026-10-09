using UnityEngine;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Positions only: no render texture, extra world camera or scene searches per frame.</summary>
    public sealed class SurvivalMiniMap : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private MoveInput input;
        private Bounds world;
        public void Configure(RunDriver owner, MoveInput controls) { run = owner; input = controls; }
        public static Rect Panel(float width, float height) => new Rect(width - 214, height - 122, 202, 110);
        private void Start()
        {
            float first = run.Wagons[0].transform.position.x, last = run.Wagons[^1].transform.position.x;
            world = new Bounds(new Vector3((first + last) * .5f, 0, 0), new Vector3(last - first + 14, 1, 32));
        }
        private void OnGUI()
        {
            if (!run.UsesOpenTrainSurvival || run.Flow == null || input.ModalBlockedScreenArea.width > 0 ||
                input.ProfileBlockedScreenArea.width > 0 || Event.current.type != EventType.Repaint) return;
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960, safe.height / 540));
            Vector2 offset = new Vector2(safe.x, Screen.height - safe.yMax);
            Rect panel = Panel(safe.width / scale, safe.height / scale);
            Matrix4x4 matrix = GUI.matrix; Color color = GUI.color; int depth = GUI.depth;
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale); GUI.depth = 20;
            Rect area = new Rect(panel.x + 8, panel.y + 8, panel.width - 16, panel.height - 16);
            Vector2 Point(Vector3 p) => new Vector2(Mathf.Lerp(area.xMin, area.xMax, Mathf.InverseLerp(world.min.x, world.max.x, p.x)),
                Mathf.Lerp(area.yMax, area.yMin, Mathf.InverseLerp(world.min.z, world.max.z, p.z)));
            foreach (var wagon in run.Wagons)
            {
                Bounds room = wagon.Geometry.Interior;
                Vector2 a = Point(wagon.transform.TransformPoint(room.min)), b = Point(wagon.transform.TransformPoint(room.max));
                Fill(Rect.MinMaxRect(a.x, b.y, b.x, a.y), wagon == run.CurrentWagon ? new Color(.22f, .42f, .38f) : new Color(.18f, .25f, .29f));
                foreach (var enemy in wagon.Enemies.Active)
                {
                    if (!enemy.Health.IsAlive) continue;
                    Vector2 p = Point(enemy.transform.position);
                    Fill(new Rect(p.x - 1.5f, p.y - 1.5f, 3, 3), enemy.OnBoard ? new Color(1, .34f, .12f) : new Color(.85f, .15f, .16f));
                }
            }
            foreach (var passage in run.Solo.Connections)
            {
                Vector2 p = Point(passage.transform.position);
                Fill(new Rect(p.x - 3, p.y - 3, 6, 6), new Color(.45f, .58f, .58f));
            }
            Vector2 hero = Point(run.Player.transform.position);
            Fill(new Rect(hero.x - 2.5f, hero.y - 2.5f, 5, 5), new Color(.2f, 1, .3f));
            GUI.matrix = matrix; GUI.color = color; GUI.depth = depth;
        }
        private static void Fill(Rect rect, Color color) { GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); }
    }
}
