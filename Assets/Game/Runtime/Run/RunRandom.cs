using System;

namespace WaitYourTurn.Run
{
    /// <summary>Explicit portable RNG state; no reflection into framework Random internals.</summary>
    public sealed class RunRandom : Random
    {
        public uint State { get; set; }
        public RunRandom(int seed) { State = unchecked((uint)seed); if (State == 0) State = 0x9E3779B9; }
        protected override double Sample()
        {
            uint x = State; x ^= x << 13; x ^= x >> 17; x ^= x << 5; State = x;
            return x / 4294967296d;
        }
        public override int Next(int maxValue)
        { if (maxValue < 0) throw new ArgumentOutOfRangeException(nameof(maxValue)); return (int)(Sample() * maxValue); }
        public override int Next(int minValue, int maxValue)
        { if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(maxValue)); return (int)(minValue + Sample() * ((long)maxValue - minValue)); }
        public override int Next() => Next(int.MaxValue);
        public override double NextDouble() => Sample();
    }
}
