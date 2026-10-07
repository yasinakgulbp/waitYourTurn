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
        private DoorController door;
        private HealthComponent player;
        private Vector3 doorOffset;
        private float targetAngle;
        private float nextDecision;
        private uint portalRevision;
        private HealthComponent attackTarget;
        private bool spawned;
        public HealthComponent Health => health;
        public int AgentTypeId => motor.Agent.agentTypeID;
        public int AreaMask => motor.Agent.areaMask;
        public EnemyState State { get; private set; }
        public bool IsInside => door != null && door.Portal.IsInside(transform.position);

        public void Configure(HealthComponent life, AgentMotor movement, MeleeAttack melee)
        { health = life; motor = movement; attack = melee; }
        private void OnEnable() => health.Died += OnDeath;
        private void OnDisable() => health.Died -= OnDeath;
        private void OnDeath(DeathNotice death)
        {
            if (door != null) door.ReleaseAttackPosition(this);
            State = EnemyState.Dead;
            attackTarget = null;
            attack.Cancel();
            motor.Stop();
        }
        public bool Spawn(Vector3 position, DoorController entry, HealthComponent hero, int slot)
        {
            door = entry;
            player = hero;
            doorOffset = new Vector3((slot % 3 - 1) * 0.65f, 0, -(slot / 3) * 0.65f);
            targetAngle = slot * 2.399963f;
            attackTarget = null;
            attack.ResetAttack();
            if (!health.ResetForSpawn(health.Maximum, Team.Enemy)) return false;
            // Register colliders at the new spawn, rather than at the previous pooled life position.
            transform.position = position;
            gameObject.SetActive(true);
            if (!motor.TryPlace(position)) return false;
            motor.Agent.speed = 2.6f + slot % 5 * 0.12f;
            motor.Agent.avoidancePriority = 30 + slot % 30;
            portalRevision = uint.MaxValue;
            nextDecision = Time.time + slot % 5 * 0.03f;
            State = EnemyState.ApproachingDoor;
            spawned = true;
            return true;
        }
        public void Despawn()
        {
            if (door != null) door.ReleaseAttackPosition(this);
            spawned = false;
            attackTarget = null;
            attack.ResetAttack();
            motor.Clear();
            door = null;
            player = null;
            State = EnemyState.Dormant;
        }
        private void Update()
        {
            if (!spawned || !health.IsAlive) return;
            if (!player.IsAlive) { attack.Cancel(); motor.Stop(); return; }
            if (Time.time >= nextDecision)
            {
                nextDecision = Time.time + 0.15f;
                Decide();
            }
            attack.Tick(attackTarget);
        }
        private void Decide()
        {
            EntryPortal portal = door.Portal;
            bool routeChanged = portalRevision != portal.Revision;
            portalRevision = portal.Revision;
            attackTarget = null;
            if (!motor.Ready) { State = EnemyState.WaitingForRoute; return; }
            if (portal.ClosePending && portal.IsInPassage(transform.position, motor.Agent.radius))
            {
                motor.GoTo(portal.InsideDestination, routeChanged);
                State = EnemyState.EnteringWagon;
                return;
            }
            if (!IsInside && !portal.AcceptsEntry)
            {
                bool assigned = door.TryGetAttackPosition(this, out Vector3 approach);
                if (assigned && door.Durability.IsAlive && attack.CanReach(door.Durability))
                { motor.Stop(); attackTarget = door.Durability; State = EnemyState.AttackingDoor; }
                else
                { motor.GoTo(assigned ? approach : portal.OutsideApproach + doorOffset - portal.transform.forward, routeChanged);
                    State = EnemyState.ApproachingDoor; }
                return;
            }
            door.ReleaseAttackPosition(this);
            if (!IsInside)
            {
                motor.GoTo(portal.InsideDestination, routeChanged);
                State = EnemyState.EnteringWagon;
                return;
            }
            if (attack.CanReach(player))
            { motor.Stop(); attackTarget = player; State = EnemyState.AttackingPlayer; return; }
            // Nearby attack range, rather than an exact occupied grid point, completes pursuit.
            Vector3 offset = new Vector3(Mathf.Sin(targetAngle), 0, Mathf.Cos(targetAngle)) * 0.8f;
            motor.GoTo(player.transform.position + offset, routeChanged);
            State = motor.HasRoute ? EnemyState.SeekingPlayer : EnemyState.WaitingForRoute;
        }
    }
}
