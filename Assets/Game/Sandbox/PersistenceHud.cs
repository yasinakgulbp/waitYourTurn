using UnityEngine;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Development controls only; the player build saves/resumes automatically.</summary>
    public sealed class PersistenceHud : MonoBehaviour
    {
        [SerializeField] private RunPersistence save;
        public void Configure(RunPersistence persistence) => save = persistence;
        private void Update()
        {
            if (!Application.isEditor || save.Busy) return;
            if (Input.GetKeyDown(KeyCode.F5)) save.SaveNow();
            if (Input.GetKeyDown(KeyCode.F6)) StartCoroutine(save.LoadNow());
        }
        private void OnGUI()
        {
            if (!Application.isEditor) return;
            GUI.Label(new Rect(12, Screen.height - 20, Mathf.Min(760, Screen.width - 24), 20), "F5 Save / F6 Resume / F4 Check | " + save.Status);
        }
    }
}
