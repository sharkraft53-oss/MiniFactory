using System;

namespace MiniFactory.Config
{
    [Serializable]
    public class MachineConfig
    {
        public string id;
        public string displayName;

        public bool unlockedByDefault;

        public int startLevel = 1;

        public double baseProductionPerSecond = 1;
        public double unlockCost = 100;

        public double baseUpgradeCost = 50;
        public double upgradeCostMultiplier = 1.5;
    }
}