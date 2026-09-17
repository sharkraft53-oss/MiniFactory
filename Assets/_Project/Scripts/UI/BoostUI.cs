using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiniFactory.UI
{
    public class BoostUI : MonoBehaviour
    {
        [Header("Game")]
        [SerializeField] private GameController gameController;

        [Header("UI")]
        [SerializeField] private Button boostButton;
        [SerializeField] private TMP_Text boostButtonText;
        [SerializeField] private TMP_Text timerText;

        private void Start()
        {
            if (boostButton == null)
            {
                Debug.LogError("Boost Button is not assigned.");
                return;
            }

            boostButton.onClick.AddListener(OnBoostClicked);

            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void OnBoostClicked()
        {
            if (gameController == null)
                return;

            gameController.TryStartBoost();

            Refresh();
        }

        private void Refresh()
        {
            if (gameController == null)
                return;

            if (gameController.IsBoostActive)
            {
                int remainingSeconds =
                    Mathf.CeilToInt(
                        (float)gameController.BoostRemainingSeconds
                    );

                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;

                timerText.text =
                    $"Boost x{gameController.BoostMultiplier:F0} " +
                    $"{minutes:00}:{seconds:00}";

                boostButton.interactable = false;

                if (boostButtonText != null)
                    boostButtonText.text = "BOOST ACTIVE";
            }
            else
            {
                timerText.text = "Boost готов";

                boostButton.interactable = true;

                if (boostButtonText != null)
                    boostButtonText.text = "BOOST x2";
            }
        }

        private void OnDestroy()
        {
            if (boostButton != null)
                boostButton.onClick.RemoveListener(OnBoostClicked);
        }
    }
}