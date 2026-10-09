using System;
using System.Collections.Generic;
using UnityEngine;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Train;

namespace WaitYourTurn.Run
{
    /// <summary>Solo composition policy. Purchase, movement context and pursuit share the same unlocked topology.</summary>
    [DefaultExecutionOrder(-490)]
    public sealed class SoloProgression : MonoBehaviour, IInteriorPursuit
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private InteriorConnection[] connections;
        [SerializeField] private MovementArea movement;
        [SerializeField, Min(.1f)] private float interactionDistance = 2.8f;
        public int UnlockedThrough { get; private set; }
        public MovementArea Movement => movement;
        public InteriorConnection[] Connections => connections;
        public bool Active => run.UsesSoloProgression;
        public string Status { get; private set; }
        public Transform Frame => movement.transform;
        public Bounds FlightBounds { get; private set; }
        public uint Revision { get; private set; }
        public void Configure(RunDriver owner, RunEconomy shop, InteriorConnection[] gates, MovementArea region)
        { run = owner; economy = shop; connections = gates; movement = region; run.ConfigureSolo(this); }
        public void ConfigureInteractionDistance(float distance) => interactionDistance = Mathf.Max(.1f, distance);
        public void ResetForRun()
        {
            UnlockedThrough = 0; Status = null;
            foreach (var connection in connections) connection.Restore(false);
            RefreshRegions();
            foreach (var wagon in run.Wagons) wagon.Enemies.SetInteriorPursuit(Active ? this : null);
        }
        public bool Nearby(int index)
        {
            if (index < 0 || index >= connections.Length) return false;
            Vector3 delta = run.Player.transform.position - connections[index].transform.position; delta.y = 0;
            return delta.sqrMagnitude <= interactionDistance * interactionDistance;
        }
        public int NearbyConnection()
        {
            for (int i = 0; i < connections.Length; i++) if (i <= UnlockedThrough && Nearby(i)) return i;
            return -1;
        }
        private bool CanInteract(int index) => Active && economy.CanShop && !run.Loading && Nearby(index);
        public bool TryUnlock(int index)
        {
            if (!CanInteract(index) || index != UnlockedThrough) return false;
            if (!economy.Wallet.TrySpend(connections[index].UnlockPrice)) { Status = "Yeterli para yok"; return false; }
            // Single scene-thread transaction; no asynchronous effect after charging.
            UnlockedThrough++; RefreshRegions(); Status = "Vagon açıldı; ara kapıyı açabilirsin"; return true;
        }
        public bool TryToggle(int index)
        {
            if (!CanInteract(index) || index >= UnlockedThrough) return false;
            var gate = connections[index];
            var drone = economy.Drone.Actor;
            if (gate.IsOpen && drone.Occupied)
            {
                var occupied = gate.Passage; occupied.Expand(economy.Drone.Definition.flightRadius * 2);
                if (occupied.Contains(gate.transform.InverseTransformPoint(drone.transform.position)))
                { Status = "Dron geçitte; kapatılamıyor"; return false; }
            }
            if (!gate.TrySetOpen(!gate.IsOpen)) { Status = "Geçitte biri var; kapatılamıyor"; return false; }
            RefreshRegions(); Status = gate.IsOpen ? "Ara kapı açık" : "Ara kapı kapalı"; return true;
        }
        private Bounds Room(int index)
        {
            var wagon = run.Wagons[index]; var local = wagon.Geometry.Interior;
            // Same inset as the individual battle MovementArea. Y does not constrain locomotion.
            return new Bounds(Frame.InverseTransformPoint(wagon.transform.TransformPoint(local.center)),
                new Vector3(local.size.x, local.size.y, local.size.z - .18f));
        }
        private Bounds Bridge(int index)
        {
            var gate = connections[index]; var bounds = gate.Passage;
            return new Bounds(Frame.InverseTransformPoint(gate.transform.TransformPoint(bounds.center)), bounds.size);
        }
        private void RefreshRegions()
        {
            Revision++;
            var regions = new List<Bounds>(run.Wagons.Length + connections.Length);
            int current = Mathf.Clamp(Array.IndexOf(run.Wagons, run.CurrentWagon), 0, UnlockedThrough);
            int first = current, last = current;
            while (first > 0 && connections[first - 1].IsOpen) first--;
            while (last < UnlockedThrough && connections[last].IsOpen) last++;
            // Only the connected component containing the player is a movement region.
            // A crowd cannot push a capsule into a disconnected unlocked room through a closed gate.
            for (int i = first; i <= last; i++) regions.Add(Room(i));
            for (int i = first; i < last; i++) regions.Add(Bridge(i));
            movement.ConfigureRegions(regions.ToArray());
            var flight = Room(0); for (int i = 1; i <= UnlockedThrough; i++) flight.Encapsulate(Room(i));
            FlightBounds = flight;
        }
        public void RefreshMovementContext() => RefreshRegions();
        public bool ContainsSaved(Vector3 point, int unlocked, bool[] open)
        {
            if (unlocked < 0 || unlocked >= run.Wagons.Length || open == null || open.Length != connections.Length) return false;
            Vector3 local = Frame.InverseTransformPoint(point);
            if (local.y < -.25f || local.y > 3) return false;
            for (int i = 0; i <= unlocked; i++) if (InXZ(Room(i), local)) return true;
            for (int i = 0; i < unlocked; i++) if (open[i] && InXZ(Bridge(i), local)) return true;
            return false;
        }
        private static bool InXZ(Bounds b, Vector3 p) => p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z;
        public bool Contains(Vector3 point)
        {
            Vector3 local = Frame.InverseTransformPoint(point);
            for (int i = 0; i <= UnlockedThrough; i++) if (InXZ(Room(i), local)) return true;
            for (int i = 0; i < UnlockedThrough; i++) if (connections[i].IsOpen && InXZ(Bridge(i), local)) return true;
            return false;
        }
        public Vector3 Constrain(Vector3 point, float radius) => movement.Constrain(point, radius);
        public bool[] CaptureGates()
        { var states = new bool[connections.Length]; for (int i = 0; i < states.Length; i++) states[i] = connections[i].IsOpen; return states; }
        public void Restore(int unlocked, bool[] open)
        {
            if (unlocked < 0 || unlocked >= run.Wagons.Length || open == null || open.Length != connections.Length)
                throw new ArgumentException("Invalid Solo topology");
            UnlockedThrough = unlocked;
            for (int i = 0; i < open.Length; i++) connections[i].Restore(i < unlocked && open[i]);
            RefreshRegions();
            foreach (var wagon in run.Wagons) wagon.Enemies.SetInteriorPursuit(this);
        }
        public void BindPursuit()
        {
            // Pool owner remains stable for capacity, death rewards and serialization.
            foreach (var wagon in run.Wagons)
            { wagon.Enemies.SetDefender(run.Player); wagon.Enemies.SetScope(wagon.Id, true); }
        }
        private void Update()
        {
            if (!Active || run.Loading || run.Flow == null || run.Flow.Paused || !run.Player.IsAlive) return;
            int nearest = Array.IndexOf(run.Wagons, run.CurrentWagon); float distance = float.PositiveInfinity;
            for (int i = 0; i <= UnlockedThrough; i++)
            {
                float delta = Mathf.Abs(run.Player.transform.position.x - run.Wagons[i].transform.position.x);
                if (delta < distance) { distance = delta; nearest = i; }
            }
            if (run.Wagons[nearest] != run.CurrentWagon) run.EnterWagonOnFoot(run.Wagons[nearest]);
        }
    }
}
