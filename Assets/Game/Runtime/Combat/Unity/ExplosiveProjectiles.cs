using System;
using UnityEngine;

namespace WaitYourTurn.Combat
{
    [Serializable] public sealed class GrenadeFlightSnapshot
    {
        public int definition;
        public Vector3 position, direction;
        public float remainingRange;
    }
    [Serializable] public sealed class GrenadeView
    {
        public Transform body;
        public ExplosionPulse explosion;
    }
    /// <summary>Fixed-capacity swept projectiles. Shot delivery has no wagon/shop/mode dependency.</summary>
    public sealed class ExplosiveProjectiles : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private HealthComponent owner;
        [SerializeField] private TargetRegistry registry;
        [SerializeField] private GrenadeView[] views;
        private sealed class Flight
        {
            public bool active;
            public int definition;
            public Vector3 position, direction;
            public float remaining;
            public uint ownerLife;
            public HealthComponent credit;
            public uint creditLife;
        }
        private Flight[] flights;
        private readonly HitscanResolver resolver = new HitscanResolver();
        private readonly RadialDamage blast = new RadialDamage();
        public int Detonations { get; private set; }
        public int LastBlastHits { get; private set; }
        public int Capacity => views?.Length ?? 0;
        public int ActiveCount { get { Ensure(); int n = 0; foreach (var f in flights) if (f.active) n++; return n; } }
        public bool CanLaunch { get { Ensure(); for (int i = 0; i < flights.Length; i++) if (!flights[i].active && !views[i].explosion.Visible) return true; return false; } }
        public void Configure(HitscanWeapon gun, HealthComponent body, TargetRegistry targets, GrenadeView[] visuals)
        { weapon = gun; owner = body; registry = targets; views = visuals; weapon.ConfigureProjectiles(this); }
        private void Ensure()
        {
            if (flights != null) return;
            flights = new Flight[Capacity];
            for (int i = 0; i < flights.Length; i++) { flights[i] = new Flight(); views[i].body.gameObject.SetActive(false); views[i].explosion.Clear(); }
        }
        public bool Launch(int definition, Vector3 position, Vector3 direction, float distance, HealthComponent credit)
        {
            Ensure();
            for (int i = 0; i < flights.Length; i++) if (!flights[i].active && !views[i].explosion.Visible)
            {
                var f = flights[i]; f.active = true; f.definition = definition; f.position = position;
                f.direction = direction.normalized; f.remaining = distance; f.ownerLife = owner.LifeVersion;
                f.credit = credit ?? owner; f.creditLife = f.credit.LifeVersion;
                views[i].body.position = position; views[i].body.gameObject.SetActive(true); return true;
            }
            return false;
        }
        private void Update()
        {
            if (weapon.Paused || Time.timeScale <= 0) return;
            Tick(Time.deltaTime);
        }
        public void Tick(float seconds)
        {
            Ensure();
            for (int i = 0; i < flights.Length; i++)
            {
                views[i].explosion.Tick(seconds);
                var f = flights[i]; if (!f.active) continue;
                if (!owner.IsAlive || owner.LifeVersion != f.ownerLife || f.credit == null ||
                    !f.credit.IsAlive || f.credit.LifeVersion != f.creditLife) { Hide(i); continue; }
                var definition = weapon.DefinitionAt(f.definition);
                float step = Mathf.Min(f.remaining, definition.projectileSpeed * seconds);
                bool hit = resolver.Sweep(f.position, f.direction, step, definition.projectileRadius, weapon.HitMask, owner, out var impact);
                f.position += f.direction * (hit ? Mathf.Max(0, impact.distance - .01f) : step);
                f.remaining -= step; views[i].body.position = f.position;
                if (!hit && f.remaining > .001f) continue;
                Hide(i); Detonations++;
                LastBlastHits = blast.Apply(registry, owner, f.credit.Identity, f.position,
                    definition.blastRadius, definition.damage, weapon.HitMask);
                views[i].explosion.Show(f.position, definition.blastRadius);
            }
        }
        private void Hide(int slot) { flights[slot].active = false; views[slot].body.gameObject.SetActive(false); }
        public void Clear()
        { Ensure(); for (int i = 0; i < flights.Length; i++) { Hide(i); views[i].explosion.Clear(); } }
        public GrenadeFlightSnapshot[] Capture()
        {
            Ensure(); var result = new GrenadeFlightSnapshot[ActiveCount]; int n = 0;
            foreach (var f in flights) if (f.active) result[n++] = new GrenadeFlightSnapshot
            { definition = f.definition, position = f.position, direction = f.direction, remainingRange = f.remaining };
            return result;
        }
        public bool CanRestore(GrenadeFlightSnapshot[] data, bool[] owned)
        {
            if (data == null) return true;
            if (data.Length > Capacity) return false;
            foreach (var f in data)
            {
                if (f == null || f.definition < 0 || f.definition >= weapon.WeaponCount || !owned[f.definition]) return false;
                var definition = weapon.DefinitionAt(f.definition);
                if (definition == null || definition.delivery != ShotDelivery.Grenade || !definition.ValidDelivery ||
                    !(f.remainingRange > 0 && f.remainingRange <= definition.range * 1.1f) ||
                    !(f.position.sqrMagnitude < 100000000) || !Mathf.Approximately(f.direction.sqrMagnitude, 1)) return false;
            }
            return true;
        }
        public void Restore(GrenadeFlightSnapshot[] data)
        {
            Clear(); if (data == null) return;
            foreach (var f in data) Launch(f.definition, f.position, f.direction, f.remainingRange, owner);
        }
    }
}
