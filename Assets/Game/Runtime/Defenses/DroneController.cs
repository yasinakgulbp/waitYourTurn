using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    public enum DroneState { Inactive, Following, Searching, Diving, Retiring }

    /// <summary>One reused owner defense. Space/gates arrive from composition; no Run/Train/Player dependency.</summary>
    public sealed class DroneController : MonoBehaviour
    {
        [SerializeField] private HealthComponent source;
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private DronePresentation presentation;
        [SerializeField] private LayerMask flightMask = Physics.AllLayers;
        private HealthComponent owner, target;
        private uint ownerLife, targetLife;
        private TargetRegistry registry;
        private Transform space;
        private Bounds interior;
        private Vector3 velocity;
        private float nextSelection, searchUntil, retireAt;
        private bool paused;
        private readonly DroneFlight flight = new DroneFlight();
        private readonly RadialDamage blast = new RadialDamage();
        public DroneState State { get; private set; }
        public bool Occupied => State != DroneState.Inactive;
        public DroneDefinition Definition { get; private set; }
        public HitscanWeapon Weapon => weapon;
        public HealthComponent Target => target;
        public int ShotsFired { get; private set; }
        public int Explosions { get; private set; }
        public int BlastHits { get; private set; }
        public int TimedOut { get; private set; }
        public float SearchRemaining => Mathf.Max(0, searchUntil - Time.time);
        public void RestoreFlight(DroneState state, Vector3 position, Quaternion rotation, float searchRemaining)
        {
            transform.SetPositionAndRotation(position, rotation); velocity = Vector3.zero; target = null;
            State = state == DroneState.Diving ? DroneState.Searching : state;
            searchUntil = Time.time + searchRemaining; nextSelection = Time.time;
            if (State == DroneState.Searching) presentation.SetKamikaze();
        }
        public void Configure(HealthComponent life, HitscanWeapon gun, DronePresentation view)
        { source = life; weapon = gun; presentation = view; }
        public bool TryDeploy(DroneDefinition definition, HealthComponent credit, TargetRegistry targets, Transform frame, Bounds bounds)
        {
            if (Occupied || definition == null || !definition.Valid || credit == null || !credit.IsAlive || targets == null || frame == null) return false;
            Definition = definition; owner = credit; ownerLife = credit.LifeVersion; registry = targets;
            source.ResetForSpawn(1, Team.Player); source.Invulnerable = true;
            weapon.Configure(source, credit); weapon.ConfigureDefinitions(new[] { definition.weapon }); weapon.ResetWeapon();
            State = DroneState.Following; target = null; ShotsFired = BlastHits = 0;
            SetSpace(frame, bounds); RelocateToOwner();
            weapon.Paused = paused; gameObject.SetActive(true); presentation.Show(definition);
            return true;
        }
        public void SetSpace(Transform frame, Bounds bounds) { space = frame; interior = bounds; }
        public void RelocateToOwner()
        {
            if (!Occupied || State == DroneState.Retiring || owner == null) return;
            transform.position = FollowGoal(); velocity = Vector3.zero; target = null; nextSelection = Time.time;
            if (State == DroneState.Diving) State = DroneState.Searching;
        }
        public void SetPaused(bool value) { paused = value; weapon.Paused = value || State != DroneState.Following; }
        public void Clear()
        {
            State = DroneState.Inactive; weapon.Paused = true; target = owner = null; Definition = null;
            velocity = Vector3.zero; presentation.Clear(); gameObject.SetActive(false);
        }
        private Vector3 FollowGoal()
        {
            Vector3 local = space.InverseTransformPoint(owner.transform.position);
            // Interior bounds reach the side-wall centre. Keep the swept sphere inside its inner face.
            float margin = Definition.flightRadius + .12f;
            local.x = Mathf.Clamp(local.x + Definition.followOffset.x, interior.min.x + margin, interior.max.x - margin);
            local.z = Mathf.Clamp(local.z + Definition.followOffset.y, interior.min.z + margin, interior.max.z - margin);
            local.y = Definition.hoverHeight;
            return space.TransformPoint(local);
        }
        private void Update()
        {
            if (!Occupied || paused || Time.timeScale <= 0) return;
            if (owner == null || !owner.IsAlive || owner.LifeVersion != ownerLife) { Clear(); return; }
            if (State == DroneState.Retiring)
            { if (Time.time >= retireAt) Clear(); return; }
            if (State == DroneState.Following)
            {
                Vector3 wanted = Vector3.SmoothDamp(transform.position, FollowGoal(), ref velocity, Definition.followSmoothTime, Definition.followSpeed, Time.deltaTime);
                flight.Sweep(transform.position, wanted, Definition.flightRadius, flightMask, out Vector3 allowed);
                transform.position = allowed;
                if (Time.time >= nextSelection)
                {
                    nextSelection = Time.time + Definition.selectionInterval;
                    target = NearestVisibleTarget.Select(registry, weapon, owner.transform.position);
                    if (target != null) targetLife = target.LifeVersion;
                }
                if (!ValidTarget() || !weapon.HasSight(target)) return;
                presentation.AimAt(target.transform.position + Vector3.up * .85f);
                if (!weapon.TryFire(target.transform.position + Vector3.up * .85f - weapon.Muzzle)) return;
                ShotsFired++;
                if (weapon.State.Empty)
                {
                    State = DroneState.Searching; weapon.Paused = true; target = null;
                    searchUntil = Time.time + Definition.searchSeconds; nextSelection = Time.time;
                    presentation.SetKamikaze();
                }
                return;
            }
            // One finite deadline survives lost targets, pauses and wagon reassignment.
            if (Time.time >= searchUntil) { TimedOut++; Retire(false); return; }
            if (!ValidTarget() || Time.time >= nextSelection)
            {
                nextSelection = Time.time + Definition.selectionInterval;
                target = SelectReachable();
                if (target != null) targetLife = target.LifeVersion;
                State = target == null ? DroneState.Searching : DroneState.Diving;
            }
            if (target == null) return;
            Vector3 aim = target.transform.position + Vector3.up * .85f;
            // Recheck the whole route every frame: a repairing door may close during the dive.
            if (!flight.Sweep(transform.position, aim, Definition.flightRadius, flightMask, out _))
            { target = null; State = DroneState.Searching; return; }
            presentation.AimAt(aim);
            Vector3 next = Vector3.MoveTowards(transform.position, aim, Definition.diveSpeed * Time.deltaTime);
            flight.Sweep(transform.position, next, Definition.flightRadius, flightMask, out Vector3 reached);
            transform.position = reached;
            if ((transform.position - aim).sqrMagnitude <= Definition.impactDistance * Definition.impactDistance) Retire(true);
        }
        private bool ValidTarget() => target != null && target.IsAlive && target.LifeVersion == targetLife;
        private HealthComponent SelectReachable()
        {
            HealthComponent best = null; float closest = weapon.Range * weapon.Range;
            foreach (var candidate in registry.Targets)
            {
                if (!HitscanResolver.Hostile(source, candidate)) continue;
                Vector3 aim = candidate.transform.position + Vector3.up * .85f;
                float distance = (aim - transform.position).sqrMagnitude;
                if (distance >= closest || !flight.Sweep(transform.position, aim, Definition.flightRadius, flightMask, out _)) continue;
                best = candidate; closest = distance;
            }
            return best;
        }
        private void Retire(bool explode)
        {
            State = DroneState.Retiring; target = null; weapon.Paused = true; retireAt = Time.time + .35f;
            if (explode)
            {
                Explosions++;
                BlastHits = blast.Apply(registry, source, owner.Identity, transform.position, Definition.blastRadius, Definition.blastDamage, weapon.HitMask);
            }
            presentation.Exhaust(explode);
        }
    }
}
