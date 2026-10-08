using System;
using System.Collections.Generic;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Player;
using WaitYourTurn.Train;

namespace WaitYourTurn.Run
{
    /// <summary>Composition and gameplay gate. Scene state survives phase changes; Restart alone resets it.</summary>
    [DefaultExecutionOrder(-500)]
    public sealed class RunDriver : MonoBehaviour
    {
        [SerializeField] private WagonRuntime[] wagons;
        [SerializeField] private HealthComponent player;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private MoveInput input;
        [SerializeField] private AutoAim aim;
        [SerializeField] private HitscanWeapon pistol;
        [SerializeField] private ProximityRepair repair;
        [SerializeField] private RunTimings timings = new RunTimings();
        [SerializeField] private int seed = 12345;
        [SerializeField] private int initialWagonIndex;
        public void ConfigureInitialWagon(int index) => initialWagonIndex = index;
        private System.Random random;
        private bool gateClosed;
        private float previousTimeScale;
        private readonly List<(HealthComponent health, bool invulnerable)> protectedBodies = new List<(HealthComponent, bool)>(40);
        public RunFlow Flow { get; private set; }
        public WagonRuntime CurrentWagon { get; private set; }
        public WagonRuntime[] Wagons => wagons;
        public HealthComponent Player => player;
        public HitscanWeapon Weapon => pistol;
        public ProximityRepair Repair => repair;
        public int Assignments { get; private set; }
        public bool ControlsAllowed { get; set; } = true;
        public bool AimAllowed { get; set; } = true;
        public string Failure { get; private set; }
        public event Action<WagonRuntime> Assigned;
        public void Configure(WagonRuntime[] coaches, HealthComponent hero, PlayerMotor movement, MoveInput controls,
            AutoAim targeting, HitscanWeapon gun, ProximityRepair interaction)
        { wagons = coaches; player = hero; motor = movement; input = controls; aim = targeting; pistol = gun; repair = interaction; }
        private void OnEnable() => player.Died += OnDeath;
        private void OnDisable()
        { if (player != null) player.Died -= OnDeath; if (Flow != null) Flow.Changed -= OnPhase; SetGate(false); }
        private void Start() => Restart();
        public void Restart(RunTimings overrideTimings = null)
        {
            SetGate(false);
            if (Flow != null) Flow.Changed -= OnPhase;
            random = new System.Random(seed); Assignments = 0; Failure = null;
            foreach (WagonRuntime wagon in wagons)
            {
                wagon.Enemies.ClearAlive(); wagon.SetStationAccess(false);
                foreach (DoorController door in wagon.Doors) door.Durability.ResetForSpawn(door.Durability.Maximum, Team.Neutral);
            }
            player.ResetForSpawn(player.Maximum, Team.Player); pistol.ResetWeapon(); repair.Cancel(); aim.ClearTarget();
            Flow = new RunFlow(overrideTimings ?? timings); Flow.Changed += OnPhase;
            if (!Assign(wagons[Mathf.Clamp(initialWagonIndex, 0, wagons.Length - 1)], false)) { Flow.Fail(); return; }
            OnPhase(RunPhase.Approach);
        }
        private void Update()
        {
            if (Flow == null) return;
            Flow.Tick(Time.unscaledDeltaTime);
            bool open = !Flow.Paused && player.IsAlive;
            input.InputEnabled = open && ControlsAllowed;
            aim.enabled = open && AimAllowed;
            repair.Paused = !open;
            if (open) repair.SetDoor(CurrentWagon.NearestDoor(player.transform.position));
        }
        private void OnDeath(DeathNotice death) => Flow?.EndRun();
        private void OnPhase(RunPhase phase)
        {
            if (phase == RunPhase.Defense) foreach (WagonRuntime wagon in wagons) wagon.SetStationAccess(true);
            if (phase == RunPhase.Departing)
                foreach (WagonRuntime wagon in wagons)
                    if (!wagon.ResolveDeparture()) { Failure = "No clear interior position for a doorway survivor."; Flow.Fail(); return; }
            SetGate(Flow.Paused);
            if (phase == RunPhase.FadeIn && !Assign(wagons[random.Next(wagons.Length)], true)) Flow.Fail();
        }
        private bool Assign(WagonRuntime wagon, bool count)
        {
            if (!player.IsAlive || !wagon.TrySafePoint(player, out Vector3 point))
            { Failure = "No clear player spawn inside the assigned wagon."; return false; }
            repair.Cancel(); repair.SetDoor(wagon.NearestDoor(point));
            motor.SetMovementArea(wagon.Area); motor.Place(point); aim.ClearTarget();
            CurrentWagon = wagon;
            foreach (WagonRuntime coach in wagons) coach.Enemies.SetScope(coach.Id, coach == wagon);
            if (count) Assignments++;
            Assigned?.Invoke(wagon); return true;
        }
        private void SetGate(bool closed)
        {
            if (pistol != null) pistol.Paused = closed;
            if (gateClosed == closed) return;
            gateClosed = closed;
            if (closed)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
                Protect(player);
                foreach (WagonRuntime wagon in wagons)
                {
                    foreach (DoorController door in wagon.Doors) Protect(door.Durability);
                    foreach (var enemy in wagon.Enemies.Active) Protect(enemy.Health);
                }
            }
            else
            {
                Time.timeScale = previousTimeScale;
                foreach (var body in protectedBodies) if (body.health != null) body.health.Invulnerable = body.invulnerable;
                protectedBodies.Clear();
            }
            foreach (WagonRuntime wagon in wagons)
                if (wagon != null && wagon.Enemies != null) wagon.Enemies.SetPaused(closed);
            if (input != null) input.InputEnabled = !closed && ControlsAllowed && player != null && player.IsAlive;
            if (aim != null) aim.enabled = !closed && AimAllowed && player != null && player.IsAlive;
            if (repair != null) repair.Paused = closed;
        }
        private void Protect(HealthComponent health)
        { protectedBodies.Add((health, health.Invulnerable)); health.Invulnerable = true; }
    }
}
