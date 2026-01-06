using GameTool.GameDataScripts;

namespace GameToolSample.GameDataScripts.Scripts
{
    public static class GameDataControl
    {
        public static void SaveAllData()
        {
            SaveGameData.SaveData(eData.FirstOpen, GameData.Instance.Data.FirstOpen);
            SaveGameData.SaveData(eData.FirstPlay, GameData.Instance.Data.FirstPlay);
            SaveGameData.SaveData(eData.Rated, GameData.Instance.Data.Rated);
            SaveGameData.SaveData(eData.RemoveAds, GameData.Instance.Data.RemoveAds);
            SaveGameData.SaveData(eData.GDPR, GameData.Instance.Data.GDPR);
            SaveGameData.SaveData(eData.GDPRShowed, GameData.Instance.Data.GDPRShowed);
            SaveGameData.SaveData(eData.MuteAll, GameData.Instance.Data.MuteAll);
            SaveGameData.SaveData(eData.Music, GameData.Instance.Data.Music);
            SaveGameData.SaveData(eData.SoundFX, GameData.Instance.Data.SoundFX);
            SaveGameData.SaveData(eData.Vibrate, GameData.Instance.Data.Vibrate);
            SaveGameData.SaveData(eData.MasterVolume, GameData.Instance.Data.MasterVolume);
            SaveGameData.SaveData(eData.MusicVolume, GameData.Instance.Data.MusicVolume);
            SaveGameData.SaveData(eData.SoundFXVolume, GameData.Instance.Data.SoundFXVolume);
            SaveGameData.SaveData(eData.Coin, GameData.Instance.Data.Coin);
            SaveGameData.SaveData(eData.Diamond, GameData.Instance.Data.Diamond);
            SaveGameData.SaveData(eData.VictoryCount, GameData.Instance.Data.VictoryCount);
            SaveGameData.SaveData(eData.LoseCount, GameData.Instance.Data.LoseCount);
            SaveGameData.SaveData(eData.CurrentLevel, GameData.Instance.Data.CurrentLevel);
            SaveGameData.SaveData(eData.LevelUnlocked, GameData.Instance.Data.LevelUnlocked);
            SaveGameData.SaveData(eData.ListLevelUnlockID, GameData.Instance.Data.ListLevelUnlockID);
            SaveGameData.SaveData(eData.DictLevelPlayInfoData, GameData.Instance.Data.DictLevelPlayInfoData);
            SaveGameData.SaveData(eData.DayLogin, GameData.Instance.Data.DayLogin);
            SaveGameData.SaveData(eData.DayDailyReward, GameData.Instance.Data.DayDailyReward);
            SaveGameData.SaveData(eData.StartAngleSpin, GameData.Instance.Data.StartAngleSpin);
            SaveGameData.SaveData(eData.CanSpin, GameData.Instance.Data.CanSpin);
            SaveGameData.SaveData(eData.TimeSpin, GameData.Instance.Data.TimeSpin);
            SaveGameData.SaveData(eData.CountDownTimeSpin, GameData.Instance.Data.CountDownTimeSpin);
            SaveGameData.SaveData(eData.ListIdSkinSpin, GameData.Instance.Data.ListIdSkinSpin);
            SaveGameData.SaveData(eData.CurrentLanguage, GameData.Instance.Data.CurrentLanguage);
            SaveGameData.SaveData(eData.SelectedShopBgId, GameData.Instance.Data.SelectedShopBgId);
            SaveGameData.SaveData(eData.SelectedShopRingId, GameData.Instance.Data.SelectedShopRingId);
            SaveGameData.SaveData(eData.ListShopBgOwned, GameData.Instance.Data.ListShopBgOwned);
            SaveGameData.SaveData(eData.ItemShopStates, GameData.Instance.Data.ItemShopStates);
        }

        public static void LoadAllData()
        {
            SaveGameData.LoadData(eData.FirstOpen, ref GameData.Instance.Data.FirstOpen);
            SaveGameData.LoadData(eData.FirstPlay, ref GameData.Instance.Data.FirstPlay);
            SaveGameData.LoadData(eData.Rated, ref GameData.Instance.Data.Rated);
            SaveGameData.LoadData(eData.RemoveAds, ref GameData.Instance.Data.RemoveAds);
            SaveGameData.LoadData(eData.GDPR, ref GameData.Instance.Data.GDPR);
            SaveGameData.LoadData(eData.GDPRShowed, ref GameData.Instance.Data.GDPRShowed);
            SaveGameData.LoadData(eData.MuteAll, ref GameData.Instance.Data.MuteAll);
            SaveGameData.LoadData(eData.Music, ref GameData.Instance.Data.Music);
            SaveGameData.LoadData(eData.SoundFX, ref GameData.Instance.Data.SoundFX);
            SaveGameData.LoadData(eData.Vibrate, ref GameData.Instance.Data.Vibrate);
            SaveGameData.LoadData(eData.MasterVolume, ref GameData.Instance.Data.MasterVolume);
            SaveGameData.LoadData(eData.MusicVolume, ref GameData.Instance.Data.MusicVolume);
            SaveGameData.LoadData(eData.SoundFXVolume, ref GameData.Instance.Data.SoundFXVolume);
            SaveGameData.LoadData(eData.Coin, ref GameData.Instance.Data.Coin);
            SaveGameData.LoadData(eData.Diamond, ref GameData.Instance.Data.Diamond);
            SaveGameData.LoadData(eData.VictoryCount, ref GameData.Instance.Data.VictoryCount);
            SaveGameData.LoadData(eData.LoseCount, ref GameData.Instance.Data.LoseCount);
            SaveGameData.LoadData(eData.CurrentLevel, ref GameData.Instance.Data.CurrentLevel);
            SaveGameData.LoadData(eData.LevelUnlocked, ref GameData.Instance.Data.LevelUnlocked);
            SaveGameData.LoadData(eData.ListLevelUnlockID, ref GameData.Instance.Data.ListLevelUnlockID);
            SaveGameData.LoadData(eData.DictLevelPlayInfoData, ref GameData.Instance.Data.DictLevelPlayInfoData);
            SaveGameData.LoadData(eData.DayLogin, ref GameData.Instance.Data.DayLogin);
            SaveGameData.LoadData(eData.DayDailyReward, ref GameData.Instance.Data.DayDailyReward);
            SaveGameData.LoadData(eData.StartAngleSpin, ref GameData.Instance.Data.StartAngleSpin);
            SaveGameData.LoadData(eData.CanSpin, ref GameData.Instance.Data.CanSpin);
            SaveGameData.LoadData(eData.TimeSpin, ref GameData.Instance.Data.TimeSpin);
            SaveGameData.LoadData(eData.CountDownTimeSpin, ref GameData.Instance.Data.CountDownTimeSpin);
            SaveGameData.LoadData(eData.ListIdSkinSpin, ref GameData.Instance.Data.ListIdSkinSpin);
            SaveGameData.LoadData(eData.CurrentLanguage, ref GameData.Instance.Data.CurrentLanguage);
            SaveGameData.LoadData(eData.SelectedShopBgId, ref GameData.Instance.Data.SelectedShopBgId);
            SaveGameData.LoadData(eData.SelectedShopRingId, ref GameData.Instance.Data.SelectedShopRingId);
            SaveGameData.LoadData(eData.ListShopBgOwned, ref GameData.Instance.Data.ListShopBgOwned);
            SaveGameData.LoadData(eData.ItemShopStates, ref GameData.Instance.Data.ItemShopStates);
            
        }
    }
}
