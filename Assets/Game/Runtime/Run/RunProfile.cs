using System;
using System.IO;
using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Composition adapter, after match settlement and before active-run retirement.</summary>
    [DefaultExecutionOrder(1000)]
    public sealed class RunProfile : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunPersistence persistence;
        [SerializeField] private int[] weaponPrices = { 0, 20, 40, 60 };
        [SerializeField, Min(0)] private int soloBaseReward = 2, soloCompletedStationReward = 3;
        [SerializeField] private int[] battlePlaceRewards = { 20, 12, 8, 3, 2 };
        private LocalProfileStore store;
        private PlayerProfile profile;
        private RunFlow observed;
        private bool credited, testProfile;
        private float retryAt;
        public string Status { get; private set; }
        public string ProfilePath => store.Path;
        public int Tokens => profile?.tokens ?? 0;
        public int BestSoloStation => profile?.bestSoloStation ?? 0;
        public int LastReward { get; private set; }
        public bool Ready => profile != null;
        // Existing acceptance fixtures never change the production profile or its weapon gates.
        public bool Active => isActiveAndEnabled && (testProfile || !persistence.IsTestStore) && !run.Match.Suppressed;
        public void Configure(RunDriver owner, RunPersistence save, RunEconomy shop)
        { run = owner; persistence = save; shop.ConfigureProfile(this); }
        private void Awake() => UseStore(Path.Combine(Application.persistentDataPath, "player-profile.json"), false);
        public void UseStore(string path, bool isolated)
        { store = new LocalProfileStore(path); profile = store.Load(); testProfile = isolated; observed = null; credited = false; LastReward = 0; retryAt = 0; Status = store.Diagnostic; }
        public bool WeaponUnlocked(int index) => !Active || profile != null && index >= 0 && index < profile.weapons.Length && profile.weapons[index];
        public int WeaponPrice(int index) => index > 0 && index < weaponPrices.Length ? weaponPrices[index] : 0;
        public bool CanUnlock(int index) => Active && Ready && !run.Loading && !persistence.Busy && run.Flow != null &&
            run.Flow.Phase == RunPhase.GameOver && index > 0 && index < profile.weapons.Length && !profile.weapons[index] &&
            WeaponPrice(index) > 0 && Tokens >= WeaponPrice(index) && credited;
        public bool TryUnlock(int index)
        {
            if (!CanUnlock(index)) return false;
            var next = profile.Copy();
            if (!next.TryUnlock(index, WeaponPrice(index)) || !Commit(next)) return false;
            Status = "Silah kalıcı açıldı; koşuda altınla alabilirsin"; return true;
        }
        private bool Commit(PlayerProfile next)
        {
            if (!store.Save(next)) { Status = store.Diagnostic; retryAt = Time.unscaledTime + 5; return false; }
            profile = next; return true;
        }
        public bool TrySettle()
        {
            if (!Active || !Ready || run.Loading || persistence.Busy || run.Flow == null || run.Flow.Phase != RunPhase.GameOver ||
                string.IsNullOrEmpty(persistence.RunId)) return false;
            if (observed != run.Flow) { observed = run.Flow; credited = false; LastReward = 0; }
            if (credited) return false;
            if (Array.IndexOf(profile.settledRuns, persistence.RunId) >= 0) { credited = true; return false; }
            int station = 0, reward;
            if (run.Match.Mode == RunMode.Solo)
            {
                if (run.Player.IsAlive) return false;
                station = run.Flow.Station;
                reward = (int)Math.Min(int.MaxValue, Math.Max(0, (long)soloBaseReward) + Math.Max(0, (long)station - 1) * Math.Max(0, soloCompletedStationReward));
            }
            else
            {
                if (run.Match.Result == BattleResult.None || run.Match.HumanPlace <= 0 || battlePlaceRewards.Length == 0) return false;
                reward = Math.Max(0, battlePlaceRewards[Math.Min(run.Match.HumanPlace - 1, battlePlaceRewards.Length - 1)]);
            }
            var next = profile.Copy();
            if (!next.TryReward(persistence.RunId, reward, station) || !Commit(next)) return false;
            credited = true; LastReward = reward; Status = $"Koşu ödülü: +{reward} jeton"; return true;
        }
        private void LateUpdate()
        {
            if (observed != run.Flow) { observed = run.Flow; credited = false; LastReward = 0; }
            if (!credited && Time.unscaledTime >= retryAt) TrySettle();
        }
    }
}
