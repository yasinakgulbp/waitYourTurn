using System;
using UnityEngine;
using UnityEngine.AI;

namespace WaitYourTurn.Navigation
{
    /// <summary>Owns passage availability, independently of door health or visuals.</summary>
    [DisallowMultipleComponent]
    public sealed class EntryPortal : MonoBehaviour
    {
        [SerializeField] private NavMeshObstacle doorwayCut;
        [SerializeField] private BoxCollider blocker;
        [SerializeField] private Transform outsideApproach;
        [SerializeField] private Transform insideDestination;
        private readonly Collider[] occupants = new Collider[16];

        public event Action Changed;
        public bool IsOpen { get; private set; }
        public bool ClosePending { get; private set; }
        public bool AcceptsEntry => IsOpen && !ClosePending;
        public Vector3 OutsideApproach => outsideApproach.position;
        public Vector3 InsideDestination => insideDestination.position;
        public uint Revision { get; private set; }

        public void Configure(NavMeshObstacle navigationCut, BoxCollider obstruction,
            Transform approach, Transform destination)
        {
            doorwayCut = navigationCut;
            blocker = obstruction;
            outsideApproach = approach;
            insideDestination = destination;
        }

        private void Awake()
        {
            if (doorwayCut == null || blocker == null || outsideApproach == null || insideDestination == null)
            {
                Debug.LogError("EntryPortal requires a carving obstacle, blocker and two navigation anchors.", this);
                enabled = false;
                return;
            }
            blocker.enabled = !IsOpen;
            doorwayCut.enabled = !IsOpen;
        }

        public bool IsInside(Vector3 position)
        {
            return Vector3.Dot(position - transform.position, transform.forward) > 0f;
        }

        public bool IsInPassage(Vector3 position, float bodyRadius)
        {
            Vector3 local = blocker.transform.InverseTransformPoint(position) - blocker.center;
            Vector3 scale = blocker.transform.lossyScale;
            float margin = bodyRadius + 0.05f;
            Vector3 half = blocker.size * 0.5f + new Vector3(
                margin / Mathf.Abs(scale.x), margin / Mathf.Abs(scale.y), margin / Mathf.Abs(scale.z));
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        public void SetOpen(bool open)
        {
            if (!enabled || doorwayCut == null || blocker == null) return;
            if (open)
            {
                if (IsOpen && !ClosePending) return;
                ClosePending = false;
                blocker.enabled = false;
                doorwayCut.enabled = false;
                IsOpen = true;
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
            if (HasCharacterInDoorway()) return;
            doorwayCut.enabled = true;
            IsOpen = false;
            blocker.enabled = true;
            ClosePending = false;
            NotifyChanged();
        }

        private bool HasCharacterInDoorway()
        {
            Physics.SyncTransforms(); // Closure only: navigation transforms may have moved since the last physics step.
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
