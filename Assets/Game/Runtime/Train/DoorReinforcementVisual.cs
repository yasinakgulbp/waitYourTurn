using UnityEngine;

namespace WaitYourTurn.Train
{
    /// <summary>Replaceable cosmetics only: no collider, nav change or shot-blocking layer.</summary>
    public sealed class DoorReinforcementVisual : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        [SerializeField] private GameObject wood, wire;
        private int level;
        public void Configure(DoorController owner, GameObject first, GameObject second) { door = owner; wood = first; wire = second; Sync(); }
        public void SetLevel(int stage) { level = stage; Sync(); }
        private void OnEnable() { door.Changed += Sync; Sync(); }
        private void OnDisable() => door.Changed -= Sync;
        private void Sync()
        {
            bool intact = door.Durability.IsAlive;
            wood.SetActive(intact && level == 1); wire.SetActive(intact && level == 2);
        }
    }
}
