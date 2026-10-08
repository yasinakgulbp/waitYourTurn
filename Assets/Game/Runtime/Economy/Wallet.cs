namespace WaitYourTurn.Economy
{
    /// <summary>Integer run currency. A pending purchase reserves funds without losing them on failure.</summary>
    public sealed class Wallet
    {
        public int Balance { get; private set; }
        public int Available => Balance - reserved;
        private int reserved;
        private bool purchasing;
        public bool Reset(int amount)
        { if (purchasing || amount < 0) return false; Balance = amount; return true; }
        public bool TryCredit(int amount)
        { if (amount <= 0 || amount > int.MaxValue - Balance) return false; Balance += amount; return true; }
        public bool TrySpend(int amount)
        { if (amount <= 0 || amount > Available) return false; Balance -= amount; return true; }
        internal bool Reserve(int amount)
        { if (purchasing || amount <= 0 || amount > Available) return false; reserved = amount; purchasing = true; return true; }
        internal void Finish(bool applied)
        { if (applied) Balance -= reserved; reserved = 0; purchasing = false; }
    }
}
