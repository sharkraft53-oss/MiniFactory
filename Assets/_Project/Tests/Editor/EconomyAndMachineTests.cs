using NUnit.Framework;

using MiniFactory.Config;
using MiniFactory.Economy;
using MiniFactory.Machines;

namespace MiniFactory.Tests
{
    public class EconomyAndMachineTests
    {
        [Test]
        public void TrySpend_WhenBalanceIsEnough_DeductsCurrency()
        {
            // Arrange
            EconomyService economy =
                new EconomyService(1000);

            // Act
            bool result =
                economy.TrySpend(250);

            // Assert
            Assert.IsTrue(result);

            Assert.AreEqual(
                750,
                economy.Balance,
                0.001
            );
        }


        [Test]
        public void TrySpend_WhenBalanceIsNotEnough_DoesNotChangeBalance()
        {
            // Arrange
            EconomyService economy =
                new EconomyService(100);

            // Act
            bool result =
                economy.TrySpend(250);

            // Assert
            Assert.IsFalse(result);

            Assert.AreEqual(
                100,
                economy.Balance,
                0.001
            );
        }


        [Test]
        public void UpgradeMachine_IncreasesLevelProductionAndUpgradeCost()
        {
            // Arrange
            MachineConfig config =
                CreateMachineConfig(
                    unlocked: true
                );

            MachineModel machine =
                new MachineModel(config);

            Assert.AreEqual(
                1,
                machine.Level
            );

            Assert.AreEqual(
                5,
                machine.ProductionPerSecond,
                0.001
            );

            Assert.AreEqual(
                50,
                machine.UpgradeCost,
                0.001
            );


            // Act
            machine.Upgrade();


            // Assert
            Assert.AreEqual(
                2,
                machine.Level
            );

            Assert.AreEqual(
                10,
                machine.ProductionPerSecond,
                0.001
            );

            Assert.AreEqual(
                75,
                machine.UpgradeCost,
                0.001
            );
        }


        [Test]
        public void UnlockMachine_ChangesLockedState()
        {
            // Arrange
            MachineConfig config =
                CreateMachineConfig(
                    unlocked: false
                );

            MachineModel machine =
                new MachineModel(config);

            Assert.IsFalse(
                machine.IsUnlocked
            );


            // Act
            machine.Unlock();


            // Assert
            Assert.IsTrue(
                machine.IsUnlocked
            );
        }


        private static MachineConfig
            CreateMachineConfig(
                bool unlocked)
        {
            return new MachineConfig
            {
                id = "test_machine",
                displayName = "Test Machine",

                unlockedByDefault =
                    unlocked,

                startLevel = 1,

                baseProductionPerSecond = 5,

                unlockCost = 100,

                baseUpgradeCost = 50,

                upgradeCostMultiplier = 1.5
            };
        }
    }
}