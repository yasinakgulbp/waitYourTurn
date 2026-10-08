using UnityEngine;

namespace WaitYourTurn.Combat
{
    public sealed class ShotTracer : MonoBehaviour
    {
        [SerializeField] private HitscanPistol pistol;
        [SerializeField] private LineRenderer line;
        private float until;
        public void Configure(HitscanPistol gun, LineRenderer trace) { pistol = gun; line = trace; }
        private void OnEnable() => pistol.Fired += OnShot;
        private void OnDisable()
        {
            if (pistol != null) pistol.Fired -= OnShot;
            // Scene shutdown can destroy the separate visual object before the player component.
            if (line != null) line.enabled = false;
        }
        private void OnShot(ShotNotice shot)
        { line.SetPosition(0, shot.Start); line.SetPosition(1, shot.End); line.enabled = true; until = Time.time + 0.08f; }
        private void Update() { if (line.enabled && Time.time >= until) line.enabled = false; }
    }
}
