using System;
using UnityEngine;
using Monk.Infrastructure;

namespace Monk.Presentation
{
    public class CoinManager : MonoBehaviour
    {
        public const string CoinTotalKey = "monk.coins.total";

        [SerializeField] private HUDView hudView;

        public event Action<int> OnCoinsChanged;

        public int TotalCoins { get; private set; }

        private PlayerPrefsStorage storage;

        private void Awake()
        {
            storage = new PlayerPrefsStorage();
            TotalCoins = storage.GetInt(CoinTotalKey, 0);

            TryResolveHud();
            PublishCoins();
        }

        private void Start()
        {
            TryResolveHud();
            PublishCoins();
        }

        private void TryResolveHud()
        {
            if (hudView == null)
            {
                hudView = FindFirstObjectByType<HUDView>();
            }
        }

        private void PublishCoins()
        {
            hudView?.UpdateScore(TotalCoins);
            OnCoinsChanged?.Invoke(TotalCoins);
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            TotalCoins += amount;
            storage.SetInt(CoinTotalKey, TotalCoins);
            storage.Save();

            TryResolveHud();
            PublishCoins();
        }

        public bool CanAfford(int amount)
        {
            return amount > 0 && TotalCoins >= amount;
        }

        public bool SpendCoins(int amount)
        {
            if (!CanAfford(amount))
            {
                return false;
            }

            TotalCoins -= amount;
            storage.SetInt(CoinTotalKey, TotalCoins);
            storage.Save();

            TryResolveHud();
            PublishCoins();
            return true;
        }
    }
}
