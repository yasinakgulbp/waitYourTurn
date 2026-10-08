using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>Bounded cosmetic traces. No collider, projectile mover or damage callback.</summary>
    public sealed class ShotTracer : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon pistol;
        [SerializeField] private LineRenderer line;
        private LineRenderer[] lines;
        private float until;
        public void Configure(HitscanWeapon gun, LineRenderer trace) { pistol = gun; line = trace; }
        private void OnEnable() { if (pistol != null) pistol.Fired += OnShot; }
        private void Start()
        {
            lines = new LineRenderer[16]; lines[0] = line;
            for (int i = 1; i < lines.Length; i++)
            {
                lines[i] = new GameObject("Pellet trace " + i).AddComponent<LineRenderer>();
                lines[i].transform.SetParent(transform, false);
                lines[i].positionCount = 2; lines[i].useWorldSpace = true;
                lines[i].sharedMaterial = line.sharedMaterial;
                lines[i].startWidth = lines[i].endWidth = line.startWidth;
                lines[i].enabled = false;
            }
        }
        private void Hide()
        { if (lines != null) foreach (LineRenderer trace in lines) if (trace != null) trace.enabled = false; }
        private void OnDisable()
        { if (pistol != null) pistol.Fired -= OnShot; Hide(); if (line != null) line.enabled = false; }
        private void OnShot(ShotNotice shot)
        {
            if (lines == null) return;
            if (shot.Pellet == 0) Hide();
            LineRenderer trace = lines[shot.Pellet];
            if (trace == null) return;
            trace.startColor = trace.endColor = shot.Color;
            trace.SetPosition(0, shot.Start); trace.SetPosition(1, shot.End); trace.enabled = true;
            until = Time.time + .08f;
        }
        private void Update() { if (Time.time >= until) Hide(); }
    }
}
