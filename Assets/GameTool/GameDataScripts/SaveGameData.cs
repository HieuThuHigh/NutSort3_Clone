using GameToolSample.GameDataScripts.Scripts;

namespace GameTool.GameDataScripts
{
    public class SaveGameData
    {
        #region LOAD DATA
        public static void LoadAllData()
        {
            GameData.Instance.Data = new DataField();

            GameDataControl.LoadAllData();

            OnAllDataLoaded();
        }
        #endregion

        #region SAVE DATA
        public static void SaveAllData()
        {
            GameDataControl.SaveAllData();
        }

        public static void SaveData<T>(eData filename, T value)
        {
#if !Minify
            var save = new SaveUtility<T>();
            save.SaveData(filename, value);
#endif
        }



        public static void LoadData<T>(eData filename, ref T variable)
        {
#if !Minify
            var save = new SaveUtility<T>();
            save.LoadData(filename, ref variable);
#endif
        }

        public static void OnAllDataLoaded()
        {
            GameData.Instance.OnAllDataLoaded();
        }
        #endregion

        #region CLEAR DATA
        public static void ClearData()
        {
            SaveGameManager.DeleteAllSave();
            GameData.Instance.Data = new DataField();
            SaveAllData();
        }
        #endregion

    }
}
