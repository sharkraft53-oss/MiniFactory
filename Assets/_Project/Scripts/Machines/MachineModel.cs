using System;
using MiniFactory.Config;

namespace MiniFactory.Machines
{
    public class MachineModel
    {
        public string Id { get; }

        public string DisplayName { get; }

        public int Level { get; private set; }

        public bool IsUnlocked { get; private set; }

        private readonly MachineConfig _config;

        public MachineModel(
            MachineConfig config)
        {
            _config = config;

            Id = config.id;
            DisplayName = config.displayName;

            Level =
                Math.Max(
                    1,
                    config.startLevel
                );

            IsUnlocked =
                config.unlockedByDefault;
        }

        public double ProductionPerSecond =>
            _config.baseProductionPerSecond *
            Level;

        public double UpgradeCost =>
            _config.baseUpgradeCost *
            Math.Pow(
                _config.upgradeCostMultiplier,
                Level - 1
            );

        public double UnlockCost =>
            _config.unlockCost;

        public void Unlock()
        {
            IsUnlocked = true;
        }

        public void Upgrade()
        {
            Level++;
        }

        public void ApplyState(
            int level,
            bool isUnlocked)
        {
            Level =
                Math.Max(
                    1,
                    level
                );

            IsUnlocked = isUnlocked;
        }
    }
}