using UnityEngine;
using WaitYourTurn.Navigation;

namespace WaitYourTurn.Train
{
    /// <summary>Cosmetic-only replacement; the original panels retain their shot collider authority.</summary>
    public sealed class TrainDoorVisual : MonoBehaviour
    {
        [SerializeField] private EntryPortal portal;
        [SerializeField] private GameObject appearance;
        public void Configure(EntryPortal entry, GameObject model) { portal = entry; appearance = model; }
        private void OnEnable() { if (portal != null) { portal.Changed += Apply; Apply(); } }
        private void OnDisable() { if (portal != null) portal.Changed -= Apply; }
        private void Apply() { if (appearance != null) appearance.SetActive(!portal.IsOpen); }
    }
}
