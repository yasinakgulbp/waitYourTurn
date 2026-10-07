using System;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Navigation;

namespace WaitYourTurn.Train
{
    public enum DoorState { Intact, Broken, WaitingForClearance }

    [DisallowMultipleComponent]
    public sealed class DoorController : MonoBehaviour
    {
        [SerializeField] private HealthComponent durability;
        [SerializeField] private EntryPortal portal;
        [SerializeField] private Transform repairPoint;
        private readonly UnityEngine.Object[] attackers = new UnityEngine.Object[3];
        public HealthComponent Durability => durability;
        public EntryPortal Portal => portal;
        public Vector3 RepairPosition => repairPoint.position;
        public DoorState State => !durability.IsAlive ? DoorState.Broken :
            portal.ClosePending ? DoorState.WaitingForClearance : DoorState.Intact;
        public event Action Changed;

        public void Configure(HealthComponent health, EntryPortal entry, Transform repairAnchor)
        { durability = health; portal = entry; repairPoint = repairAnchor; }

        private void OnEnable()
        {
            durability.HealthChanged += OnHealthChanged;
            portal.Changed += OnPortalChanged;
            SyncPassage();
        }
        private void OnDisable()
        {
            durability.HealthChanged -= OnHealthChanged;
            portal.Changed -= OnPortalChanged;
        }
        private void OnHealthChanged(HealthSnapshot health)
        {
            if (!durability.IsAlive) Array.Clear(attackers, 0, attackers.Length);
            SyncPassage(); Changed?.Invoke();
        }
        private void OnPortalChanged() => Changed?.Invoke();
        private void SyncPassage() => portal.SetOpen(!durability.IsAlive);

        public bool TryGetAttackPosition(UnityEngine.Object actor, out Vector3 point)
        {
            for (int i = 0; i < attackers.Length; i++)
                if (attackers[i] == actor)
                { point = portal.OutsideApproach + portal.transform.right * ((i - 1) * 0.65f); return true; }
            if (durability.IsAlive)
                for (int i = 0; i < attackers.Length; i++)
                    if (attackers[i] == null)
                    {
                        attackers[i] = actor;
                        point = portal.OutsideApproach + portal.transform.right * ((i - 1) * 0.65f);
                        return true;
                    }
            point = portal.OutsideApproach;
            return false;
        }
        public void ReleaseAttackPosition(UnityEngine.Object actor)
        { for (int i = 0; i < attackers.Length; i++) if (attackers[i] == actor) attackers[i] = null; }

        public bool TryRepair()
        {
            if (durability.IsAlive) return false;
            // Repair is an explicit new durability life, never ordinary healing of a dead target.
            return durability.ResetForSpawn(durability.Maximum, Team.Neutral);
        }
    }
}
