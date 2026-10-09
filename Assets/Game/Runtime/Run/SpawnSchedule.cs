using System;

namespace WaitYourTurn.Run
{
    [Serializable]
    public sealed class ScheduleSnapshot
    {
        public int[] emitted, failures;
        public float[] retryAt;
        public SpawnRequest[] pending;
        public int cursor, spawned, dropped, attempts, suppressed;
    }
    public enum SpawnResult { Spawned, CapacityFull, InvalidPosition, Unoccupied }
    public readonly struct SpawnStream
    {
        public readonly int Wagon, Profile, Count;
        public readonly float Start, Interval;
        public SpawnStream(int wagon, int profile, int count, float start, float interval)
        { Wagon = wagon; Profile = profile; Count = count; Start = start; Interval = interval; }
    }
    [Serializable]
    public struct SpawnRequest
    {
        public int Wagon, Profile, Ordinal, Failures;
        public int Stream;
    }

    /// <summary>Bounded, fair scheduler. No Unity objects, frame allocations, catch-up burst or hidden spawning.</summary>
    public sealed class SpawnSchedule
    {
        private readonly SpawnStream[] streams;
        private readonly int[] emitted;
        private readonly int[] failures;
        private readonly float[] retryAt;
        private readonly bool[] queued;
        private readonly SpawnRequest[] pending;
        private readonly int maxFailures;
        private readonly float retryDelay;
        private int head, count, cursor;
        private bool stopped;
        public int Pending => count;
        public int Spawned { get; private set; }
        public int Dropped { get; private set; }
        public int Attempts { get; private set; }
        public int SuppressedStreams { get; private set; }
        public SpawnSchedule(SpawnStream[] source, int capacity, int retries, float delay)
        {
            if (source == null || source.Length == 0 || source.Length > 1024 || capacity < 1 || capacity > 256 || retries < 1 ||
                !(delay > 0) || float.IsInfinity(delay)) throw new ArgumentException("Invalid spawn schedule limits.");
            foreach (var stream in source)
                if (stream.Wagon < 0 || stream.Profile < 0 || stream.Count < 1 || stream.Start < 0 ||
                    float.IsNaN(stream.Start) || float.IsInfinity(stream.Start) || !(stream.Interval > 0) || float.IsInfinity(stream.Interval))
                    throw new ArgumentException("Invalid spawn stream.");
            streams = (SpawnStream[])source.Clone(); emitted = new int[source.Length]; failures = new int[source.Length];
            retryAt = new float[source.Length]; queued = new bool[source.Length];
            pending = new SpawnRequest[capacity]; maxFailures = retries; retryDelay = delay;
        }
        public void Tick(float elapsed, int budget, Func<SpawnRequest, SpawnResult> spawn)
        {
            if (stopped || float.IsNaN(elapsed) || float.IsInfinity(elapsed) || elapsed < 0 || budget < 1 || spawn == null) return;
            // Inspect each stream at most once; bounded enqueue work even after a very long frame.
            int inspected = 0, added = 0;
            while (count < pending.Length && inspected++ < streams.Length && added < budget)
            {
                int index = cursor; cursor = (cursor + 1) % streams.Length;
                var stream = streams[index];
                if (queued[index] || emitted[index] >= stream.Count || elapsed < retryAt[index] || elapsed < stream.Start + emitted[index] * stream.Interval) continue;
                queued[index] = true;
                Enqueue(new SpawnRequest { Wagon = stream.Wagon, Profile = stream.Profile, Ordinal = emitted[index], Failures = failures[index], Stream = index }); added++;
            }
            // Snapshot count: a rejected request cannot consume all retries in this frame.
            int tries = Math.Min(budget, count);
            for (int i = 0; i < tries; i++)
            {
                var request = pending[head]; head = (head + 1) % pending.Length; count--;
                int index = request.Stream; queued[index] = false;
                Attempts++;
                var result = spawn(request);
                // No deferred invasion when an empty wagon is assigned later. A new station builds fresh streams.
                if (result == SpawnResult.Unoccupied)
                { emitted[index] = streams[index].Count; failures[index] = 0; SuppressedStreams++; continue; }
                if (result == SpawnResult.Spawned) { Spawned++; emitted[index]++; failures[index] = 0; continue; }
                if (result == SpawnResult.InvalidPosition && ++failures[index] >= maxFailures)
                { Dropped++; emitted[index]++; failures[index] = 0; continue; }
                // A full wagon releases its queue slot. Its stream retries later, allowing other wagons to enqueue.
                retryAt[index] = elapsed + retryDelay;
            }
        }
        public void Clear() { head = count = 0; stopped = true; }
        public ScheduleSnapshot Capture()
        {
            var data = new ScheduleSnapshot { emitted = (int[])emitted.Clone(), failures = (int[])failures.Clone(),
                retryAt = (float[])retryAt.Clone(), pending = new SpawnRequest[count], cursor = cursor,
                spawned = Spawned, dropped = Dropped, attempts = Attempts, suppressed = SuppressedStreams };
            for (int i = 0; i < count; i++) data.pending[i] = pending[(head + i) % pending.Length]; return data;
        }
        public bool Restore(ScheduleSnapshot data)
        {
            if (data == null || data.emitted == null || data.failures == null || data.retryAt == null || data.pending == null ||
                data.emitted.Length != streams.Length || data.failures.Length != streams.Length || data.retryAt.Length != streams.Length ||
                data.pending.Length > pending.Length || data.cursor < 0 || data.cursor >= streams.Length ||
                data.spawned < 0 || data.dropped < 0 || data.attempts < 0 || data.suppressed < 0 || data.suppressed > streams.Length) return false;
            for (int i = 0; i < streams.Length; i++)
                if (data.emitted[i] < 0 || data.emitted[i] > streams[i].Count || data.failures[i] < 0 || data.failures[i] >= maxFailures ||
                    !(data.retryAt[i] >= 0) || float.IsInfinity(data.retryAt[i])) return false;
            var seen = new bool[streams.Length];
            foreach (var request in data.pending)
            {
                int s = request.Stream;
                if (s < 0 || s >= streams.Length || seen[s] || request.Wagon != streams[s].Wagon || request.Profile != streams[s].Profile ||
                    request.Ordinal != data.emitted[s] || request.Ordinal >= streams[s].Count || request.Failures != data.failures[s]) return false;
                seen[s] = true;
            }
            Array.Copy(data.emitted, emitted, emitted.Length); Array.Copy(data.failures, failures, failures.Length);
            Array.Copy(data.retryAt, retryAt, retryAt.Length); Array.Copy(seen, queued, seen.Length);
            Array.Copy(data.pending, pending, data.pending.Length); head = 0; count = data.pending.Length; cursor = data.cursor;
            Spawned = data.spawned; Dropped = data.dropped; Attempts = data.attempts; SuppressedStreams = data.suppressed; stopped = false; return true;
        }
        private void Enqueue(SpawnRequest request) { pending[(head + count) % pending.Length] = request; count++; }
    }
}
