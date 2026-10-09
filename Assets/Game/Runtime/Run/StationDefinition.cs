using System;
using System.Collections.Generic;
using UnityEngine;
using WaitYourTurn.Enemies;

namespace WaitYourTurn.Run
{
    [Serializable]
    public sealed class SpawnBand
    {
        public EnemyProfile profile;
        [Tooltip("-1 targets every wagon; otherwise the zero-based wagon index.")] public int wagon = -1;
        [Min(1)] public int count = 8;
        [Min(0)] public int perStationIncrease;
        [Range(1, 1000)] public int maximumCount = 1000;
        [Min(0)] public float firstAt = 2;
        [Min(.01f)] public float interval = 3;
        [Min(1)] public int threatCost = 1;
        [Range(1, 100)] public int threatWeight = 1;
        [Min(1)] public int unlockStation = 1;
    }
    [CreateAssetMenu(menuName = "Wait Your Turn/Station Definition")]
    public sealed class StationDefinition : ScriptableObject
    {
        [Min(1)] public int firstStation = 1;
        [Min(1)] public int globalLiveLimit = 40;
        [Min(1)] public int wagonLiveLimit = 12;
        [Min(1)] public int queueCapacity = 32;
        [Min(1)] public int requestsPerFrame = 2;
        [Min(1)] public int warmupPerFrame = 4;
        [Min(1)] public int positionAttempts = 3;
        [Min(.01f)] public float retryDelay = .25f;
        public SpawnBand[] bands;
        public bool useThreatBudget;
        [Min(1)] public int initialThreatBudget = 13, threatBudgetPerStation = 3, maximumThreatBudget = 160;
        public int ThreatBudgetAt(int station) => (int)Math.Min(maximumThreatBudget,
            initialThreatBudget + (long)Math.Max(0, station - firstStation) * threatBudgetPerStation);
        public bool Validate(int wagons, out string reason)
        {
            reason = null;
            if (firstStation < 1 || wagons < 1 || wagons > 32 || globalLiveLimit < 1 || wagonLiveLimit < 1 ||
                queueCapacity < 1 || queueCapacity > 256 || requestsPerFrame < 1 || requestsPerFrame > 32 ||
                warmupPerFrame < 1 || warmupPerFrame > 32 || positionAttempts < 1 || positionAttempts > 16 ||
                !(retryDelay > 0) || float.IsInfinity(retryDelay)) reason = "Invalid limits (queue 1–256, frame budgets 1–32, position attempts 1–16).";
            else if (useThreatBudget && (initialThreatBudget < 1 || initialThreatBudget > maximumThreatBudget ||
                maximumThreatBudget > 1000 || threatBudgetPerStation < 0 || threatBudgetPerStation > 1000)) reason = "Invalid threat budget (1–1000).";
            else if (bands == null || bands.Length == 0 || bands.Length > 32) reason = "Expected 1–32 spawn bands.";
            else for (int i = 0; i < bands.Length; i++)
            {
                var b = bands[i];
                if (b == null || b.profile == null || !b.profile.Valid || b.wagon < -1 || b.wagon >= wagons ||
                    b.count < 1 || b.count > 1000 || b.perStationIncrease < 0 || b.perStationIncrease > 1000 ||
                    b.maximumCount < b.count || b.maximumCount > 1000 || b.firstAt < 0 || float.IsNaN(b.firstAt) || float.IsInfinity(b.firstAt) ||
                    !(b.interval > 0) || float.IsInfinity(b.interval) || useThreatBudget &&
                    (b.wagon != -1 || b.threatCost < 1 || b.threatCost > 100 || b.threatWeight < 1 || b.threatWeight > 100 || b.unlockStation < 1))
                { reason = $"Band {i}: invalid profile, wagon index, count (1–1000), start or interval."; break; }
            }
            if (reason == null && useThreatBudget)
            {
                bool affordable = false;
                foreach (var band in bands) if (band.unlockStation <= firstStation && band.threatCost <= initialThreatBudget) affordable = true;
                if (!affordable) reason = "Threat budget needs an affordable starting type.";
            }
            return reason == null;
        }
        private int CountAt(SpawnBand band, int station) => (int)Math.Min(band.maximumCount,
            band.count + (long)Math.Max(0, station - firstStation) * band.perStationIncrease);
        public SpawnStream[] BuildStreams(int wagonCount, int station = 0)
        {
            if (!Validate(wagonCount, out string reason)) throw new ArgumentException(reason);
            if (useThreatBudget) return BuildBudgetStreams(wagonCount, Math.Max(firstStation, station), false);
            int length = 0; foreach (var band in bands) length += band.wagon < 0 ? wagonCount : 1;
            var streams = new SpawnStream[length]; int next = 0;
            // Wagon-first order prevents a single type/wagon from monopolizing the queue.
            for (int wagon = 0; wagon < wagonCount; wagon++) for (int i = 0; i < bands.Length; i++)
                if (bands[i].wagon < 0 || bands[i].wagon == wagon)
                    streams[next++] = new SpawnStream(wagon, i, CountAt(bands[i], station), bands[i].firstAt, bands[i].interval);
            return streams;
        }
        public SpawnStream[] BuildSoloStreams(int wagonCount, int station = 0)
        {
            if (!Validate(wagonCount, out string reason)) throw new ArgumentException(reason);
            if (useThreatBudget) return BuildBudgetStreams(wagonCount, Math.Max(firstStation, station), true);
            // A mobile defender has one station budget. Explicit wagon-only bands belong to Battle.
            int count = 0; foreach (var band in bands) if (band.wagon == -1) count++;
            var streams = new SpawnStream[count]; int next = 0;
            for (int i = 0; i < bands.Length; i++) if (bands[i].wagon == -1)
                streams[next++] = new SpawnStream(0, i, CountAt(bands[i], station), bands[i].firstAt, bands[i].interval);
            return streams;
        }
        public int[] BudgetCountsAt(int station)
        {
            if (!useThreatBudget || !Validate(32, out string reason)) throw new ArgumentException("Invalid threat program.");
            var counts = new int[bands.Length]; var credits = new int[bands.Length];
            int remaining = ThreatBudgetAt(station);
            while (remaining > 0)
            {
                int winner = -1, totalWeight = 0;
                for (int i = 0; i < bands.Length; i++)
                {
                    var band = bands[i];
                    if (station < band.unlockStation || band.threatCost > remaining || counts[i] >= band.maximumCount) continue;
                    totalWeight += band.threatWeight; credits[i] += band.threatWeight;
                    if (winner < 0 || credits[i] > credits[winner]) winner = i;
                }
                if (winner < 0) break;
                counts[winner]++; credits[winner] -= totalWeight; remaining -= bands[winner].threatCost;
            }
            return counts;
        }
        private SpawnStream[] BuildBudgetStreams(int wagonCount, int station, bool solo)
        {
            // One global wave budget, deterministic across capture/restore. No per-frame planning.
            var counts = BudgetCountsAt(station); var streams = new List<SpawnStream>();
            int destinations = solo ? 1 : wagonCount;
            for (int wagon = 0; wagon < destinations; wagon++) for (int type = 0; type < bands.Length; type++)
            {
                int relative = (wagon + destinations - (station + type) % destinations) % destinations;
                int count = counts[type] / destinations + (relative < counts[type] % destinations ? 1 : 0);
                if (count == 0) continue;
                streams.Add(new SpawnStream(wagon, type, count, bands[type].firstAt + wagon * .35f, bands[type].interval));
            }
            return streams.ToArray();
        }
    }
}
