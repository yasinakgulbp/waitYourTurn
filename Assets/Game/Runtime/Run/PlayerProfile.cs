using System;
using System.Collections.Generic;

namespace WaitYourTurn.Run
{
    [Serializable]
    public sealed class PlayerProfile
    {
        public const int Version = 2, ReceiptLimit = 128;
        public int version = Version, tokens, bestSoloStation;
        public int healthLevel, rangeLevel;
        public bool[] weapons = { true, false, false, false };
        public string[] settledRuns = Array.Empty<string>();
        public bool Valid()
        {
            if (version != Version || tokens < 0 || bestSoloStation < 0 || weapons == null || weapons.Length != 4 || !weapons[0] ||
                settledRuns == null || settledRuns.Length > ReceiptLimit || healthLevel < 0 || healthLevel > PermanentUpgradeRules.LevelLimit ||
                rangeLevel < 0 || rangeLevel > PermanentUpgradeRules.LevelLimit) return false;
            var unique = new HashSet<string>();
            foreach (var id in settledRuns) if (!Guid.TryParseExact(id, "N", out _) || !unique.Add(id)) return false;
            return true;
        }
        public PlayerProfile Copy() => new PlayerProfile { version = version, tokens = tokens, bestSoloStation = bestSoloStation,
            healthLevel = healthLevel, rangeLevel = rangeLevel,
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
        public int UpgradeLevel(PermanentUpgrade kind) => kind == PermanentUpgrade.Health ? healthLevel : kind == PermanentUpgrade.Range ? rangeLevel : -1;
        public bool TryUpgrade(PermanentUpgrade kind, PermanentUpgradeRules rules)
        {
            if (rules == null || !rules.Valid || !PermanentUpgradeRules.Known(kind)) return false;
            int price = rules.Price(kind, UpgradeLevel(kind));
            if (price <= 0 || tokens < price) return false;
            tokens -= price;
            if (kind == PermanentUpgrade.Health) healthLevel++; else rangeLevel++;
            return true;
        }
        public bool MigratePrevious()
        {
            if (version != 1 || healthLevel != 0 || rangeLevel != 0) return false;
            version = Version; return Valid(); // Missing v1 level fields become zero; preserve money/unlocks/receipts.
        }
    }
}
