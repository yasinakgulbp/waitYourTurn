using System;
using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>One owned-ammunition refill per newly unlocked wagon; cosmetic crates have no colliders.</summary>
    public sealed class SoloLoot : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private GameObject[] crates;
        [SerializeField] private Vector3 localPosition = new Vector3(-3.6f, .05f, 0);
        [SerializeField, Min(.1f)] private float pickupRadius = .9f;
        private bool[] claimed;
        public Vector3 LocalPosition => localPosition;
        public float PickupRadius => pickupRadius;
        public string Status { get; private set; }
        public void Configure(RunDriver owner, GameObject[] visuals) { run = owner; crates = visuals; run.ConfigureLoot(this); }
        private void OnEnable() { run.Restarted += ResetForRun; ResetForRun(); }
        private void OnDisable() { run.Restarted -= ResetForRun; foreach (var crate in crates) if (crate != null) crate.SetActive(false); }
        private void ResetForRun() { claimed = new bool[run.Wagons.Length]; Status = null; RefreshVisuals(); }
        private void RefreshVisuals()
        {
            for (int i = 1; i < crates.Length; i++)
            {
                bool visible = run.UsesSoloProgression && i <= run.Solo.UnlockedThrough && !claimed[i];
                if (crates[i].activeSelf != visible) crates[i].SetActive(visible);
            }
        }
        public bool TryCollect(int wagon)
        {
            if (!run.UsesSoloProgression || run.Loading || run.Flow == null || run.Flow.Paused || !run.Player.IsAlive ||
                wagon < 1 || wagon > run.Solo.UnlockedThrough || wagon >= crates.Length || claimed[wagon] || run.CurrentWagon != run.Wagons[wagon]) return false;
            Vector3 delta = run.Player.transform.position - run.Wagons[wagon].transform.TransformPoint(localPosition); delta.y = 0;
            if (delta.sqrMagnitude > pickupRadius * pickupRadius || !run.Weapon.TryRefillOwned()) return false;
            claimed[wagon] = true; Status = "Cephane tamamlandı"; RefreshVisuals(); return true;
        }
        private void Update()
        {
            RefreshVisuals();
            if (run.UsesSoloProgression) TryCollect(Array.IndexOf(run.Wagons, run.CurrentWagon));
        }
        public bool[] Capture() => (bool[])claimed.Clone();
        public bool Valid(bool[] states, int unlocked)
        {
            if (unlocked < 0 || unlocked >= run.Wagons.Length || states == null || states.Length != run.Wagons.Length || states[0]) return false;
            for (int i = unlocked + 1; i < states.Length; i++) if (states[i]) return false;
            return true;
        }
        public void Restore(bool[] states) { claimed = (bool[])states.Clone(); RefreshVisuals(); }
    }
}
