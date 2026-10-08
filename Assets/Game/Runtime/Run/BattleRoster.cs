using System;

namespace WaitYourTurn.Run
{
    /// <summary>Pure elimination/rank rules. A frame's deaths are submitted as one batch.</summary>
    public sealed class BattleRoster
    {
        private readonly bool[] alive;
        private readonly int[] places;
        public int Count => alive.Length;
        public int Living { get; private set; }
        public bool Finished => places[0] > 0 || Living <= 1;
        public bool HumanWon => Living == 1 && alive[0];
        public BattleRoster(int count)
        {
            if (count < 2 || count > 64) throw new ArgumentOutOfRangeException(nameof(count));
            alive = new bool[count]; places = new int[count]; Living = count;
            for (int i = 0; i < count; i++) alive[i] = true;
        }
        public bool IsAlive(int index) => alive[index];
        public int Place(int index) => places[index];
        public void Eliminate(bool[] dead)
        {
            if (dead == null || dead.Length != Count) throw new ArgumentException("One flag per participant required.");
            int removed = 0;
            for (int i = 0; i < Count; i++) if (alive[i] && dead[i]) removed++;
            if (removed == 0) return;
            int rank = Living - removed + 1;
            for (int i = 0; i < Count; i++) if (alive[i] && dead[i]) { alive[i] = false; places[i] = rank; }
            Living -= removed;
            if (Living == 1) for (int i = 0; i < Count; i++) if (alive[i]) places[i] = 1;
        }
        public static int[] Assign(int wagonCount, int humanWagon, bool[] living, Random random)
        {
            if (living == null || living.Length > wagonCount || wagonCount < 1 || humanWagon < 0 ||
                humanWagon >= wagonCount || random == null || !living[0]) throw new ArgumentException("Invalid assignment inputs.");
            var order = new int[wagonCount - 1]; int n = 0;
            for (int i = 0; i < wagonCount; i++) if (i != humanWagon) order[n++] = i;
            for (int i = order.Length - 1; i > 0; i--) { int j = random.Next(i + 1); (order[i], order[j]) = (order[j], order[i]); }
            var result = new int[living.Length]; result[0] = humanWagon; n = 0;
            for (int i = 1; i < living.Length; i++) result[i] = living[i] ? order[n++] : -1;
            return result;
        }
    }
}
