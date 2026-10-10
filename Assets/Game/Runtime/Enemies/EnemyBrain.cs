using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Navigation;
using WaitYourTurn.Train;

namespace WaitYourTurn.Enemies
{
    public enum EnemyState { Dormant, ApproachingDoor, AttackingDoor, EnteringWagon, SeekingPlayer, AttackingPlayer, WaitingForRoute, Dead }

    public sealed class EnemyBrain : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private AgentMotor motor;
        [SerializeField] private MeleeAttack attack;
        [SerializeField] private Renderer blockoutBody;
        private MaterialPropertyBlock tint;
        private float defaultHealth;
        private bool defaultsCaptured;
        private Color defaultTint;
        private DoorController door;
        private WagonGeometry geometry;
        private IInteriorPursuit interiorPursuit;
        private uint interiorRevision;
        public void SetInteriorPursuit(IInteriorPursuit value) { interiorPursuit = value; portalRevision = uint.MaxValue; }
        private HealthComponent player;
        private Vector3 doorOffset;
        private float targetAngle;
        private float nextDecision;
        private float decisionPhase;
        private uint portalRevision;
        private HealthComponent attackTarget;
        private bool spawned;
        private bool paused;
        private bool playerPresent = true;
        private bool onBoard;
        public bool OnBoard => onBoard;
        public bool Paused => paused;
        public string WagonId { get; private set; }
        public EnemyProfile Profile { get; private set; }
        public DoorController Entry => door;
        public void RestoreTravel(bool retained) { onBoard = retained; }
        public float AttackRemaining => attack.CooldownRemaining;
        public void RestoreAttack(float remaining) => attack.RestoreCooldown(remaining);
        public int SpawnStation { get; private set; }
        public HealthComponent Health => health;
        public int AgentTypeId => motor.Agent.agentTypeID;
        public int AreaMask => motor.Agent.areaMask;
        public EnemyState State { get; private set; }
        public bool IsInside => onBoard && interiorPursuit != null ? interiorPursuit.Contains(transform.position) :
            geometry != null ? geometry.Contains(transform.position) : door != null && door.Portal.IsInside(transform.position);
        private bool InPassage => geometry != null ? geometry.InPassage(transform.position, motor.Agent.radius) : door.Portal.IsInPassage(transform.position, motor.Agent.radius);
        public bool NeedsSafeDeparture => door != null && (!IsInside || InPassage);
        public void SetScope(string wagonId, bool hasPlayer) { WagonId = wagonId; SetPlayerPresent(hasPlayer); }
        public void SetPlayerPresent(bool present)
        {
            playerPresent = present;
            attackTarget = null; attack.Cancel(); nextDecision = Time.time + decisionPhase;
        }
        public void SetDefender(HealthComponent defender)
        {
            if (player == defender && playerPresent == (defender != null)) return;
            player = defender; SetPlayerPresent(defender != null);
        }
        public void SetPaused(bool value)
        {
            if (paused == value) return;
            paused = value;
            if (value) motor.Stop();
            else { nextDecision = Time.time + decisionPhase; portalRevision = uint.MaxValue; }
        }
        public bool RetainForTravel(Vector3 safeInterior)
        {
            if (!health.IsAlive || (!onBoard && !IsInside)) return false;
            if (NeedsSafeDeparture && !motor.TryPlace(safeInterior)) return false;
            onBoard = true; attackTarget = null; attack.Cancel(); nextDecision = Time.time;
            return true;
        }

        public void Configure(HealthComponent life, AgentMotor movement, MeleeAttack melee)
        { health = life; motor = movement; attack = melee; }
        public void ConfigureBlockout(Renderer body) => blockoutBody = body;
        public void PrepareWarmup() { CaptureDefaults(); attack.ApplyProfile(null); }
        private void CaptureDefaults()
        {
            if (defaultsCaptured) return;
            defaultsCaptured = true;
            defaultHealth = health.Maximum;
            defaultTint = blockoutBody != null ? blockoutBody.sharedMaterial.color : Color.white;
            if (blockoutBody != null) tint = new MaterialPropertyBlock();
        }
        private void OnEnable() => health.Died += OnDeath;
        private void OnDisable() => health.Died -= OnDeath;
        private void OnDeath(DeathNotice death)
        {
            if (door != null) door.ReleaseAttackPosition(this);
            State = EnemyState.Dead;
            attackTarget = null;
            paused = false; onBoard = false; playerPresent = true;
            attack.Cancel();
            motor.Stop();
        }
        public bool Spawn(Vector3 position, DoorController entry, HealthComponent hero, int slot, WagonGeometry space = null,
            EnemyProfile profile = null, int station = 0)
        {
            if (profile != null && !profile.Valid) return false;
            CaptureDefaults();
            Profile = profile; SpawnStation = station;
            door = entry;
            geometry = space;
            player = hero;
            doorOffset = new Vector3((slot % 3 - 1) * 0.65f, 0, -(slot / 3) * 0.65f);
            targetAngle = slot * 2.399963f;
            attackTarget = null;
            paused = false; onBoard = false; playerPresent = true;
            attack.ResetAttack();
            attack.ApplyProfile(profile);
            if (!health.ResetForSpawn(profile != null ? profile.health : defaultHealth, Team.Enemy)) return false;
            if (blockoutBody != null)
            {
                tint.SetColor("_Color", profile != null ? profile.blockoutTint : defaultTint);
                blockoutBody.SetPropertyBlock(tint);
            }
            // Register colliders at the new spawn, rather than at the previous pooled life position.
            transform.position = position;
            gameObject.SetActive(true);
            if (!motor.TryPlace(position)) return false;
            motor.Agent.speed = (profile != null ? profile.speed : 2.6f) + slot % 5 * 0.04f;
            motor.Agent.avoidancePriority = 30 + slot % 30;
            portalRevision = uint.MaxValue;
            decisionPhase = slot % 5 * .03f;
            nextDecision = Time.time + decisionPhase;
            State = EnemyState.ApproachingDoor;
            spawned = true;
            return true;
        }
        public void Despawn()
        {
            if (door != null) door.ReleaseAttackPosition(this);
            spawned = false;
            paused = false; onBoard = false; WagonId = null;
            Profile = null; SpawnStation = 0;
            attackTarget = null;
            attack.ResetAttack();
            motor.Clear();
            door = null;
            geometry = null;
            player = null;
            State = EnemyState.Dormant;
        }
        private void Update()
        {
            if (!spawned || !health.IsAlive || paused) return;
            if (!playerPresent || player == null || !player.IsAlive)
            { attackTarget = null; attack.Cancel(); motor.Stop(); State = EnemyState.WaitingForRoute; return; }
            if (Time.time >= nextDecision)
            {
                nextDecision = Time.time + 0.15f;
                Decide();
            }
            attack.Tick(attackTarget);
            if (attackTarget != null)
            {
                Vector3 facing = State == EnemyState.AttackingDoor ? door.Portal.transform.forward : attackTarget.transform.position - transform.position;
                facing.y = 0;
                if (facing.sqrMagnitude > .001f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facing), 360 * Time.deltaTime);
            }
        }
        private void Decide()
        {
            EntryPortal portal = door.Portal;
            bool routeChanged = portalRevision != portal.Revision;
            if (interiorPursuit != null)
            { routeChanged |= interiorRevision != interiorPursuit.Revision; interiorRevision = interiorPursuit.Revision; }
            portalRevision = portal.Revision;
            attackTarget = null;
            if (!motor.Ready) { State = EnemyState.WaitingForRoute; return; }
            if (IsInside && !InPassage) onBoard = true;
            // Finish crossing before attacking a nearby player, otherwise melee range can park a body in the doorway.
            EntryPortal occupiedPortal = geometry != null ? geometry.PassageAt(transform.position, motor.Agent.radius) :
                portal.IsInPassage(transform.position, motor.Agent.radius) ? portal : null;
            if (occupiedPortal != null && (occupiedPortal.ClosePending || IsInside))
            {
                motor.GoTo(occupiedPortal.InsideDestination, routeChanged);
                State = EnemyState.EnteringWagon;
                return;
            }
            if (!onBoard && !IsInside && !portal.AcceptsEntry)
            {
                bool assigned = door.TryGetAttackPosition(this, out Vector3 approach);
                Vector3 remaining = approach - transform.position; remaining.y = 0;
                if (assigned && remaining.sqrMagnitude <= .24f * .24f && door.Durability.IsAlive && attack.CanReach(door.Durability))
                { motor.Stop(); attackTarget = door.Durability; State = EnemyState.AttackingDoor; }
                else
                { motor.GoTo(assigned ? approach : portal.OutsideApproach + portal.transform.TransformDirection(doorOffset) - portal.transform.forward, routeChanged);
                    State = EnemyState.ApproachingDoor; }
                return;
            }
            door.ReleaseAttackPosition(this);
            if (!onBoard && !IsInside)
            {
                motor.GoTo(portal.InsideDestination, routeChanged);
                State = EnemyState.EnteringWagon;
                return;
            }
            if (!playerPresent)
            { motor.Stop(); State = EnemyState.WaitingForRoute; return; }
            if (attack.CanReach(player))
            { motor.Stop(); attackTarget = player; State = EnemyState.AttackingPlayer; return; }
            // Nearby attack range, rather than an exact occupied grid point, completes pursuit.
            Vector3 offset = new Vector3(Mathf.Sin(targetAngle), 0, Mathf.Cos(targetAngle)) * Mathf.Min(.72f, attack.ActorReach - .12f);
            Vector3 goal = player.transform.position + offset;
            if (onBoard && interiorPursuit != null) goal = interiorPursuit.Constrain(goal, motor.Agent.radius + .1f);
            else if (geometry != null) goal = geometry.Constrain(goal, motor.Agent.radius + .1f);
            else
            {
                float inward = Vector3.Dot(goal - portal.transform.position, portal.transform.forward);
                if (inward < 0.6f) goal += portal.transform.forward * (0.6f - inward);
            }
            motor.GoTo(goal, routeChanged);
            State = motor.HasRoute ? EnemyState.SeekingPlayer : EnemyState.WaitingForRoute;
        }
    }
}
