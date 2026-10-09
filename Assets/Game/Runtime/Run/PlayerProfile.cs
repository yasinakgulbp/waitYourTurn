using System;
using System.Collections.Generic;

namespace WaitYourTurn.Run
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public const int Version = 1, ReceiptLimit = 128;
        public int version = Version, tokens, bestSoloStation;
        public bool[] weapons = { true, false, false, false };
        public string[] settledRuns = Array.Empty<string>();
        public bool Valid()
        {
            if (version != Version || tokens < 0 || bestSoloStation < 0 || weapons == null || weapons.Length != 4 || !weapons[0] ||
                settledRuns == null || settledRuns.Length > ReceiptLimit) return false;
            var unique = new HashSet<string>();
            foreach (var id in settledRuns) if (!Guid.TryParseExact(id, "N", out _) || !unique.Add(id)) return false;
            return true;
        }
        public PlayerProfile Copy() => new PlayerProfile { version = version, tokens = tokens, bestSoloStation = bestSoloStation,
            weapons = (bool[])weapons.Clone(), settledRuns = (string[])settledRuns.Clone() };
        public bool TryReward(string id, int amount, int soloStation)
        {
            if (!Guid.TryParseExact(id, "N", out _) || amount < 0 || soloStation < 0 || amount > int.MaxValue - tokens ||
                Array.IndexOf(settledRuns, id) >= 0) return false;
            var ids = new List<string>(settledRuns);
            if (ids.Count == ReceiptLimit) ids.RemoveAt(0);
            ids.Add(id); settledRuns = ids.ToArray(); tokens += amount;
            bestSoloStation = Math.Max(bestSoloStation, soloStation); return true;
        }
        public bool TryUnlock(int index, int price)
        {
            if (index <= 0 || index >= weapons.Length || weapons[index] || price <= 0 || tokens < price) return false;
            tokens -= price; weapons[index] = true; return true;
        }
    }
}
