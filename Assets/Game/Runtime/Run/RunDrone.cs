using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;

namespace WaitYourTurn.Run
{
    [DefaultExecutionOrder(-400)]
    public sealed class RunDrone : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private TargetRegistry registry;
        [SerializeField] private DroneController actor;
        [SerializeField] private DroneDefinition definition;
        private RunFlow observedFlow;
        private uint soloRevision;
        public DroneController Actor => actor;
        public DroneDefinition Definition => definition;
        public void Configure(RunDriver owner, TargetRegistry targets, DroneController drone, DroneDefinition data)
        { run = owner; registry = targets; actor = drone; definition = data; }
        public void ConfigureDefinition(DroneDefinition data) => definition = data;
        private void OnEnable() { run.Assigned += OnAssigned; if (run.Flow != null) OnAssigned(run.CurrentWagon); }
        private void OnDisable() { if (run != null) run.Assigned -= OnAssigned; actor.SetPaused(true); }
        private void OnAssigned(WagonRuntime wagon)
        {
            if (observedFlow != run.Flow) { observedFlow = run.Flow; actor.Clear(); return; }
            actor.SetSpace(run.UsesSoloProgression ? run.Solo.Frame : wagon.transform,
                run.UsesSoloProgression ? run.Solo.FlightBounds : wagon.Geometry.Interior);
            if (!run.UsesSoloProgression) actor.RelocateToOwner();
        }
        private void Update()
        {
            if (run.Player == null || !run.Player.IsAlive) { actor.Clear(); return; }
            if (run.UsesSoloProgression && soloRevision != run.Solo.Revision)
            {
                soloRevision = run.Solo.Revision;
                actor.SetSpace(run.Solo.Frame, run.Solo.FlightBounds);
            }
            actor.SetPaused(run.Loading || run.Flow == null || run.Flow.Paused);
        }
        public bool CanBuy => isActiveAndEnabled && run.Flow != null && !run.Flow.Paused && run.Player.IsAlive &&
            run.CurrentWagon != null && definition != null && definition.Valid && !actor.Occupied;
        public bool TryBuy() => CanBuy && actor.TryDeploy(definition, run.Player, registry,
            run.UsesSoloProgression ? run.Solo.Frame : run.CurrentWagon.transform,
            run.UsesSoloProgression ? run.Solo.FlightBounds : run.CurrentWagon.Geometry.Interior);
        public bool Restore(WeaponSnapshot ammo, DroneState state, Vector3 position, Quaternion rotation, float remaining)
        {
            if (!actor.TryDeploy(definition, run.Player, registry,
                run.UsesSoloProgression ? run.Solo.Frame : run.CurrentWagon.transform,
                run.UsesSoloProgression ? run.Solo.FlightBounds : run.CurrentWagon.Geometry.Interior) || !actor.Weapon.Restore(ammo)) return false;
            actor.RestoreFlight(state, position, rotation, remaining); return true;
        }
    }
}
