using System;
using UnityEngine;

namespace WaitYourTurn.Combat
{
    public readonly struct ShotNotice
    {
        public readonly Vector3 Start, End;
        public ShotNotice(Vector3 start, Vector3 end) { Start = start; End = end; }
    }
    public sealed class HitscanPistol : MonoBehaviour
    {
        [SerializeField] private HealthComponent owner;
        [SerializeField, Min(0.01f)] private float damage = 2f;
        [SerializeField, Min(0.01f)] private float fireInterval = 0.35f;
        [SerializeField, Min(0.1f)] private float range = 10f;
        [SerializeField, Min(1)] private int magazineSize = 8;
        [SerializeField, Min(0.1f)] private float reloadSeconds = 1.2f;
        private float nextShot;
        private float reloadUntil;
        private ulong shotId;
        private int rounds;
        public int Rounds => rounds;
        public bool Reloading => reloadUntil > 0f;
        public float Range => range;
        public Vector3 Muzzle => transform.position + Vector3.up * 0.9f;
        public event Action<ShotNotice> Fired;
        public void Configure(HealthComponent hero) => owner = hero;
        private void Start() => ResetWeapon();
        public void ResetWeapon() { rounds = magazineSize; nextShot = reloadUntil = 0f; shotId = 0; }
        private void Update()
        {
            if (!owner.IsAlive) return;
            if (reloadUntil > 0 && Time.time >= reloadUntil) { rounds = magazineSize; reloadUntil = 0; }
        }
        public bool HasSight(HealthComponent target)
        {
            if (target == null || !target.IsAlive || target.Team != Team.Enemy) return false;
            Vector3 delta = target.transform.position + Vector3.up * 0.85f - Muzzle;
            return delta.sqrMagnitude <= range * range && Physics.Raycast(Muzzle, delta.normalized,
                out RaycastHit hit, delta.magnitude + 0.2f, Physics.AllLayers, QueryTriggerInteraction.Ignore) &&
                hit.collider.GetComponentInParent<HealthComponent>() == target;
        }
        public bool TryFire(Vector3 direction)
        {
            if (!owner.IsAlive || Reloading || Time.time < nextShot || rounds <= 0 || direction.sqrMagnitude < 0.001f) return false;
            direction.Normalize();
            nextShot = Time.time + fireInterval; // No burst catch-up after a slow frame.
            rounds--;
            Vector3 end = Muzzle + direction * range;
            if (Physics.Raycast(Muzzle, direction, out RaycastHit hit, range, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                HealthComponent target = hit.collider.GetComponentInParent<HealthComponent>();
                if (target != null && target.Team == Team.Enemy)
                    target.TryApplyDamage(new DamageContext(damage, owner.Identity, owner.Team, ++shotId, target.LifeVersion));
            }
            Fired?.Invoke(new ShotNotice(Muzzle, end));
            if (rounds == 0) reloadUntil = Time.time + reloadSeconds;
            return true;
        }
    }
}
