using System;
using System.Collections.Generic;
using UnityEngine;

using MiniFactory.Analytics;
using MiniFactory.Boost;
using MiniFactory.Config;
using MiniFactory.Economy;
using MiniFactory.IAP;
using MiniFactory.Machines;
using MiniFactory.Persistence;

namespace MiniFactory
{
    public class GameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig gameConfig;

        private EconomyService _economy;
        private BoostService _boostService;
        private SaveService _saveService;
        private AnalyticsService _analytics;
        private IIAPService _iapService;

        private readonly List<MachineModel> _machines = new();

        private float _debugTimer;
        private long _backgroundStartUnixTime;
        private bool _wasBoostActive;

        public double Balance => _economy?.Balance ?? 0;

        public IReadOnlyList<MachineModel> Machines => _machines;

        public bool IsBoostActive =>
            _boostService != null &&
            _boostService.IsActive;

        public double BoostRemainingSeconds =>
            _boostService?.RemainingSeconds ?? 0;

        public float BoostMultiplier =>
            _boostService?.Multiplier ?? 1f;

        public bool IsIAPInitialized =>
            _iapService != null &&
            _iapService.IsInitialized;

        public double LastOfflineIncome { get; private set; }

        public double LastOfflineSeconds { get; private set; }

        public double BaseProductionPerSecond
        {
            get
            {
                double total = 0;

                foreach (MachineModel machine in _machines)
                {
                    if (machine.IsUnlocked)
                        total += machine.ProductionPerSecond;
                }

                return total;
            }
        }

        public double TotalProductionPerSecond =>
            BaseProductionPerSecond * BoostMultiplier;

        private void Awake()
        {
            InitializeGame();
        }

        private void Update()
        {
            if (_economy == null)
                return;

            double income =
                TotalProductionPerSecond *
                Time.deltaTime;

            _economy.AddCurrency(income);

            CheckBoostFinished();
            DebugGameState();
        }

        private void InitializeGame()
        {
            if (gameConfig == null)
            {
                Debug.LogError("GameConfig is not assigned!");
                return;
            }

            InitializeAnalytics();

            _saveService = new SaveService();

            GameSaveData saveData =
                _saveService.Load();

            InitializeEconomy(saveData);
            InitializeMachines(saveData);
            InitializeBoost(saveData);

            if (saveData != null &&
                saveData.lastSaveUnixTime > 0)
            {
                long now = GetUnixTime();

                ApplyOfflineProgress(
                    saveData.lastSaveUnixTime,
                    now
                );

                CheckExpiredBoostWhileOffline(
                    saveData,
                    now
                );
            }

            _wasBoostActive = IsBoostActive;

            SaveGame();

            SendGameStartedEvent();

            InitializeIAP();

            Debug.Log(
                $"Mini Factory initialized. Machines: {_machines.Count}"
            );

            Debug.Log(
                $"Save path: {_saveService.GetSavePath()}"
            );
        }

        private void InitializeAnalytics()
        {
            _analytics = new AnalyticsService();

            _analytics.AddProvider(
                new ConsoleAnalyticsProvider()
            );
        }

        private void InitializeIAP()
        {
            _iapService =
                new UnityIAPService();

            _iapService.PurchaseSucceeded +=
                OnPurchaseSucceeded;

            _iapService.PurchaseFailed +=
                OnPurchaseFailed;

            _iapService.InitializationFailed +=
                OnIAPInitializationFailed;

            Debug.Log(
                "[IAP] Initialization started."
            );

            _iapService.Initialize();
        }

        private void InitializeEconomy(
            GameSaveData saveData)
        {
            double startingBalance =
                saveData != null
                    ? saveData.balance
                    : gameConfig.startingBalance;

            _economy =
                new EconomyService(
                    startingBalance
                );
        }

        private void InitializeMachines(
            GameSaveData saveData)
        {
            _machines.Clear();

            if (gameConfig.machines == null)
            {
                Debug.LogError(
                    "Machines config is missing."
                );

                return;
            }

            foreach (
                MachineConfig machineConfig
                in gameConfig.machines)
            {
                MachineModel machine =
                    new MachineModel(
                        machineConfig
                    );

                if (saveData != null)
                {
                    ApplySavedMachineState(
                        machine,
                        saveData
                    );
                }

                _machines.Add(machine);
            }
        }

        private void ApplySavedMachineState(
            MachineModel machine,
            GameSaveData saveData)
        {
            foreach (
                MachineSaveData savedMachine
                in saveData.machines)
            {
                if (savedMachine.id != machine.Id)
                    continue;

                machine.ApplyState(
                    savedMachine.level,
                    savedMachine.isUnlocked
                );

                return;
            }
        }

        private void InitializeBoost(
            GameSaveData saveData)
        {
            if (gameConfig.boost == null)
            {
                Debug.LogError(
                    "Boost config is missing."
                );

                return;
            }

            _boostService =
                new BoostService(
                    gameConfig.boost.enabled,
                    gameConfig.boost.durationSeconds,
                    gameConfig.boost.multiplier
                );

            if (saveData != null)
            {
                _boostService.RestoreEndTime(
                    saveData.boostEndUnixTime
                );
            }
        }

        public bool TryUnlockMachine(int index)
        {
            if (!IsValidMachineIndex(index))
                return false;

            MachineModel machine =
                _machines[index];

            if (machine.IsUnlocked)
                return false;

            if (!_economy.TrySpend(machine.UnlockCost))
                return false;

            machine.Unlock();

            _analytics.SendEvent(
                "machine_unlocked",
                new Dictionary<string, object>
                {
                    { "machine_id", machine.Id },
                    { "unlock_cost", machine.UnlockCost },
                    { "balance_after", Balance }
                }
            );

            SaveGame();

            return true;
        }

        public bool TryUpgradeMachine(int index)
        {
            if (!IsValidMachineIndex(index))
                return false;

            MachineModel machine =
                _machines[index];

            if (!machine.IsUnlocked)
                return false;

            double cost =
                machine.UpgradeCost;

            if (!_economy.TrySpend(cost))
                return false;

            machine.Upgrade();

            _analytics.SendEvent(
                "machine_upgraded",
                new Dictionary<string, object>
                {
                    { "machine_id", machine.Id },
                    { "new_level", machine.Level },
                    { "upgrade_cost", cost },
                    {
                        "production_per_second",
                        machine.ProductionPerSecond
                    }
                }
            );

            SaveGame();

            return true;
        }

        public bool TryStartBoost()
        {
            if (_boostService == null)
                return false;

            bool started =
                _boostService.TryStartBoost();

            if (!started)
                return false;

            _wasBoostActive = true;

            _analytics.SendEvent(
                "boost_started",
                new Dictionary<string, object>
                {
                    {
                        "multiplier",
                        gameConfig.boost.multiplier
                    },
                    {
                        "duration_seconds",
                        gameConfig.boost.durationSeconds
                    }
                }
            );

            SaveGame();

            return true;
        }

        public void BuySmallCoinsPack()
        {
            if (_iapService == null)
            {
                Debug.LogWarning(
                    "[IAP] Service is unavailable."
                );

                _analytics.SendEvent(
                    "purchase_failed",
                    new Dictionary<string, object>
                    {
                        {
                            "product_id",
                            IAPProductIds.CoinsPackSmall
                        },
                        {
                            "reason",
                            "iap_service_unavailable"
                        }
                    }
                );

                return;
            }

            Debug.Log(
                $"[IAP] Buy requested: " +
                $"{IAPProductIds.CoinsPackSmall}"
            );

            _iapService.BuyProduct(
                IAPProductIds.CoinsPackSmall
            );
        }

        private void OnPurchaseSucceeded(
            string productId)
        {
            if (productId !=
                IAPProductIds.CoinsPackSmall)
            {
                Debug.LogWarning(
                    $"[IAP] Unknown product: {productId}"
                );

                return;
            }

            double reward =
                gameConfig.smallCoinsPackAmount;

            _economy.AddCurrency(reward);

            SaveGame();

            _analytics.SendEvent(
                "purchase_succeeded",
                new Dictionary<string, object>
                {
                    { "product_id", productId },
                    { "currency_reward", reward },
                    { "balance_after", Balance }
                }
            );

            Debug.Log(
                $"[IAP] Reward granted: " +
                $"+{reward} coins."
            );
        }

        private void OnPurchaseFailed(
            string productId,
            string reason)
        {
            _analytics.SendEvent(
                "purchase_failed",
                new Dictionary<string, object>
                {
                    { "product_id", productId },
                    { "reason", reason }
                }
            );

            Debug.LogWarning(
                $"[IAP] Purchase failed: " +
                $"{productId} | {reason}"
            );
        }

        private void OnIAPInitializationFailed(
            string reason)
        {
            Debug.LogError(
                $"[IAP] Initialization failed: {reason}"
            );

            _analytics.SendEvent(
                "iap_initialization_failed",
                new Dictionary<string, object>
                {
                    { "reason", reason }
                }
            );
        }

        private void CheckBoostFinished()
        {
            bool boostActiveNow =
                IsBoostActive;

            if (!_wasBoostActive ||
                boostActiveNow)
            {
                return;
            }

            _wasBoostActive = false;

            _analytics.SendEvent(
                "boost_finished",
                new Dictionary<string, object>
                {
                    {
                        "multiplier",
                        gameConfig.boost.multiplier
                    }
                }
            );

            SaveGame();
        }

        private void CheckExpiredBoostWhileOffline(
            GameSaveData saveData,
            long now)
        {
            if (saveData.boostEndUnixTime <= 0)
                return;

            if (saveData.boostEndUnixTime >
                    saveData.lastSaveUnixTime &&
                saveData.boostEndUnixTime <= now)
            {
                _analytics.SendEvent(
                    "boost_finished",
                    new Dictionary<string, object>
                    {
                        {
                            "finished_offline",
                            true
                        }
                    }
                );
            }
        }

        private void ApplyOfflineProgress(
            long startUnixTime,
            long endUnixTime)
        {
            if (endUnixTime <= startUnixTime)
                return;

            double elapsedSeconds =
                endUnixTime - startUnixTime;

            double offlineSeconds =
                Math.Min(
                    elapsedSeconds,
                    gameConfig.maxOfflineSeconds
                );

            if (offlineSeconds <= 0)
                return;

            double effectiveStart =
                endUnixTime - offlineSeconds;

            double boostedSeconds = 0;

            if (gameConfig.boost != null &&
                gameConfig.boost.enabled &&
                _boostService != null)
            {
                double boostEnd =
                    _boostService.EndUnixTimeSeconds;

                boostedSeconds =
                    Math.Max(
                        0,
                        boostEnd - effectiveStart
                    );

                boostedSeconds =
                    Math.Min(
                        boostedSeconds,
                        offlineSeconds
                    );
            }

            double normalSeconds =
                offlineSeconds - boostedSeconds;

            double baseProduction =
                BaseProductionPerSecond;

            double normalIncome =
                baseProduction *
                normalSeconds;

            double boostedIncome =
                baseProduction *
                boostedSeconds *
                gameConfig.boost.multiplier;

            double totalIncome =
                normalIncome +
                boostedIncome;

            if (totalIncome > 0)
            {
                _economy.AddCurrency(
                    totalIncome
                );
            }

            LastOfflineIncome =
                totalIncome;

            LastOfflineSeconds =
                offlineSeconds;

            _analytics.SendEvent(
                "offline_income_applied",
                new Dictionary<string, object>
                {
                    {
                        "offline_seconds",
                        offlineSeconds
                    },
                    { "income", totalIncome },
                    {
                        "boosted_seconds",
                        boostedSeconds
                    }
                }
            );

            Debug.Log(
                $"Offline progress applied: " +
                $"{offlineSeconds:F0} sec | " +
                $"Income: {totalIncome:F1} | " +
                $"Boosted time: {boostedSeconds:F0} sec"
            );
        }

        private void SendGameStartedEvent()
        {
            _analytics.SendEvent(
                "game_started",
                new Dictionary<string, object>
                {
                    { "balance", Balance },
                    {
                        "machines_count",
                        _machines.Count
                    }
                }
            );
        }

        private void SaveGame()
        {
            SaveGame(GetUnixTime());
        }

        private void SaveGame(long saveUnixTime)
        {
            if (_saveService == null ||
                _economy == null)
            {
                return;
            }

            GameSaveData saveData =
                new GameSaveData
                {
                    balance = Balance,
                    lastSaveUnixTime =
                        saveUnixTime,
                    boostEndUnixTime =
                        _boostService != null
                            ? _boostService
                                .EndUnixTimeSeconds
                            : 0
                };

            foreach (
                MachineModel machine
                in _machines)
            {
                saveData.machines.Add(
                    new MachineSaveData
                    {
                        id = machine.Id,
                        level = machine.Level,
                        isUnlocked =
                            machine.IsUnlocked
                    }
                );
            }

            _saveService.Save(saveData);
        }

        private bool IsValidMachineIndex(
            int index)
        {
            return index >= 0 &&
                   index < _machines.Count;
        }

        private static long GetUnixTime()
        {
            return DateTimeOffset
                .UtcNow
                .ToUnixTimeSeconds();
        }

        private void OnApplicationPause(
            bool pauseStatus)
        {
            if (pauseStatus)
            {
                _backgroundStartUnixTime =
                    GetUnixTime();

                SaveGame(
                    _backgroundStartUnixTime
                );

                return;
            }

            if (_backgroundStartUnixTime <= 0)
                return;

            long now =
                GetUnixTime();

            ApplyOfflineProgress(
                _backgroundStartUnixTime,
                now
            );

            _backgroundStartUnixTime = 0;

            SaveGame(now);
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        private void OnDestroy()
        {
            if (_iapService == null)
                return;

            _iapService.PurchaseSucceeded -=
                OnPurchaseSucceeded;

            _iapService.PurchaseFailed -=
                OnPurchaseFailed;

            _iapService.InitializationFailed -=
                OnIAPInitializationFailed;
        }

        private void DebugGameState()
        {
            _debugTimer += Time.deltaTime;

            if (_debugTimer < 2f)
                return;

            _debugTimer = 0;

            Debug.Log(
                $"Balance: {Balance:F1} | " +
                $"Production: " +
                $"{TotalProductionPerSecond:F1}/sec | " +
                $"Boost: " +
                $"{(IsBoostActive ? "ON" : "OFF")} | " +
                $"IAP: " +
                $"{(IsIAPInitialized ? "READY" : "NOT READY")}"
            );
        }
    }
}