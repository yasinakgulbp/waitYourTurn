using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Train
{
    /// <summary>Gameplay owns progress; a slider can observe it without controlling repair.</summary>
    public sealed class ProximityRepair : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        [SerializeField] private HealthComponent actor;
        [SerializeField, Min(0.1f)] private float duration = 3f;
        [SerializeField, Min(0.1f)] private float radius = 1f;
        [SerializeField] private bool interruptOnDoorDamage = true;
        [SerializeField] private bool interruptOnActorDamage;
        private float elapsed;
        private ulong doorDamageRevision, actorDamageRevision;
        private uint doorLife, actorLife;
        public float Progress => Mathf.Clamp01(elapsed / duration);
        public bool Paused { get; set; }
        public bool InterruptOnDoorDamage => interruptOnDoorDamage;
        public bool InterruptOnActorDamage => interruptOnActorDamage;
        public DoorController Target => door;
        public bool InRange => door != null && (transform.position - door.RepairPosition).sqrMagnitude <= radius * radius;
        public void Configure(DoorController target, HealthComponent player) { door = target; actor = player; Cancel(); }
        public void SetDoor(DoorController target) { if (door == target) return; door = target; Cancel(); }
        public void SetDamageInterruption(bool onDoorDamage, bool onActorDamage)
        {
            if (interruptOnDoorDamage == onDoorDamage && interruptOnActorDamage == onActorDamage) return;
            interruptOnDoorDamage = onDoorDamage; interruptOnActorDamage = onActorDamage; Cancel();
        }
        private void OnEnable() => Cancel();
        private void OnDisable() => elapsed = 0f;
        public void Cancel()
        {
            elapsed = 0f;
            HealthComponent targetHealth = door != null ? door.Durability : null;
            doorDamageRevision = targetHealth != null ? targetHealth.DamageRevision : 0;
            actorDamageRevision = actor != null ? actor.DamageRevision : 0;
            doorLife = targetHealth != null ? targetHealth.LifeVersion : 0;
            actorLife = actor != null ? actor.LifeVersion : 0;
        }
        public void RestoreProgress(DoorController target, float progress)
        { SetDoor(target); Cancel(); if (InRange && door.NeedsRepair && actor.IsAlive) elapsed = Mathf.Clamp01(progress) * duration; }
        // Resolve after Update-based attacks; optional interruption takes priority over completion.
        private void LateUpdate() => Tick(Time.deltaTime);
        /// <summary>Uses scaled gameplay time. Also permits deterministic rule checks without waiting.</summary>
        public void Tick(float deltaTime)
        {
            if (Paused) return;
            if (door == null || actor == null || !actor.IsAlive || !door.NeedsRepair || !InRange)
            { Cancel(); return; }
            if (doorLife != door.Durability.LifeVersion || actorLife != actor.LifeVersion ||
                (interruptOnDoorDamage && doorDamageRevision != door.Durability.DamageRevision) ||
                (interruptOnActorDamage && actorDamageRevision != actor.DamageRevision))
            { Cancel(); return; }
            if (!(deltaTime > 0f) || float.IsInfinity(deltaTime)) return;
            elapsed += deltaTime;
            if (elapsed < duration) return;
            door.TryRepair();
            Cancel();
        }
    }
}
