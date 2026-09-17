using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MiniFactory.Machines;

namespace MiniFactory.UI
{
    public class MachineCardView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text productionText;
        [SerializeField] private TMP_Text priceText;

        [Header("Buttons")]
        [SerializeField] private Button unlockButton;
        [SerializeField] private Button upgradeButton;

        private GameController _gameController;
        private int _machineIndex;

        public void Initialize(
            GameController gameController,
            int machineIndex)
        {
            _gameController = gameController;
            _machineIndex = machineIndex;

            unlockButton.onClick.AddListener(OnUnlockClicked);
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

            Refresh();
        }

        public void Refresh()
        {
            if (_gameController == null)
                return;

            if (_machineIndex < 0 ||
                _machineIndex >= _gameController.Machines.Count)
                return;

            MachineModel machine =
                _gameController.Machines[_machineIndex];

            nameText.text = machine.DisplayName;

            if (machine.IsUnlocked)
            {
                stateText.text = "Открыта";

                levelText.text =
                    $"Уровень: {machine.Level}";

                productionText.text =
                    $"Доход: {machine.ProductionPerSecond:F1} / сек";

                priceText.text =
                    $"Улучшение: {machine.UpgradeCost:F0}";

                unlockButton.gameObject.SetActive(false);
                upgradeButton.gameObject.SetActive(true);
            }
            else
            {
                stateText.text = "Закрыта";

                levelText.text = "Уровень: -";

                productionText.text =
                    $"Доход: {machine.ProductionPerSecond:F1} / сек";

                priceText.text =
                    $"Открыть: {machine.UnlockCost:F0}";

                unlockButton.gameObject.SetActive(true);
                upgradeButton.gameObject.SetActive(false);
            }
        }

        private void OnUnlockClicked()
        {
            bool success =
                _gameController.TryUnlockMachine(_machineIndex);

            if (!success)
            {
                Debug.Log(
                    "Не удалось открыть машину. Недостаточно валюты.");
            }

            Refresh();
        }

        private void OnUpgradeClicked()
        {
            bool success =
                _gameController.TryUpgradeMachine(_machineIndex);

            if (!success)
            {
                Debug.Log(
                    "Не удалось улучшить машину. Недостаточно валюты.");
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (unlockButton != null)
                unlockButton.onClick.RemoveListener(OnUnlockClicked);

            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
        }
    }
}