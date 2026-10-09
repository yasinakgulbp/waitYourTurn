using System;
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
        [Min(0)] public float firstAt = 2;
        [Min(.01f)] public float interval = 3;
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
        public bool Validate(int wagons, out string reason)
        {
            reason = null;
            if (firstStation < 1 || wagons < 1 || wagons > 32 || globalLiveLimit < 1 || wagonLiveLimit < 1 ||
                queueCapacity < 1 || queueCapacity > 256 || requestsPerFrame < 1 || requestsPerFrame > 32 ||
                warmupPerFrame < 1 || warmupPerFrame > 32 || positionAttempts < 1 || positionAttempts > 16 ||
                !(retryDelay > 0) || float.IsInfinity(retryDelay)) reason = "Invalid limits (queue 1–256, frame budgets 1–32, position attempts 1–16).";
            else if (bands == null || bands.Length == 0 || bands.Length > 32) reason = "Expected 1–32 spawn bands.";
            else for (int i = 0; i < bands.Length; i++)
            {
                var b = bands[i];
                if (b == null || b.profile == null || !b.profile.Valid || b.wagon < -1 || b.wagon >= wagons ||
                    b.count < 1 || b.count > 1000 || b.firstAt < 0 || float.IsNaN(b.firstAt) || float.IsInfinity(b.firstAt) ||
                    !(b.interval > 0) || float.IsInfinity(b.interval))
                { reason = $"Band {i}: invalid profile, wagon index, count (1–1000), start or interval."; break; }
            }
            return reason == null;
        }
        public SpawnStream[] BuildStreams(int wagonCount)
        {
            if (!Validate(wagonCount, out string reason)) throw new ArgumentException(reason);
            int length = 0; foreach (var band in bands) length += band.wagon < 0 ? wagonCount : 1;
            var streams = new SpawnStream[length]; int next = 0;
            // Wagon-first order prevents a single type/wagon from monopolizing the queue.
            for (int wagon = 0; wagon < wagonCount; wagon++) for (int i = 0; i < bands.Length; i++)
                if (bands[i].wagon < 0 || bands[i].wagon == wagon)
                    streams[next++] = new SpawnStream(wagon, i, bands[i].count, bands[i].firstAt, bands[i].interval);
            return streams;
        }
        public SpawnStream[] BuildSoloStreams(int wagonCount)
        {
            if (!Validate(wagonCount, out string reason)) throw new ArgumentException(reason);
            // A mobile defender has one station budget. Explicit wagon-only bands belong to Battle.
            int count = 0; foreach (var band in bands) if (band.wagon == -1) count++;
            var streams = new SpawnStream[count]; int next = 0;
            for (int i = 0; i < bands.Length; i++) if (bands[i].wagon == -1)
                streams[next++] = new SpawnStream(0, i, bands[i].count, bands[i].firstAt, bands[i].interval);
            return streams;
        }
    }
}
