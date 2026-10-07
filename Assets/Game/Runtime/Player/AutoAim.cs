using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Player
{
    public sealed class AutoAim : MonoBehaviour
    {
        [SerializeField] private TargetRegistry registry;
        [SerializeField] private HitscanPistol pistol;
        private HealthComponent target;
        private uint targetLife;
        private float nextSelection;
        public void Configure(TargetRegistry targets, HitscanPistol gun) { registry = targets; pistol = gun; }
        public void ClearTarget() { target = null; nextSelection = 0; }
        private void Update()
        {
            if (Time.time >= nextSelection)
            {
                nextSelection = Time.time + 0.15f;
                target = null;
                float closest = pistol.Range * pistol.Range;
                foreach (HealthComponent candidate in registry.Targets)
                {
                    if (candidate == null || !candidate.IsAlive) continue;
                    float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                    if (distance >= closest || !pistol.HasSight(candidate)) continue;
                    closest = distance;
                    target = candidate;
                    targetLife = candidate.LifeVersion;
                }
            }
            if (target == null || !target.IsAlive || target.LifeVersion != targetLife || !pistol.HasSight(target)) return;
            Vector3 direction = target.transform.position + Vector3.up * 0.85f - pistol.Muzzle;
            Vector3 facing = direction; facing.y = 0;
            if (facing.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(facing);
            pistol.TryFire(direction);
        }
    }
}
