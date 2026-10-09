using System;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;

namespace WaitYourTurn.Run
{
    [Serializable] public sealed class MineSave
    {
        public int slot, wagon;
        public Vector3 position;
        public float armingRemaining;
    }
    /// <summary>Run lifecycle/placement only; reusable mine actors do not know modes, currency or wagons.</summary>
    [DefaultExecutionOrder(-400)]
    public sealed class RunMines : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private TargetRegistry registry;
        [SerializeField] private MineDefinition definition;
        [SerializeField] private MineController[] actors;
        [SerializeField, Min(.1f)] private float minimumSpacing = .5f;
        private int[] wagons;
        private readonly Collider[] overlaps = new Collider[16];
        public MineController[] Actors => actors;
        public MineDefinition Definition => definition;
        public string DefinitionKey => definition.DefinitionKey + FormattableString.Invariant($"/{actors.Length}/{minimumSpacing}");
        public void Configure(RunDriver owner, TargetRegistry targets, MineDefinition data, MineController[] pool)
        { run = owner; registry = targets; definition = data; actors = pool; }
        private void Awake() { wagons = new int[actors.Length]; }
        private void OnEnable() => run.Restarted += Clear;
        private void OnDisable() { run.Restarted -= Clear; SetPaused(true); }
        private void Update() => SetPaused(run.Loading || run.Flow == null || run.Flow.Paused || !run.Player.IsAlive);
        private void SetPaused(bool value) { foreach (var actor in actors) actor.SetPaused(value); }
        public void Clear() { foreach (var actor in actors) actor.Clear(); }
        private int FreeSlot() { for (int i = 0; i < actors.Length; i++) if (!actors[i].Occupied) return i; return -1; }
        private bool TryPoint(out Vector3 point)
        {
            point = run.Player.transform.position; point.y = run.CurrentWagon.transform.position.y + .025f;
            if (!run.CurrentWagon.Geometry.Contains(point)) return false;
            foreach (var actor in actors) if (actor.Deployed && (actor.transform.position - point).sqrMagnitude < minimumSpacing * minimumSpacing) return false;
            int count = Physics.OverlapSphereNonAlloc(point + Vector3.up * .14f, .14f, overlaps, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            if (count == overlaps.Length) return false;
            for (int i = 0; i < count; i++)
                if (overlaps[i].GetComponentInParent<HealthComponent>() != run.Player) return false;
            return true;
        }
        public bool CanBuy => isActiveAndEnabled && definition != null && definition.Valid && run.Flow != null &&
            !run.Flow.Paused && !run.Loading && run.Player.IsAlive && run.CurrentWagon != null && FreeSlot() >= 0 && TryPoint(out _);
        public bool TryBuy()
        {
            if (!CanBuy || !TryPoint(out var point)) return false;
            int slot = FreeSlot();
            if (!actors[slot].TryDeploy(definition, run.Player, registry, run.Weapon.HitMask, point)) return false;
            wagons[slot] = Array.IndexOf(run.Wagons, run.CurrentWagon); return true;
        }
        public MineSave[] Capture()
        {
            int count = 0; foreach (var actor in actors) if (actor.Deployed) count++;
            var result = new MineSave[count]; int n = 0;
            for (int i = 0; i < actors.Length; i++) if (actors[i].Deployed) result[n++] = new MineSave
            { slot = i, wagon = wagons[i], position = actors[i].transform.position, armingRemaining = actors[i].ArmingRemaining };
            return result;
        }
        public bool CanRestore(MineSave[] data)
        {
            if (data == null) return true;
            if (data.Length > actors.Length) return false;
            var used = new bool[actors.Length];
            foreach (var saved in data)
            {
                if (saved == null || saved.slot < 0 || saved.slot >= actors.Length || used[saved.slot] || saved.wagon < 0 ||
                    saved.wagon >= run.Wagons.Length || !run.Wagons[saved.wagon].Geometry.Contains(saved.position) ||
                    !(saved.position.sqrMagnitude < 100000000) ||
                    !(saved.armingRemaining >= 0 && saved.armingRemaining <= definition.armingSeconds) ||
                    Mathf.Abs(saved.position.y - run.Wagons[saved.wagon].transform.position.y - .025f) > .01f) return false;
                used[saved.slot] = true;
            }
            return true;
        }
        public bool Restore(MineSave[] data)
        {
            if (!CanRestore(data)) return false;
            Clear(); if (data == null) return true;
            foreach (var saved in data)
            {
                if (!actors[saved.slot].TryDeploy(definition, run.Player, registry, run.Weapon.HitMask, saved.position)) return false;
                wagons[saved.slot] = saved.wagon; actors[saved.slot].RestoreArming(saved.armingRemaining);
                actors[saved.slot].SetPaused(true);
            }
            return true;
        }
    }
}
