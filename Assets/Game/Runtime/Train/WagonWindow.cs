using UnityEngine;

namespace WaitYourTurn.Train
{
    /// <summary>Authoring/validation marker for a clear shot opening. No AI, tick or damage rules.</summary>
    public sealed class WagonWindow : MonoBehaviour
    {
        [SerializeField] private Vector3 clearSize;
        [SerializeField] private float frameWidth;
        public Vector3 ClearSize => clearSize;
        public float FrameWidth => frameWidth;
        public void Configure(Vector3 size, float frame) { clearSize = size; frameWidth = frame; }
    }
}
