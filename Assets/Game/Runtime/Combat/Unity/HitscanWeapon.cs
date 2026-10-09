using System;
using UnityEngine;

namespace WaitYourTurn.Combat
{
    [Serializable]
    public sealed class WeaponSnapshot
    {
        public int equipped;
        public bool[] owned;
        public AmmoSnapshot[] ammo;
        public float triggerRemaining;
        public GrenadeFlightSnapshot[] projectiles;
    }
    public readonly struct ShotNotice
    {
        public readonly Vector3 Start, End;
        public readonly int Pellet, PelletCount;
        public readonly Color Color;
        public readonly bool Projectile;
        public ShotNotice(Vector3 start, Vector3 end, int pellet = 0, int pelletCount = 1, Color color = default, bool projectile = false)
        { Start = start; End = end; Pellet = pellet; PelletCount = pelletCount; Color = color; Projectile = projectile; }
    }

    /// <summary>Shared owner-independent weapon. Several definitions retain separate ammo when equipped.</summary>
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private HealthComponent owner;
        [SerializeField] private HealthComponent rewardOwner;
        [SerializeField] private WeaponDefinition[] definitions;
        [SerializeField] private LayerMask hitMask = Physics.AllLayers;
        [SerializeField] private bool startingWeaponOnly;
        [SerializeField] private Transform muzzleSocket;
        [SerializeField] private ExplosiveProjectiles projectiles;
        public ExplosiveProjectiles Projectiles => projectiles;
        public void ConfigureProjectiles(ExplosiveProjectiles delivery) => projectiles = delivery;
        private readonly HitscanResolver resolver = new HitscanResolver();
        private WeaponState[] states;
        private bool[] owned;
        private int equipped;
        private float nextTrigger;
        private ulong attackId, triggerId;
        private bool firing;
        private float rangeMultiplier = 1;
        public float RangeMultiplier => rangeMultiplier;
        public bool SetRangeMultiplier(float value)
        {
            if (firing || !(value >= 1 && value <= 1.1f)) return false;
            rangeMultiplier = value; return true;
        }
        public bool Paused { get; set; }
        public LayerMask HitMask => hitMask;
        public int EquippedIndex => equipped;
        public int WeaponCount => definitions == null || definitions.Length == 0 ? 1 : definitions.Length;
        public WeaponState State { get { EnsureInitialized(); return states[equipped]; } }
        public WeaponDefinition Definition => definitions == null || definitions.Length == 0 ? null : definitions[equipped];
        public WeaponDefinition DefinitionAt(int index) => definitions != null && index >= 0 && index < definitions.Length ? definitions[index] : null;
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
        public bool CanBuyMagazine(int index) => !firing && index > 0 && index < WeaponCount && IsOwned(index) && StateAt(index).CanBuyMagazine;
        public bool TryBuyMagazine(int index) => CanBuyMagazine(index) && !Paused && Time.timeScale > 0 &&
            owner != null && owner.IsAlive && StateAt(index).TryBuyMagazine(Time.time);
        public bool TryGrantOrRefill(int index)
        {
            if (!CanGrantOrRefill(index) || Paused || Time.timeScale <= 0 || owner == null || !owner.IsAlive) return false;
            if (owned[index]) states[index].TryRefill();
            else owned[index] = true;
            equipped = index; return true;
        }
        public float Range => State.Spec.Range * rangeMultiplier;
        public bool TryRefillOwned()
        {
            if (firing || Paused || Time.timeScale <= 0 || owner == null || !owner.IsAlive) return false;
            EnsureInitialized(); bool changed = false;
            for (int i = 0; i < states.Length; i++) if (owned[i]) changed |= states[i].TryRefill();
            return changed; // No unlock, equip change or bypass of nextShot.
        }
        public Vector3 Muzzle => muzzleSocket != null ? muzzleSocket.position : transform.position + Vector3.up * .9f;
        public void ConfigureMuzzle(Transform socket) => muzzleSocket = socket;
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
            if (projectiles != null) projectiles.Clear();
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
            if (Definition != null && Definition.delivery == ShotDelivery.Grenade &&
                (projectiles == null || !Definition.ValidDelivery || resolver.Sweep(Muzzle, delta.normalized, delta.magnitude,
                    Definition.projectileRadius, hitMask, owner, out var sweep) &&
                    (sweep.collider == null || sweep.collider.GetComponentInParent<HealthComponent>() != target))) return false;
            return delta.sqrMagnitude <= Range * Range && resolver.Cast(Muzzle, delta.normalized,
                delta.magnitude + .2f, hitMask, owner, out RaycastHit hit) && hit.collider != null &&
                hit.collider.GetComponentInParent<HealthComponent>() == target;
        }
        public Vector3 AimDirectionFor(HealthComponent target, Vector3 velocity)
        {
            Vector3 direct = target.transform.position + Vector3.up * .85f - Muzzle;
            if (Definition == null || Definition.delivery != ShotDelivery.Grenade || !Definition.ValidDelivery ||
                !ProjectileAim.TryLead(direct, velocity, Definition.projectileSpeed, Range, out var lead)) return direct;
            // A predicted point must not redirect the shot through door metal or a window post.
            if (resolver.Sweep(Muzzle, lead.normalized, lead.magnitude, Definition.projectileRadius, hitMask, owner, out var hit))
            {
                var body = hit.collider != null ? hit.collider.GetComponentInParent<HealthComponent>() : null;
                if (body == null || body.Team == Team.Neutral) return direct;
            }
            return lead;
        }
        public bool TryFireAt(HealthComponent target, Vector3 velocity)
        {
            if (Paused || Time.timeScale <= 0 || Time.time < nextTrigger || !HitscanResolver.Hostile(owner, target)) return false;
            return TryFire(AimDirectionFor(target, velocity));
        }
        public bool TryFire(Vector3 direction)
        {
            if (firing || Paused || Time.timeScale <= 0 || owner == null || !owner.IsAlive ||
                !Finite(direction) || direction.sqrMagnitude < .001f || Time.time < nextTrigger) return false;
            WeaponState state = State;
            bool grenade = Definition != null && Definition.delivery == ShotDelivery.Grenade;
            if (grenade && (!Definition.ValidDelivery || projectiles == null || !projectiles.CanLaunch)) return false;
            if (!state.TryFire(Time.time)) return false;
            nextTrigger = Time.time + state.Spec.Interval;
            Vector3 start = Muzzle; WeaponSpec spec = state.Spec; float shotRange = Range;
            Color color = Definition != null ? Definition.tracerColor : Color.yellow;
            EntityIdentity source = owner.Identity, credit = rewardOwner != null ? rewardOwner.Identity : source;
            Team team = owner.Team;
            triggerId++; firing = true;
            try
            {
                if (grenade)
                {
                    projectiles.Launch(equipped, start, direction, shotRange, rewardOwner ?? owner);
                    Fired?.Invoke(new ShotNotice(start, start, color: color, projectile: true));
                    return true;
                }
                for (int i = 0; i < spec.Pellets; i++)
                {
                    Vector3 ray = HitscanResolver.PelletDirection(direction, i, spec.Pellets, spec.SpreadDegrees, triggerId);
                    Vector3 end = start + ray * shotRange;
                    if (resolver.Cast(start, ray, shotRange, hitMask, owner, out RaycastHit hit))
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
        public WeaponSnapshot Capture()
        {
            EnsureInitialized(); var data = new WeaponSnapshot { equipped = equipped, owned = (bool[])owned.Clone(),
                ammo = new AmmoSnapshot[states.Length], triggerRemaining = Mathf.Max(0, nextTrigger - Time.time),
                projectiles = projectiles != null ? projectiles.Capture() : null };
            for (int i = 0; i < states.Length; i++) data.ammo[i] = states[i].Capture(Time.time);
            return data;
        }
        public bool CanRestore(WeaponSnapshot data)
        {
            EnsureInitialized();
            if (data == null || data.owned == null || data.ammo == null || data.owned.Length != states.Length ||
                data.ammo.Length != states.Length || data.equipped < 0 || data.equipped >= states.Length || !data.owned[0] ||
                !data.owned[data.equipped] || !(data.triggerRemaining >= 0) || data.triggerRemaining > 60) return false;
            for (int i = 0; i < states.Length; i++) if (!states[i].CanRestore(data.ammo[i])) return false;
            if (projectiles != null ? !projectiles.CanRestore(data.projectiles, data.owned) :
                data.projectiles != null && data.projectiles.Length != 0) return false;
            return true;
        }
        public bool Restore(WeaponSnapshot data)
        {
            if (!CanRestore(data)) return false;
            for (int i = 0; i < states.Length; i++) states[i].Restore(data.ammo[i], Time.time);
            owned = (bool[])data.owned.Clone(); equipped = data.equipped; nextTrigger = Time.time + data.triggerRemaining;
            if (projectiles != null) projectiles.Restore(data.projectiles);
            return true;
        }
    }
}
