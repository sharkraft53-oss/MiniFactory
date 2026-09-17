using System;

namespace MiniFactory.Config
{
    [Serializable]
    public class BoostConfig
    {
        public bool enabled = true;
        public float durationSeconds = 300f;
        public float multiplier = 2f;
    }
}