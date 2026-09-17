using System;

namespace MiniFactory.Economy
{
    public class EconomyService
    {
        public double Balance { get; private set; }

        public event Action<double> BalanceChanged;

        public EconomyService(double startingBalance)
        {
            Balance = startingBalance;
        }

        public void AddCurrency(double amount)
        {
            if (amount <= 0)
                return;

            Balance += amount;
            BalanceChanged?.Invoke(Balance);
        }

        public bool TrySpend(double amount)
        {
            if (amount < 0 || Balance < amount)
                return false;

            Balance -= amount;
            BalanceChanged?.Invoke(Balance);

            return true;
        }
    }
}