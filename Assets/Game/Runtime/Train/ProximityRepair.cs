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
        private float elapsed;
        public float Progress => Mathf.Clamp01(elapsed / duration);
        public bool InRange => (transform.position - door.RepairPosition).sqrMagnitude <= radius * radius;
        public void Configure(DoorController target, HealthComponent player) { door = target; actor = player; }
        private void OnDisable() => elapsed = 0f;
        public void Cancel() => elapsed = 0f;
        private void Update()
        {
            if (!actor.IsAlive || door.State != DoorState.Broken || !InRange) { elapsed = 0f; return; }
            elapsed += Time.deltaTime;
            if (elapsed < duration) return;
            door.TryRepair();
            elapsed = 0f;
        }
    }
}
