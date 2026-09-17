using UnityEngine;

namespace MiniFactory.Config
{
    [CreateAssetMenu(
        fileName = "GameConfig",
        menuName = "Mini Factory/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Economy")]
        public double startingBalance = 0;

        [Header("Machines")]
        public MachineConfig[] machines;

        [Header("Boost")]
        public BoostConfig boost;

        [Header("Offline")]
        public float maxOfflineSeconds = 14400f;

        [Header("IAP")]
        public double smallCoinsPackAmount = 5000;
    }
}