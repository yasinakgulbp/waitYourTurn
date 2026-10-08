using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    /// <summary>One line and one reusable exhaustion visual. No damage or product ownership.</summary>
    public sealed class TurretPresentation : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private GameObject bodyVisual;
        [SerializeField] private Renderer accent;
        [SerializeField] private LineRenderer trace;
        [SerializeField] private Transform burst;
        private MaterialPropertyBlock tint;
        private float traceUntil, burstUntil;
        public void Configure(HitscanWeapon gun, GameObject visual, Renderer color, LineRenderer line, Transform effect)
        { weapon = gun; bodyVisual = visual; accent = color; trace = line; burst = effect; }
        private void OnEnable() => weapon.Fired += OnShot;
        private void OnDisable() { weapon.Fired -= OnShot; Clear(); }
        public void Show(Color color)
        {
            Clear(); bodyVisual.SetActive(true);
            if (tint == null) tint = new MaterialPropertyBlock();
            tint.SetColor("_BaseColor", color); tint.SetColor("_Color", color); accent.SetPropertyBlock(tint);
        }
        public void Clear() { trace.enabled = false; burst.gameObject.SetActive(false); burstUntil = 0; }
        public void Exhaust()
        {
            bodyVisual.SetActive(false); burst.localScale = Vector3.one * .2f;
            burst.gameObject.SetActive(true); burstUntil = Time.time + .35f;
        }
        private void OnShot(ShotNotice shot)
        {
            trace.startColor = trace.endColor = shot.Color;
            trace.SetPosition(0, shot.Start); trace.SetPosition(1, shot.End);
            trace.enabled = true; traceUntil = Time.time + .08f;
        }
        private void Update()
        {
            if (Time.time >= traceUntil) trace.enabled = false;
            if (burstUntil <= 0) return;
            if (Time.time >= burstUntil) { gameObject.SetActive(false); return; }
            burst.localScale = Vector3.one * Mathf.Lerp(.2f, .8f, 1 - (burstUntil - Time.time) / .35f);
        }
    }
}
