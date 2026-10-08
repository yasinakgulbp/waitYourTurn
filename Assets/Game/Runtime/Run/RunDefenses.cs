using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;

namespace WaitYourTurn.Run
{
    /// <summary>Run gate and current-wagon purchase adapter. Turret code never references Run.</summary>
    [DefaultExecutionOrder(-400)]
    public sealed class RunDefenses : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private TargetRegistry registry;
        [SerializeField] private TurretRack[] racks;
        [SerializeField] private TurretDefinition[] definitions;
        private RunFlow observedFlow;
        public TurretRack[] Racks => racks;
        public TurretDefinition[] Definitions => definitions;
        public void Configure(RunDriver owner, TargetRegistry targets, TurretRack[] wagons, TurretDefinition[] types)
        { run = owner; registry = targets; racks = wagons; definitions = types; }
        private void OnEnable() { run.Assigned += OnAssigned; if (run.Flow != null) OnAssigned(run.CurrentWagon); }
        private void OnDisable() { if (run != null) run.Assigned -= OnAssigned; SetPaused(true); }
        private void OnAssigned(WagonRuntime wagon)
        {
            if (observedFlow == run.Flow) return;
            observedFlow = run.Flow; foreach (var rack in racks) rack.Clear();
        }
        private void Update() => SetPaused(run.Flow == null || run.Flow.Paused || !run.Player.IsAlive);
        private void SetPaused(bool paused) { if (racks != null) foreach (var rack in racks) rack.SetPaused(paused); }
        public TurretRack CurrentRack
        {
            get
            { for (int i = 0; i < run.Wagons.Length; i++) if (run.Wagons[i] == run.CurrentWagon) return racks[i]; return null; }
        }
        public bool CanBuy(int index) => isActiveAndEnabled && run.Flow != null && !run.Flow.Paused && run.Player.IsAlive &&
            index >= 0 && index < definitions.Length && CurrentRack != null && CurrentRack.CanDeploy(definitions[index], run.Player.transform.position);
        public bool TryBuy(int index) => CanBuy(index) && CurrentRack.TryDeploy(definitions[index], run.Player, registry);
        public string PurchaseHint
        {
            get
            {
                var rack = CurrentRack; var mount = rack != null ? rack.Nearest(run.Player.transform.position) : null;
                if (mount == null) return "Move beside a turret pad";
                if (mount.Occupied) return mount.Id + " occupied (" + mount.Actor.Weapon.Rounds + " rounds)";
                return mount.Id + (mount.CanPlace() ? " available; max " + rack.PerTypeLimit + " per type/wagon" : " blocked; step beside pad");
            }
        }
    }
}
