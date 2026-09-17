using TMPro;
using UnityEngine;

namespace MiniFactory.UI
{
    public class MainUIController : MonoBehaviour
    {
        [Header("Game")]
        [SerializeField] private GameController gameController;

        [Header("Top Panel")]
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text productionText;

        [Header("Machine Cards")]
        [SerializeField] private MachineCardView[] machineCards;

        private void Start()
        {
            if (gameController == null)
            {
                Debug.LogError("GameController is not assigned.");
                return;
            }

            for (int i = 0; i < machineCards.Length; i++)
            {
                if (i >= gameController.Machines.Count)
                    break;

                machineCards[i].Initialize(gameController, i);
            }

            RefreshUI();
        }

        private void Update()
        {
            RefreshTopPanel();
        }

        private void RefreshUI()
        {
            RefreshTopPanel();

            foreach (MachineCardView card in machineCards)
            {
                if (card != null)
                    card.Refresh();
            }
        }

        private void RefreshTopPanel()
        {
            if (gameController == null)
                return;

            balanceText.text =
                $"Баланс: {gameController.Balance:F0}";

            productionText.text =
                $"Доход: {gameController.TotalProductionPerSecond:F1} / сек";
        }
    }
}