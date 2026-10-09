using UnityEngine;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Temporary safe-area interaction view; all rules live in SoloProgression.</summary>
    public sealed class SoloHud : MonoBehaviour
    {
        [SerializeField] private SoloProgression solo;
        public void Configure(SoloProgression policy) => solo = policy;
        private void OnGUI()
        {
            if (solo == null || !solo.Active) return;
            int index = solo.NearbyConnection(); if (index < 0) return;
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960, safe.height / 540));
            Vector2 offset = new Vector2(safe.x, Screen.height - safe.yMax);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1));
            float width = safe.width / scale;
            // Inside the existing top HUD's blocked input area; never creates joystick gestures.
            var connection = solo.Connections[index];
            string title = index == solo.UnlockedThrough ? $"Sonraki vagon: {connection.UnlockPrice}" :
                connection.IsOpen ? "Ara kapıyı kapat" : "Ara kapıyı aç";
            if (GUI.Button(new Rect(width * .5f - 100, 80, 200, 28), title))
            { if (index == solo.UnlockedThrough) solo.TryUnlock(index); else solo.TryToggle(index); }
            GUI.Label(new Rect(width * .5f - 175, 110, 350, 24), solo.Status ?? "");
            GUI.matrix = previous;
        }
    }
}
