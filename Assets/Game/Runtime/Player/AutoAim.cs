using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Player
{
    public sealed class AutoAim : MonoBehaviour
    {
        [SerializeField] private TargetRegistry registry;
        [SerializeField] private HitscanWeapon pistol;
        private HealthComponent target;
        private uint targetLife;
        private float nextSelection;
        [SerializeField, Min(.05f)] private float selectionInterval = .15f;
        [SerializeField, Min(0)] private float reactionDelay;
        private float reactAt;
        private Vector3 previousTargetPosition;
        private float sampledAt = -1;
        public void ConfigureReaction(float interval, float delay)
        { selectionInterval = Mathf.Max(.05f, interval); reactionDelay = Mathf.Max(0, delay); ClearTarget(); }
        public void Configure(TargetRegistry targets, HitscanWeapon gun) { registry = targets; pistol = gun; }
        public void ClearTarget() { target = null; nextSelection = 0; reactAt = 0; sampledAt = -1; }
        public bool TryFacing(out Vector3 direction)
        {
            direction = Vector3.zero;
            if (!isActiveAndEnabled || pistol.Paused || target == null || !target.IsAlive ||
                target.LifeVersion != targetLife || !pistol.HasSight(target)) return false;
            direction = target.transform.position - transform.position; direction.y = 0; return true;
        }
        private void Update()
        {
            if (pistol.Paused || Time.timeScale <= 0) { ClearTarget(); return; }
            if (Time.time >= nextSelection)
            {
                nextSelection = Time.time + selectionInterval;
                var selected = NearestVisibleTarget.Select(registry, pistol, transform.position);
                if (selected != target || selected != null && selected.LifeVersion != targetLife)
                { reactAt = Time.time + reactionDelay; sampledAt = -1; }
                target = selected;
                if (target != null) targetLife = target.LifeVersion;
            }
            if (target == null || !target.IsAlive || target.LifeVersion != targetLife || !pistol.HasSight(target))
            { sampledAt = -1; return; }
            Vector3 position = target.transform.position, velocity = Vector3.zero;
            float elapsed = Time.time - sampledAt;
            if (sampledAt >= 0 && elapsed > .001f && elapsed < .25f)
            {
                velocity = (position - previousTargetPosition) / elapsed;
                if (!(velocity.sqrMagnitude <= 400)) velocity = Vector3.zero; // Teleport/pool reuse is not movement.
            }
            previousTargetPosition = position; sampledAt = Time.time;
            if (Time.time < reactAt) return;
            pistol.TryFireAt(target, velocity);
        }
    }
}
