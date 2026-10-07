using UnityEngine;
using WaitYourTurn.Navigation;

namespace WaitYourTurn.Train
{
    /// <summary>Separate shot geometry from the portal's full-height movement blocker.</summary>
    public sealed class DoorPanels : MonoBehaviour
    {
        [SerializeField] private EntryPortal portal;
        [SerializeField] private GameObject solidPanel;
        [SerializeField] private GameObject windowPanel;
        public void Configure(EntryPortal entry, GameObject solid, GameObject window)
        { portal = entry; solidPanel = solid; windowPanel = window; }
        private void OnEnable() { portal.Changed += Apply; Apply(); }
        private void OnDisable() => portal.Changed -= Apply;
        private void Apply()
        {
            solidPanel.SetActive(!portal.IsOpen);
            windowPanel.SetActive(!portal.IsOpen);
        }
    }
}
