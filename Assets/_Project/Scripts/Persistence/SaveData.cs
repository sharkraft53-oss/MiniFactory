using System;
using System.Collections.Generic;

namespace MiniFactory.Persistence
{
    [Serializable]
    public class MachineSaveData
    {
        public string id;
        public int level;
        public bool isUnlocked;
    }

    [Serializable]
    public class GameSaveData
    {
        public double balance;

        public List<MachineSaveData> machines =
            new List<MachineSaveData>();

        public long lastSaveUnixTime;

        public long boostEndUnixTime;
    }
}