using System;
using System.Collections.Generic;
using DatdevUlts.DateTimeScripts;
using GameTool.Assistants.DictionarySerialize;
using GameToolSample.Scripts.Enum;
using UnityEngine;

namespace GameToolSample.GameDataScripts.Scripts
{
    [Serializable]
    public class DataField
    {
        [Header("GAME CHECK")] public bool FirstOpen;

        public bool FirstPlay;
        public bool Rated;
        public bool RemoveAds;

        public bool GDPR = true;
        public bool GDPRShowed;

        [Header("SETTING")] public bool MuteAll;

        public bool Music = true;
        public bool SoundFX = true;
        public bool Vibrate = true;

        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SoundFXVolume = 1f;

        [Header("RESOURCES")] public int Coin;

        public int Diamond;

        [Header("GAMEPLAY")] public int VictoryCount;

        public int LoseCount;

        [Header("LEVEL")] public int CurrentLevel = 1;

        public int LevelUnlocked = 1;
        public List<int> ListLevelUnlockID = new List<int> { 0 };

        public Dict<LevelPlayInfoKey, LevelPlayInfoData> DictLevelPlayInfoData =
            new Dict<LevelPlayInfoKey, LevelPlayInfoData>();

        [Header("DAILY REWARD")] [DateTimeAsTicks]
        public long DayLogin = 1;

        public int DayDailyReward;

        [Header("SPIN")] public float StartAngleSpin;

        public bool CanSpin;
        public long TimeSpin = 1;
        public float CountDownTimeSpin = -1;
        public List<int> ListIdSkinSpin = new List<int>();

        [Header("LANGUAGE")] public int CurrentLanguage;
        [Header("ITEM")] public int SelectedShopBgId;
        public List<int> ListShopBgOwned;


    }


    [Serializable]
    public class SkinAdsProgress
    {
        public int id;
        public int currentProgress;

        public SkinAdsProgress(int id = 0, int currentProgress = 0)
        {
            this.id = id;
            this.currentProgress = currentProgress;
        }
    }

    [Serializable]
    public class LevelPlayInfoData
    {
        public LevelPlayInfoKey levelId;
        public int playCount;
        public int victoryCount;
        public int loseCount;
        public int deadCount;
        public int reviveCount;
        public int skipCount;
        public int replayCount;
        public int score;

        public LevelPlayInfoData(LevelPlayInfoKey levelId, int playCount = 0, int victoryCount = 0, int loseCount = 0,
            int deadCount = 0, int reviveCount = 0,
            int skipCount = 0, int replayCount = 0, int score = 0)
        {
            this.levelId = levelId;
            this.playCount = playCount;
            this.victoryCount = victoryCount;
            this.loseCount = loseCount;
            this.deadCount = deadCount;
            this.reviveCount = reviveCount;
            this.skipCount = skipCount;
            this.replayCount = replayCount;
            this.score = score;
        }
    }

    [Serializable]
        public class LevelPlayInfoKey
        {
            public int level;
            public AnalyticID.GameModeName gameMode;
            
            public override bool Equals(object obj)
            {
                if (obj is LevelPlayInfoKey && Equals((LevelPlayInfoKey)obj))
                {
                    return true;
                }
    
                return false;
            }
    
            public override int GetHashCode()
            {
                return HashCode.Combine(level, (int)gameMode);
            }
    
            protected bool Equals(LevelPlayInfoKey other)
            {
                return level == other.level && gameMode == other.gameMode;
            }
        }
}