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
        public void Configure(TargetRegistry targets, HitscanWeapon gun) { registry = targets; pistol = gun; }
        public void ClearTarget() { target = null; nextSelection = 0; }
        private void Update()
        {
            if (Time.time >= nextSelection)
            {
                nextSelection = Time.time + 0.15f;
                target = NearestVisibleTarget.Select(registry, pistol, transform.position);
                if (target != null) targetLife = target.LifeVersion;
            }
            if (target == null || !target.IsAlive || target.LifeVersion != targetLife || !pistol.HasSight(target)) return;
            Vector3 direction = target.transform.position + Vector3.up * 0.85f - pistol.Muzzle;
            Vector3 facing = direction; facing.y = 0;
            if (facing.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(facing);
            pistol.TryFire(direction);
        }
    }
}
