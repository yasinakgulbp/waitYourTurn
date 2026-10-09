using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>Reusable cosmetic ring/flash; no gameplay callbacks, real lights or physics.</summary>
    public sealed class ExplosionPulse : MonoBehaviour
    {
        [SerializeField] private Transform flash;
        [SerializeField] private LineRenderer ring;
        private float elapsed, radius;
        private bool visible;
        public bool Visible => visible;
        public void Configure(Transform centerFlash, LineRenderer outline) { flash = centerFlash; ring = outline; }
        public void Show(Vector3 center, float size)
        {
            transform.position = center; radius = size; elapsed = 0; visible = true;
            flash.gameObject.SetActive(true); ring.enabled = true;
            Tick(0);
        }
        public void Tick(float seconds)
        {
            if (!visible) return;
            elapsed += seconds;
            if (elapsed >= .35f) { Clear(); return; }
            float phase = elapsed / .35f;
            flash.localScale = Vector3.one * Mathf.Lerp(.35f, .05f, phase);
            flash.gameObject.SetActive(phase < .3f);
            ring.transform.localScale = Vector3.one * radius * Mathf.Lerp(.2f, 1, phase);
            var color = new Color(1, Mathf.Lerp(.85f, .2f, phase), .05f, 1 - phase);
            ring.startColor = ring.endColor = color;
        }
        public void Clear() { visible = false; flash.gameObject.SetActive(false); ring.enabled = false; }
    }
}
