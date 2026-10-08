using System;
using UnityEngine;

namespace WaitYourTurn.Combat
{
    public readonly struct ShotNotice
    {
        public readonly Vector3 Start, End;
        public readonly int Pellet, PelletCount;
        public readonly Color Color;
        public ShotNotice(Vector3 start, Vector3 end, int pellet = 0, int pelletCount = 1, Color color = default)
        { Start = start; End = end; Pellet = pellet; PelletCount = pelletCount; Color = color; }
    }

    /// <summary>Shared owner-independent weapon. Several definitions retain separate ammo when equipped.</summary>
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private HealthComponent owner;
        [SerializeField] private HealthComponent rewardOwner;
        [SerializeField] private WeaponDefinition[] definitions;
        [SerializeField] private LayerMask hitMask = Physics.AllLayers;
        [SerializeField] private bool startingWeaponOnly;
        private readonly HitscanResolver resolver = new HitscanResolver();
        private WeaponState[] states;
        private bool[] owned;
        private int equipped;
        private float nextTrigger;
        private ulong attackId, triggerId;
        private bool firing;
        public bool Paused { get; set; }
        public int EquippedIndex => equipped;
        public int WeaponCount => definitions == null || definitions.Length == 0 ? 1 : definitions.Length;
        public WeaponState State { get { EnsureInitialized(); return states[equipped]; } }
        public WeaponDefinition Definition => definitions == null || definitions.Length == 0 ? null : definitions[equipped];
        public string DisplayName => Definition != null ? Definition.displayName : "Pistol";
        public string WeaponName(int index) => definitions == null || definitions.Length == 0 ? "Pistol" : definitions[index].displayName;
        public int Rounds => State.Rounds;
        public int Reserve => State.Reserve;
        public bool Reloading => State.Reloading;
        public bool StartingWeaponOnly => startingWeaponOnly;
        public bool IsOwned(int index) { EnsureInitialized(); return index >= 0 && index < owned.Length && owned[index]; }
        public WeaponState StateAt(int index) { EnsureInitialized(); return index >= 0 && index < states.Length ? states[index] : null; }
        public void ConfigureInventory(bool onlyPistol) { startingWeaponOnly = onlyPistol; ResetWeapon(); }
        public bool CanGrantOrRefill(int index) => index > 0 && index < WeaponCount && !firing &&
            (!IsOwned(index) || StateAt(index).NeedsRefill);
        public bool TryGrantOrRefill(int index)
        {
            if (!CanGrantOrRefill(index) || Paused || Time.timeScale <= 0 || owner == null || !owner.IsAlive) return false;
            if (owned[index]) states[index].TryRefill();
            else owned[index] = true;
            equipped = index; return true;
        }
        public float Range => State.Spec.Range;
        public Vector3 Muzzle => transform.position + Vector3.up * .9f;
        public event Action<ShotNotice> Fired;
        public void Configure(HealthComponent source, HealthComponent creditedOwner = null)
        { owner = source; rewardOwner = creditedOwner; }
        public void ConfigureDefinitions(WeaponDefinition[] available) { definitions = available; states = null; equipped = 0; }
        public void SetHitMask(LayerMask mask) => hitMask = mask;
        private void Start() => EnsureInitialized();
        private void EnsureInitialized() { if (states == null) ResetWeapon(); }
        public void ResetWeapon()
        {
            if (firing) return;
            states = new WeaponState[WeaponCount];
            owned = new bool[WeaponCount];
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = new WeaponState(definitions == null || definitions.Length == 0 ? WeaponSpec.Pistol :
                    definitions[i] != null ? definitions[i].CreateSpec() : throw new InvalidOperationException("Missing weapon definition."));
                owned[i] = !startingWeaponOnly || i == 0;
            }
            equipped = 0; nextTrigger = 0; // Only an explicit new run replenishes ammo. Attack IDs stay monotonic.
        }
        public bool Equip(int index)
        {
            if (firing || index < 0 || index >= WeaponCount || Paused || Time.timeScale <= 0 || owner == null || !owner.IsAlive) return false;
            EnsureInitialized(); if (!owned[index]) return false;
            equipped = index; State.Tick(Time.time); return true;
        }
        private void Update()
        {
            if (Paused || owner == null || !owner.IsAlive || Time.timeScale <= 0) return;
            EnsureInitialized();
            foreach (WeaponState state in states) state.Tick(Time.time);
        }
        public bool HasSight(HealthComponent target)
        {
            if (!HitscanResolver.Hostile(owner, target)) return false;
            Vector3 delta = target.transform.position + Vector3.up * .85f - Muzzle;
            return delta.sqrMagnitude <= Range * Range && resolver.Cast(Muzzle, delta.normalized,
                delta.magnitude + .2f, hitMask, owner, out RaycastHit hit) && hit.collider != null &&
                hit.collider.GetComponentInParent<HealthComponent>() == target;
        }
        public bool TryFire(Vector3 direction)
        {
            if (firing || Paused || Time.timeScale <= 0 || owner == null || !owner.IsAlive ||
                !Finite(direction) || direction.sqrMagnitude < .001f || Time.time < nextTrigger) return false;
            WeaponState state = State;
            if (!state.TryFire(Time.time)) return false;
            nextTrigger = Time.time + state.Spec.Interval;
            Vector3 start = Muzzle; WeaponSpec spec = state.Spec;
            Color color = Definition != null ? Definition.tracerColor : Color.yellow;
            EntityIdentity source = owner.Identity, credit = rewardOwner != null ? rewardOwner.Identity : source;
            Team team = owner.Team;
            triggerId++; firing = true;
            try
            {
                for (int i = 0; i < spec.Pellets; i++)
                {
                    Vector3 ray = HitscanResolver.PelletDirection(direction, i, spec.Pellets, spec.SpreadDegrees, triggerId);
                    Vector3 end = start + ray * spec.Range;
                    if (resolver.Cast(start, ray, spec.Range, hitMask, owner, out RaycastHit hit))
                    {
                        end = hit.point;
                        HealthComponent target = hit.collider != null ? hit.collider.GetComponentInParent<HealthComponent>() : null;
                        if (HitscanResolver.Hostile(owner, target))
                            target.TryApplyDamage(new DamageContext(spec.Damage, source, team, ++attackId,
                                target.LifeVersion, rewardOwner: credit));
                    }
                    Fired?.Invoke(new ShotNotice(start, end, i, spec.Pellets, color));
                }
            }
            finally { firing = false; }
            return true;
        }
        private static bool Finite(Vector3 v) => !float.IsNaN(v.sqrMagnitude) && !float.IsInfinity(v.sqrMagnitude);
    }
}
