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
        public void ConfigureReaction(float interval, float delay)
        { selectionInterval = Mathf.Max(.05f, interval); reactionDelay = Mathf.Max(0, delay); ClearTarget(); }
        public void Configure(TargetRegistry targets, HitscanWeapon gun) { registry = targets; pistol = gun; }
        public void ClearTarget() { target = null; nextSelection = 0; reactAt = 0; }
        private void Update()
        {
            if (Time.time >= nextSelection)
            {
                nextSelection = Time.time + selectionInterval;
                var selected = NearestVisibleTarget.Select(registry, pistol, transform.position);
                if (selected != target || selected != null && selected.LifeVersion != targetLife) reactAt = Time.time + reactionDelay;
                target = selected;
                if (target != null) targetLife = target.LifeVersion;
            }
            if (target == null || !target.IsAlive || target.LifeVersion != targetLife || !pistol.HasSight(target)) return;
            Vector3 direction = target.transform.position + Vector3.up * 0.85f - pistol.Muzzle;
            Vector3 facing = direction; facing.y = 0;
            if (facing.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(facing);
            if (Time.time < reactAt) return;
            pistol.TryFire(direction);
        }
    }
}
