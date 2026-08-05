using System;
using UnityEngine;

namespace WorldOfSpirits.Progression
{
    [DisallowMultipleComponent]
    public sealed class SpiritDustWallet : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingSpiritDust;
        private int balance;

        public int Balance => balance;
        public event Action<int> BalanceChanged;

        private void Awake()
        {
            balance = Mathf.Max(0, startingSpiritDust);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            balance += amount;
            BalanceChanged?.Invoke(balance);
        }

        public bool TrySpend(int amount)
        {
            amount = Mathf.Max(0, amount);
            if (balance < amount) return false;
            balance -= amount;
            BalanceChanged?.Invoke(balance);
            return true;
        }
    }
}
