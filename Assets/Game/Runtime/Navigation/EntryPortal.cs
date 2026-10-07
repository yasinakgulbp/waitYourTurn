using System;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace WaitYourTurn.Navigation
{
    /// <summary>Owns passage availability, independently of door health or visuals.</summary>
    [DisallowMultipleComponent]
    public sealed class EntryPortal : MonoBehaviour
    {
        [SerializeField] private NavMeshLink link;
        [SerializeField] private BoxCollider blocker;
        [SerializeField] private Transform outsideApproach;
        [SerializeField] private Transform insideDestination;
        private readonly Collider[] occupants = new Collider[16];

        public event Action Changed;
        public bool IsOpen => link != null && link.activated;
        public bool ClosePending { get; private set; }
        public bool AcceptsEntry => IsOpen && !ClosePending;
        public Vector3 OutsideApproach => outsideApproach.position;
        public Vector3 InsideDestination => insideDestination.position;
        public uint Revision { get; private set; }

        public void Configure(NavMeshLink traversal, BoxCollider obstruction,
            Transform approach, Transform destination)
        {
            link = traversal;
            blocker = obstruction;
            outsideApproach = approach;
            insideDestination = destination;
        }

        private void Awake()
        {
            if (link == null || blocker == null || outsideApproach == null || insideDestination == null)
            {
                Debug.LogError("EntryPortal requires a link, blocker and two navigation anchors.", this);
                enabled = false;
                return;
            }
            blocker.enabled = !IsOpen;
        }

        public bool IsInside(Vector3 position)
        {
            return Vector3.Dot(position - transform.position, transform.forward) > 0f;
        }

        public void SetOpen(bool open)
        {
            if (!enabled || link == null || blocker == null) return;
            if (open)
            {
                if (IsOpen && !ClosePending) return;
                ClosePending = false;
                blocker.enabled = false;
                link.activated = true;
                NotifyChanged();
                return;
            }

            if (!IsOpen || ClosePending) return;
            // Stop new destinations first. Allow an in-flight crossing to finish.
            ClosePending = true;
            NotifyChanged();
            TryFinishClosing();
        }

        private void Update()
        {
            if (ClosePending) TryFinishClosing();
        }

        private void TryFinishClosing()
        {
            if (link.occupied || HasCharacterInDoorway()) return;
            link.activated = false;
            blocker.enabled = true;
            ClosePending = false;
            NotifyChanged();
        }

        private bool HasCharacterInDoorway()
        {
            Vector3 halfSize = Vector3.Scale(blocker.size, blocker.transform.lossyScale) * 0.5f;
            int count = Physics.OverlapBoxNonAlloc(blocker.transform.TransformPoint(blocker.center),
                halfSize + Vector3.one * 0.05f, occupants, blocker.transform.rotation,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
            if (count == occupants.Length) return true; // Conservative on buffer saturation.
            for (int i = 0; i < count; i++)
            {
                if (occupants[i] != blocker &&
                    (occupants[i].GetComponentInParent<NavMeshAgent>() != null ||
                     occupants[i].GetComponentInParent<CharacterController>() != null)) return true;
            }
            return false;
        }

        private void NotifyChanged()
        {
            Revision++;
            Changed?.Invoke();
        }
    }
}
