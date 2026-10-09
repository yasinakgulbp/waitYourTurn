using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Train
{
    /// <summary>Internal connection, optionally a manual gate. No durability, station clock or wallet dependency.</summary>
    public sealed class InteriorConnection : MonoBehaviour
    {
        [SerializeField] private BoxCollider blocker;
        [SerializeField] private NavMeshObstacle obstacle;
        [SerializeField] private GameObject panel;
        [SerializeField] private Bounds passage;
        [SerializeField, Min(1)] private int unlockPrice = 200;
        [SerializeField] private bool permanentlyOpen;
        private readonly Collider[] occupants = new Collider[64];
        public int UnlockPrice => unlockPrice;
        public bool IsOpen { get; private set; }
        public bool PermanentlyOpen => permanentlyOpen;
        public Bounds Passage => passage;
        public void Configure(BoxCollider wall, NavMeshObstacle cut, GameObject visual, Bounds region, int price, bool alwaysOpen = false)
        { blocker = wall; obstacle = cut; panel = visual; passage = region; unlockPrice = price; permanentlyOpen = alwaysOpen; }
        public void Restore(bool open)
        {
            open |= permanentlyOpen;
            IsOpen = open;
            if (blocker != null) blocker.enabled = !open;
            if (obstacle != null) obstacle.enabled = !open;
            if (panel != null) panel.SetActive(!open);
        }
        public bool TrySetOpen(bool open)
        {
            if (permanentlyOpen && !open) return false;
            if (open == IsOpen) return true;
            if (!open)
            {
                // Refuse the close while any living body occupies the complete connector.
                // This also keeps the containment region from disappearing beneath a player.
                Physics.SyncTransforms();
                int count = Physics.OverlapBoxNonAlloc(transform.TransformPoint(passage.center),
                    Vector3.Scale(passage.extents, transform.lossyScale), occupants, transform.rotation,
                    Physics.AllLayers, QueryTriggerInteraction.Ignore);
                if (count == occupants.Length) return false;
                for (int i = 0; i < count; i++)
                {
                    var life = occupants[i].GetComponentInParent<HealthComponent>();
                    // A turret's credit Health is not a walking body. Its tiny collider may be
                    // in the room-overlap margin without occupying the closing gate itself.
                    if (life != null && life.IsAlive && (life.GetComponent<CharacterController>() != null ||
                        life.GetComponent<NavMeshAgent>() != null)) return false;
                }
            }
            Restore(open); return true;
        }
    }
}
