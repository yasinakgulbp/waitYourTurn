using UnityEngine;

namespace WaitYourTurn.Player
{
    /// <summary>Touch adapter and desktop fallback. Gameplay reads Move, never touch IDs or keys.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed class MoveInput : MonoBehaviour
    {
        private int finger = -1;
        private Vector2 origin, stick;
        public Vector2 Move { get; private set; }
        public bool InputEnabled { get; set; } = true;
        public Rect BlockedScreenArea { get; set; }
        private float Radius => Mathf.Clamp(Screen.width * 0.09f, 45f, 110f);
        private void Update()
        {
            if (!InputEnabled) { finger = -1; Move = stick = Vector2.zero; return; }
            if (Input.touchCount == 0)
            {
                finger = -1;
                stick = Vector2.zero;
                Move = Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1);
                return;
            }
            bool found = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 guiPoint = new Vector2(touch.position.x, Screen.height - touch.position.y);
                if (finger < 0 && touch.phase == TouchPhase.Began && touch.position.x < Screen.width * 0.5f &&
                    !BlockedScreenArea.Contains(guiPoint))
                { finger = touch.fingerId; origin = touch.position; }
                if (touch.fingerId != finger) continue;
                found = true;
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                { finger = -1; stick = Vector2.zero; }
                else stick = Vector2.ClampMagnitude((touch.position - origin) / Radius, 1);
            }
            if (!found) { finger = -1; stick = Vector2.zero; }
            Move = stick;
        }
        private void OnDisable() { finger = -1; Move = stick = Vector2.zero; }
        private void OnGUI()
        {
            if (finger < 0) return;
            float r = Radius;
            Vector2 center = new Vector2(origin.x, Screen.height - origin.y);
            GUI.Box(new Rect(center.x - r, center.y - r, r * 2, r * 2), "MOVE");
            Vector2 handle = center + new Vector2(stick.x, -stick.y) * r;
            GUI.Box(new Rect(handle.x - 16, handle.y - 16, 32, 32), "");
        }
    }
}
