using System;
using System.Collections.Generic;

#if USE_FALCON
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
#endif

using GameTool.Assistants.DesignPattern;
using GameTool.GameDataScripts;
using GameToolSample.GameConfigScripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.UI.ResourcesItems;
using UnityEngine;

namespace GameToolSample.GameDataScripts.Scripts
{
    public class GameData : SingletonMonoBehaviour<GameData>
    {
        [Header("DATA INFO")] public static bool allDataLoaded;
        public DataField Data;
        public GameModeData GameModeData;


        protected override void Awake()
        {
            base.Awake();
            SaveGameData.LoadAllData();
        }

        public void OnAllDataLoaded()
        {
            SetDataFake();

            allDataLoaded = true;

            Debug.Log("All Data Loaded!!!");
        }

        #region FAKE

        public int CoinFake;
        public int DiamondFake;

        public void SetDataFake()
        {
            CoinFake = Coin;
            DiamondFake = Diamond;
        }

        #endregion

        #region MORE DATA

        public bool FirstOpen
        {
            get => Data.FirstOpen;
            set
            {
                Data.FirstOpen = value;
                SaveGameData.SaveData(eData.FirstOpen, Data.FirstOpen);
            }
        }

        public bool FirstPlay
        {
            get => Data.FirstPlay;
            set
            {
                Data.FirstPlay = value;
                SaveGameData.SaveData(eData.FirstPlay, Data.FirstPlay);
            }
        }

        public bool Rated
        {
            get => Data.Rated;
            set
            {
                Data.Rated = value;
                SaveGameData.SaveData(eData.Rated, Data.Rated);
            }
        }

        public bool RemoveAds
        {
            get => Data.RemoveAds;
            set
            {
                Data.RemoveAds = value;
                SaveGameData.SaveData(eData.RemoveAds, Data.RemoveAds);
            }
        }

        public bool GDPR
        {
            get => Data.GDPR;
            set
            {
                Data.GDPR = value;
                SaveGameData.SaveData(eData.GDPR, Data.GDPR);
            }
        }

        public bool GDPRShowed
        {
            get => Data.GDPRShowed;
            set
            {
                Data.GDPRShowed = value;
                SaveGameData.SaveData(eData.GDPRShowed, Data.GDPRShowed);
            }
        }

        #endregion MORE DATA

        #region SETTING

        public bool MuteAll
        {
            get => Data.MuteAll;
            set
            {
                Data.MuteAll = value;
                SaveGameData.SaveData(eData.MuteAll, Data.MuteAll);
            }
        }

        public bool Vibrate
        {
            get => Data.Vibrate;
            set
            {
                Data.Vibrate = value;
                SaveGameData.SaveData(eData.Vibrate, Data.Vibrate);
            }
        }

        public bool Music
        {
            get => Data.Music;
            set
            {
                Data.Music = value;
                SaveGameData.SaveData(eData.Music, Data.Music);
            }
        }

        public bool SoundFX
        {
            get => Data.SoundFX;
            set
            {
                Data.SoundFX = value;
                SaveGameData.SaveData(eData.SoundFX, Data.SoundFX);
            }
        }

        public float MasterVolume
        {
            get => Data.MasterVolume;
            set
            {
                Data.MasterVolume = value;
                SaveGameData.SaveData(eData.MasterVolume, Data.MasterVolume);
            }
        }

        public float MusicVolume
        {
            get => Data.MusicVolume;
            set
            {
                Data.MusicVolume = value;
                SaveGameData.SaveData(eData.MusicVolume, Data.MusicVolume);
            }
        }

        public float SoundFXVolume
        {
            get => Data.SoundFXVolume;
            set
            {
                Data.SoundFXVolume = value;
                SaveGameData.SaveData(eData.SoundFXVolume, Data.SoundFXVolume);
            }
        }

        #endregion SETTING

        #region GAMEPLAY

        public int VictoryCount
        {
            get => Data.VictoryCount;
            set
            {
                Data.VictoryCount = value;
                SaveGameData.SaveData(eData.VictoryCount, Data.VictoryCount);
            }
        }

        public int LoseCount
        {
            get => Data.LoseCount;
            set
            {
                Data.LoseCount = value;
                SaveGameData.SaveData(eData.LoseCount, Data.LoseCount);
            }
        }

        #endregion GAMEPLAY

        #region LEVEL

        public int CurrentLevel
        {
            get => Data.CurrentLevel;
            set
            {
                Data.CurrentLevel = value;

                //if (Data.CurrentLevel >= GameConfig.TotalLevel)
                //{
                //    Data.CurrentLevel = GameConfig.TotalLevel;
                //}

                if (value > LevelUnlocked)
                {
                    LevelUnlocked = value;
                }

                SaveGameData.SaveData(eData.CurrentLevel, Data.CurrentLevel);
            }
        }

        public int LevelUnlocked
        {
            get => Data.LevelUnlocked;
            set
            {
                if (value > GameConfig.Instance.TotalLevel)
                {
                    value = GameConfig.Instance.TotalLevel;
                }

                Data.LevelUnlocked = value;

                SaveGameData.SaveData(eData.LevelUnlocked, Data.LevelUnlocked);
            }
        }

        public List<int> ListLevelUnlockID
        {
            get => Data.ListLevelUnlockID;
        }

        public void UnlockLevel(int id)
        {
            if (!Data.ListLevelUnlockID.Contains(id))
            {
                Data.ListLevelUnlockID.Add(id);
                SaveGameData.SaveData(eData.ListLevelUnlockID, Data.ListLevelUnlockID);
            }
        }

        public void LockLevel(int id)
        {
            if (Data.ListLevelUnlockID.Contains(id))
            {
                Data.ListLevelUnlockID.Remove(id);
                SaveGameData.SaveData(eData.ListLevelUnlockID, Data.ListLevelUnlockID);
            }
        }

        public bool CheckLevelUnlock(int id)
        {
            if (Data.ListLevelUnlockID.Contains(id))
            {
                return true;
            }

            return false;
        }

        public LevelPlayInfoData GetLevelPlayInfoData(LevelPlayInfoKey id)
        {
            if (Data.DictLevelPlayInfoData.TryGetValue(id, out LevelPlayInfoData data))
            {
                return data;
            }

            LevelPlayInfoData dataNew = new LevelPlayInfoData(id);
            Data.DictLevelPlayInfoData.Add(id, dataNew);
            SaveGameData.SaveData(eData.DictLevelPlayInfoData, Data.DictLevelPlayInfoData);
            return dataNew;
        }

        public void SetLevelPlayInfoData(LevelPlayInfoData dataValue)
        {
            if (Data.DictLevelPlayInfoData.ContainsKey(dataValue.levelId))
            {
                Data.DictLevelPlayInfoData[dataValue.levelId] = dataValue;
                SaveGameData.SaveData(eData.DictLevelPlayInfoData, Data.DictLevelPlayInfoData);
                return;
            }

            Data.DictLevelPlayInfoData.Add(dataValue.levelId, dataValue);
            SaveGameData.SaveData(eData.DictLevelPlayInfoData, Data.DictLevelPlayInfoData);
        }

        #endregion LEVEL

        #region CURRENCY
        public void AddCurrency(ItemResourceType itemType, int value)
        {
            switch (itemType)
            {
                case ItemResourceType.Coin:
                    Coin += value;
                    CoinFake += value;
                    break;
                case ItemResourceType.Diamond:
                    Diamond += value;
                    DiamondFake += value;
                    break;
            }
            this.PostEvent(EventID.UpdateData);
        }
        public void AddCurrencySetFake(ItemResourceType itemType, int value, bool setFake = false)
        {
            switch (itemType)
            {
                case ItemResourceType.Coin:
                    Coin += value;
                    if (setFake)
                        CoinFake += value;
                    break;
                case ItemResourceType.Diamond:
                    Diamond += value;
                    if (setFake)
                        DiamondFake += value;
                    break;
            }
            if (setFake)
                this.PostEvent(EventID.UpdateData);
        }

        public void SetCurrency(ItemResourceType itemType, int value)
        {
            switch (itemType)
            {
                case ItemResourceType.Coin:
                    Coin = value;
                    CoinFake = value;
                    break;
                case ItemResourceType.Diamond:
                    Diamond = value;
                    DiamondFake = value;
                    break;
            }

            this.PostEvent(EventID.UpdateData);
        }

        /// <summary>
        /// Không sử dụng biến này để cộng trừ Coin mà dùng hàm AddCurrency ở bên trên
        /// </summary>
        public int Coin
        {
            get => Data.Coin;
            private set
            {
                Data.Coin = value;
                SaveGameData.SaveData(eData.Coin, Data.Coin);
            }
        }

        /// <summary>
        /// Không sử dụng biến này để cộng trừ Diamond mà dùng hàm AddCurrency ở bên trên
        /// </summary>
        public int Diamond
        {
            get => Data.Diamond;
            private set
            {
                Data.Diamond = value;
                SaveGameData.SaveData(eData.Diamond, Data.Diamond);
            }
        }

        #endregion CURRENCY

        #region DAILY REWARD

        public DateTime DayLogin
        {
            get => new DateTime(Data.DayLogin);
            set
            {
                Data.DayLogin = value.Ticks;
                SaveGameData.SaveData(eData.DayLogin, Data.DayLogin);
            }
        }

        public int DayDailyReward
        {
            get => Data.DayDailyReward;
            set
            {
                Data.DayDailyReward = value;
                SaveGameData.SaveData(eData.DayDailyReward, Data.DayDailyReward);
            }
        }

        #endregion DAILY REWARD

        #region SPIN

        public bool CanSpin
        {
            get => Data.CanSpin;
            set
            {
                Data.CanSpin = value;
                SaveGameData.SaveData(eData.CanSpin, Data.CanSpin);
            }
        }

        public float StartAngleSpin
        {
            get => Data.StartAngleSpin;
            set
            {
                Data.StartAngleSpin = value;
                SaveGameData.SaveData(eData.StartAngleSpin, Data.StartAngleSpin);
            }
        }

        public float CountDownTimeSpin
        {
            get => Data.CountDownTimeSpin;
            set
            {
                Data.CountDownTimeSpin = value;
                SaveGameData.SaveData(eData.CountDownTimeSpin, Data.CountDownTimeSpin);
            }
        }

        public List<int> ListIdSkinSpin
        {
            get => Data.ListIdSkinSpin;
            set
            {
                Data.ListIdSkinSpin = value;
                SaveGameData.SaveData(eData.ListIdSkinSpin, Data.ListIdSkinSpin);
            }
        }

        public DateTime TimeSpin
        {
            get => new DateTime(Data.TimeSpin);
            set
            {
                Data.TimeSpin = value.Ticks;
                SaveGameData.SaveData(eData.TimeSpin, Data.TimeSpin);
            }
        }

        public String TimeWatchAdsInShop;
        #endregion SPIN

        #region LANGUAGE

        public int CurrenLanguage
        {
            get => Data.CurrentLanguage;
            set
            {
                Data.CurrentLanguage = value;
                SaveGameData.SaveData(eData.CurrentLanguage, Data.CurrentLanguage);
            }
        }

        #endregion LANGUAGE

        public int TargetStarChest => 1000;

        public void CollectItem(CurrencyInfo currencyInfo, bool setFake,
            AnalyticID.LocationTracking location = AnalyticID.LocationTracking.none)
        {
            CollectItem(currencyInfo, setFake, location.ToString());
        }

        public void CollectItem(CurrencyInfo currencyInfo, bool setFake, string location = "none")
        {
            var amount = currencyInfo.value;

            if (currencyInfo.itemResourceType == ItemResourceType.Coin)
            {
                Coin += amount;
                if (setFake)
                {
                    CoinFake += amount;
                }
            }

            if (currencyInfo.itemResourceType == ItemResourceType.Diamond)
            {
                Diamond += amount;
                if (setFake)
                {
                    DiamondFake += amount;
                }
            }

#if USE_FALCON
            new FResourceLog(FlowType.Source, location, currencyInfo.itemResourceType.ToString(), location, currencyInfo.value).Send();
#endif
        }

        public void SpendItem(CurrencyInfo currencyInfo, bool setFake,
            AnalyticID.LocationTracking location = AnalyticID.LocationTracking.none)
        {
            SpendItem(currencyInfo, setFake, location.ToString());
        }

        public void SpendItem(CurrencyInfo currencyInfo, bool setFake, string location = "none")
        {
            var amount = -currencyInfo.value;

            if (currencyInfo.itemResourceType == ItemResourceType.Coin)
            {
                Coin += amount;
                if (setFake)
                {
                    CoinFake += amount;
                }
            }

            if (currencyInfo.itemResourceType == ItemResourceType.Diamond)
            {
                Diamond += amount;
                if (setFake)
                {
                    DiamondFake += amount;
                }
            }

#if USE_FALCON
            new FResourceLog(FlowType.Sink, location, currencyInfo.itemResourceType.ToString(), location, currencyInfo.value).Send();
#endif
        }
// AUTO GENERATE
    }

    [Serializable]
    public class GameModeData
    {
        public AnalyticID.GameModeName GameMode;
        public int Level;
    }
}