using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Run
{
    public enum RunMode { Battle, Solo }
    public enum BattleResult { None, Won, Eliminated, Tied }

    /// <summary>Mode policy and the sole multi-participant assignment authority. RunFlow remains mode-independent.</summary>
    [DefaultExecutionOrder(-450)]
    public sealed class RunMatch : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private BotController[] bots;
        [SerializeField] private RunMode mode = RunMode.Battle;
        private RunRandom random;
        public uint RandomState => random.State;
        private bool[] living, dead;
        private Vector3[] points;
        private int[] assignment;
        private bool suppressed;
        public RunMode Mode => mode;
        public BotController[] Bots => bots;
        public BattleRoster Roster { get; private set; }
        public BattleResult Result { get; private set; }
        public bool Active => isActiveAndEnabled && !suppressed && mode == RunMode.Battle;
        public bool Suppressed => suppressed;
        public bool AiAllowed { get; set; } = true;
        public int HumanPlace => Roster?.Place(0) ?? 0;
        public void Configure(RunDriver owner, RunEconomy shop, BotController[] participants, RunMode initialMode = RunMode.Battle)
        { run = owner; economy = shop; bots = participants; mode = initialMode; run.ConfigureMatch(this); }
        // Only sandbox acceptance fixtures use suppression; normal mode changes start a new run explicitly.
        public void SetSuppressed(bool value) { suppressed = value; if (value) ClearBots(); }
        public void SetMode(RunMode value) { mode = value; run.Restart(); }
        private void OnEnable() { foreach (var wagon in run.Wagons) wagon.Enemies.Killed += OnKilled; }
        private void OnDisable()
        {
            foreach (var wagon in run.Wagons) wagon.Enemies.Killed -= OnKilled;
            ClearBots();
        }
        private void ClearBots()
        {
            foreach (var bot in bots)
            {
                if (bot.Wagon != null && bot.Wagon.Defender == bot.Health) bot.Wagon.SetDefender(null);
                bot.Clear();
            }
        }
        public void BeginRun(int seed)
        {
            ClearBots(); Roster = null; Result = BattleResult.None;
            if (!Active) return;
            if (bots == null || bots.Length < 1 || bots.Length >= run.Wagons.Length)
                throw new System.InvalidOperationException("Battle needs at least one bot and one distinct wagon per participant.");
            foreach (var bot in bots) if (bot.Profile == null || !bot.Profile.Valid || bot.Profile.preferredWeapon >= bot.Weapon.WeaponCount)
                throw new System.InvalidOperationException("Invalid bot profile or weapon inventory.");
            Roster = new BattleRoster(bots.Length + 1); living = new bool[Roster.Count]; dead = new bool[Roster.Count]; points = new Vector3[Roster.Count];
            random = new RunRandom(seed ^ 0x4B07);
            foreach (var bot in bots) bot.Begin(run, economy.Catalog, economy.Defenses);
        }
        public bool TryAssign(int preferredHumanWagon, bool arrival)
        {
            if (!Active || Roster == null) return false;
            living[0] = run.Player.IsAlive;
            for (int i = 0; i < bots.Length; i++) living[i + 1] = bots[i].Health.IsAlive;
            assignment = BattleRoster.Assign(run.Wagons.Length, preferredHumanWagon, living, random);
            // Disable old participant bodies together before preflighting the complete assignment.
            run.Player.GetComponent<CharacterController>().enabled = false;
            foreach (var bot in bots) if (bot.gameObject.activeSelf) bot.GetComponent<CharacterController>().enabled = false;
            bool valid = true;
            for (int i = 0; i < assignment.Length; i++)
                if (assignment[i] >= 0 && !run.Wagons[assignment[i]].TrySafePoint(i == 0 ? run.Player : bots[i - 1].Health, out points[i], true)) valid = false;
            if (!valid)
            {
                run.Player.GetComponent<CharacterController>().enabled = true;
                foreach (var bot in bots) if (bot.gameObject.activeSelf && bot.Health.IsAlive) bot.GetComponent<CharacterController>().enabled = true;
                return false;
            }
            foreach (var wagon in run.Wagons) wagon.SetDefender(null);
            for (int i = 1; i < assignment.Length; i++)
                if (assignment[i] >= 0) bots[i - 1].Assign(run.Wagons[assignment[i]], points[i], arrival);
            run.ApplyPlayerAssignment(run.Wagons[assignment[0]], points[0], arrival);
            return true;
        }
        private void Update()
        {
            if (!Active || Roster == null) return;
            bool paused = !AiAllowed || run.Loading || run.Flow == null || run.Flow.Paused || !run.Player.IsAlive;
            foreach (var bot in bots) bot.SetPaused(paused || !bot.Health.IsAlive);
        }
        private void LateUpdate()
        {
            if (!Active || run.Loading || Roster == null || Result != BattleResult.None) return;
            dead[0] = !run.Player.IsAlive;
            for (int i = 0; i < bots.Length; i++)
            {
                dead[i + 1] = !bots[i].Health.IsAlive;
                if (dead[i + 1] && bots[i].Wagon != null && bots[i].Wagon.Defender == bots[i].Health)
                    bots[i].Wagon.SetDefender(null);
                if (dead[i + 1]) bots[i].gameObject.SetActive(false);
            }
            Roster.Eliminate(dead);
            if (!Roster.Finished) return;
            Result = Roster.HumanWon ? BattleResult.Won : Roster.Living == 0 && HumanPlace == 1 ? BattleResult.Tied : BattleResult.Eliminated;
            run.Flow.EndRun();
        }
        private void OnKilled(DeathNotice death, int reward)
        { if (Active) foreach (var bot in bots) if (bot.Health.IsAlive) bot.ObserveKill(death, reward); }
        public void RestoreRoster(int[] ranks, uint rng)
        { Roster.Restore(ranks); random.State = rng; }
    }
}
