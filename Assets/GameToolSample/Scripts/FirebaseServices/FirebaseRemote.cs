using System;
using System.Collections.Generic;
using GameTool.Assistants.DesignPattern;
using GameTool.Assistants.Helper;
using UnityEngine;
#if USE_FIREBASE
using GameTool.FirebaseService;
#endif

using System.Threading.Tasks;
using System.Collections;
using GameTool.APIs.Scripts;


#if USE_FIREBASE
using Firebase.RemoteConfig;
using GameTool.APIs.Scripts;
#endif

namespace GameToolSample.Scripts.FirebaseServices
{
    public class FirebaseRemote : SingletonMonoBehaviour<FirebaseRemote>
    {
        public RemoteRow[] remoteRows;
        public string[] singleKeys;

        [HideInInspector] public List<object> commonClass = new List<object>();

        public static Action OnSetDataCompleted;
        public static Action<Dictionary<string, object>> OnSetDefault;
        [HideInInspector] public bool IsFirebaseFetchCompleted;
        public static bool IsFirebaseGetDataCompleted;

        //CreateClassContructorStart 
 ApiInfor apiInfor;  
 [SerializeField] 
 ApiInfor apiInforDF;  
 StoreConfig storeConfig;  
 [SerializeField] 
 StoreConfig storeConfigDF;  
 GameRemote gameRemote;  
 [SerializeField] 
 GameRemote gameRemoteDF;  
 SpinConfig spinConfig;  
 [SerializeField] 
 SpinConfig spinConfigDF;  
 DailyRewardConfig dailyRewardConfig;  
 [SerializeField] 
 DailyRewardConfig dailyRewardConfigDF;  
 SkinConfig skinConfig;  
 [SerializeField] 
 SkinConfig skinConfigDF;  
 IAPConfig iAPConfig;  
 [SerializeField] 
 IAPConfig iAPConfigDF;  
 IAPId iAPid;  
 [SerializeField] 
 IAPId iAPidDF; 
 
 //CreateClassContructorEnd 

        protected override void Awake()
        {
            base.Awake();
#if USE_FIREBASE
            InitDefault();
            FirebaseInstance.CheckAndTryInit(Init);
            StartCoroutine(WaitFetchCompleted());
#endif
        }
#if USE_FIREBASE
        void Init()
        {
            FetchDataAsync();
        }

        public Task FetchDataAsync()
        {
            System.Threading.Tasks.Task fetchTask =
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                    TimeSpan.Zero);
            return fetchTask.ContinueWith(FetchComplete);
        }

        void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                ApiDebug("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                ApiDebug("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                ApiDebug("Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;

            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                    ApiDebug(string.Format("Remote data loaded and ready (last fetch time {0}).", info.FetchTime));
                    break;
                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            ApiDebug("Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            ApiDebug("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }

                    break;
                case LastFetchStatus.Pending:
                    ApiDebug("Latest Fetch call still pending.");
                    break;
            }

            IsFirebaseFetchCompleted = true;
        }

        private IEnumerator WaitFetchCompleted()
        {
            yield return new WaitUntil(() => IsFirebaseFetchCompleted);
            SetupDefaultConfig();
        }

        //adddefaultstart 
void SetupDefaultConfig() { 
 Dictionary<string, object> defaults = new Dictionary<string, object>(); 
 defaults.Add("ApiInfor", JsonUtility.ToJson(apiInforDF ));  
 defaults.Add("StoreConfig", JsonUtility.ToJson(storeConfigDF ));  
 defaults.Add("GameRemote", JsonUtility.ToJson(gameRemoteDF ));  
 defaults.Add("SpinConfig", JsonUtility.ToJson(spinConfigDF ));  
 defaults.Add("DailyRewardConfig", JsonUtility.ToJson(dailyRewardConfigDF ));  
 defaults.Add("SkinConfig", JsonUtility.ToJson(skinConfigDF ));  
 defaults.Add("IAPConfig", JsonUtility.ToJson(iAPConfigDF ));  
 defaults.Add("IAPId", JsonUtility.ToJson(iAPidDF ));  
 if (OnSetDefault != null) 
 OnSetDefault(defaults); 
 Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWith(task => GetRemoteData());
 } 
 
 //adddefaultend

        //getremotedatastart 
void GetRemoteData()
 {
 try 
{ 
  string keyApiInfor= "ApiInfor"; 
  string dataApiInfor = FirebaseRemoteConfig.DefaultInstance.GetValue(keyApiInfor).StringValue; 
dataApiInfor= dataApiInfor.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keyApiInfor, dataApiInfor)); 
apiInfor = JsonUtility.FromJson<ApiInfor>(dataApiInfor);
} 
 catch { } 

 try 
{ 
  string keyStoreConfig= "StoreConfig"; 
  string dataStoreConfig = FirebaseRemoteConfig.DefaultInstance.GetValue(keyStoreConfig).StringValue; 
dataStoreConfig= dataStoreConfig.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keyStoreConfig, dataStoreConfig)); 
storeConfig = JsonUtility.FromJson<StoreConfig>(dataStoreConfig);
} 
 catch { } 

 try 
{ 
  string keyGameRemote= "GameRemote"; 
  string dataGameRemote = FirebaseRemoteConfig.DefaultInstance.GetValue(keyGameRemote).StringValue; 
dataGameRemote= dataGameRemote.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keyGameRemote, dataGameRemote)); 
gameRemote = JsonUtility.FromJson<GameRemote>(dataGameRemote);
} 
 catch { } 

 try 
{ 
  string keySpinConfig= "SpinConfig"; 
  string dataSpinConfig = FirebaseRemoteConfig.DefaultInstance.GetValue(keySpinConfig).StringValue; 
dataSpinConfig= dataSpinConfig.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keySpinConfig, dataSpinConfig)); 
spinConfig = JsonUtility.FromJson<SpinConfig>(dataSpinConfig);
} 
 catch { } 

 try 
{ 
  string keyDailyRewardConfig= "DailyRewardConfig"; 
  string dataDailyRewardConfig = FirebaseRemoteConfig.DefaultInstance.GetValue(keyDailyRewardConfig).StringValue; 
dataDailyRewardConfig= dataDailyRewardConfig.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keyDailyRewardConfig, dataDailyRewardConfig)); 
dailyRewardConfig = JsonUtility.FromJson<DailyRewardConfig>(dataDailyRewardConfig);
} 
 catch { } 

 try 
{ 
  string keySkinConfig= "SkinConfig"; 
  string dataSkinConfig = FirebaseRemoteConfig.DefaultInstance.GetValue(keySkinConfig).StringValue; 
dataSkinConfig= dataSkinConfig.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keySkinConfig, dataSkinConfig)); 
skinConfig = JsonUtility.FromJson<SkinConfig>(dataSkinConfig);
} 
 catch { } 

 try 
{ 
  string keyIAPConfig= "IAPConfig"; 
  string dataIAPConfig = FirebaseRemoteConfig.DefaultInstance.GetValue(keyIAPConfig).StringValue; 
dataIAPConfig= dataIAPConfig.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keyIAPConfig, dataIAPConfig)); 
iAPConfig = JsonUtility.FromJson<IAPConfig>(dataIAPConfig);
} 
 catch { } 

 try 
{ 
  string keyIAPId= "IAPId"; 
  string dataIAPId = FirebaseRemoteConfig.DefaultInstance.GetValue(keyIAPId).StringValue; 
dataIAPId= dataIAPId.CorrectString(); 
 ApiDebug(string.Format(" FirebaseRemote - key {0}:{1}", keyIAPId, dataIAPId)); 
iAPid = JsonUtility.FromJson<IAPId>(dataIAPId);
} 
 catch { } 
IsFirebaseGetDataCompleted = true;}
 
 //getremotedataend
#endif
        public void ApiDebug(string content)
        {
            Debug.LogFormat("Tag GameTool log {0}", content);
        }

        #region Utils

        public string CopyDefaultValue(int index)
        {
            return JsonUtility.ToJson(commonClass[index]);
        }

        //initdefaultstart 
void InitDefault() { 
  
apiInfor=apiInforDF ; 
storeConfig=storeConfigDF ; 
gameRemote=gameRemoteDF ; 
spinConfig=spinConfigDF ; 
dailyRewardConfig=dailyRewardConfigDF ; 
skinConfig=skinConfigDF ; 
iAPConfig=iAPConfigDF ; 
iAPid=iAPidDF ; 
} 
 public ApiInfor GetApiInfor() 
 {
 try{
 return apiInfor; 
 } 
 catch{ 
 return apiInforDF ; 
 } }
 public StoreConfig GetStoreConfig() 
 {
 try{
 return storeConfig; 
 } 
 catch{ 
 return storeConfigDF ; 
 } }
 public GameRemote GetGameRemote() 
 {
 try{
 return gameRemote; 
 } 
 catch{ 
 return gameRemoteDF ; 
 } }
 public SpinConfig GetSpinConfig() 
 {
 try{
 return spinConfig; 
 } 
 catch{ 
 return spinConfigDF ; 
 } }
 public DailyRewardConfig GetDailyRewardConfig() 
 {
 try{
 return dailyRewardConfig; 
 } 
 catch{ 
 return dailyRewardConfigDF ; 
 } }
 public SkinConfig GetSkinConfig() 
 {
 try{
 return skinConfig; 
 } 
 catch{ 
 return skinConfigDF ; 
 } }
 public IAPConfig GetIAPConfig() 
 {
 try{
 return iAPConfig; 
 } 
 catch{ 
 return iAPConfigDF ; 
 } }
 public IAPId GetIAPId() 
 {
 try{
 return iAPid; 
 } 
 catch{ 
 return iAPidDF ; 
 } }
 
 //initdefaultend 

        //updateonvalidatestart 
private void OnValidate() 
 { 
 commonClass.Clear(); 
commonClass.Add(apiInforDF);
commonClass.Add(storeConfigDF);
commonClass.Add(gameRemoteDF);
commonClass.Add(spinConfigDF);
commonClass.Add(dailyRewardConfigDF);
commonClass.Add(skinConfigDF);
commonClass.Add(iAPConfigDF);
commonClass.Add(iAPidDF);
}

 
 //updateonvalidateend


        string CreateClassString()
        {
            string totalvalue = "";

            for (int t = 0; t < remoteRows.Length; t++)
            {
                if (!remoteRows[t].customClass)
                {
                    string value = "[Serializable] \n  public class " + remoteRows[t].className.ToString() + " \n {";
                    for (int i = 0; i < remoteRows[t].param.Length; i++)
                    {
                        value += String.Format("\n public {0} {1};", remoteRows[t].param[i].GetvariableType(),
                            remoteRows[t].param[i].paramName);
                    }

                    totalvalue += "\n" + value + "\n }";
                }
            }

            //for (int t = 0; t < singleKeys.Length; t++)
            //{
            //        string value = "[Serializable] \n  public string " + singleKeys[t] + " \n {";
            //        for (int i = 0; i < remoteRows[t].param.Length; i++)
            //        {
            //            value += String.Format("\n public {0} {1};", remoteRows[t].param[i].GetvariableType(), remoteRows[t].param[i].paramName);
            //        }
            //        totalvalue += "\n" + value + "\n }";

            //}
            return totalvalue;
        }

        string CreateSetDefaultString()
        {
            string totalvalue =
                "void SetupDefaultConfig() { \n Dictionary<string, object> defaults = new Dictionary<string, object>();";

            for (int t = 0; t < remoteRows.Length; t++)
            {
                string className = remoteRows[t].className.ToString();
                char[] firstChar = className[0].ToString().ToLower().ToCharArray();
                string finalName = className.Replace(className[0], firstChar[0]) + "DF ";
                string value = " \n defaults.Add(\"" +
                               (remoteRows[t].remoteKey != ""
                                   ? remoteRows[t].remoteKey
                                   : remoteRows[t].className.ToString()) + "\", JsonUtility.ToJson(" + finalName +
                               ")); ";
                totalvalue += value;
            }

            // defaults.Add("interAds", interAdsDF); 
            for (int t = 0; t < singleKeys.Length; t++)
            {
                string value = " \n defaults.Add(\"" + singleKeys[t] + "\"," + singleKeys[t] + "); ";
                totalvalue += value;
            }

            totalvalue +=
                " \n if (OnSetDefault != null) \n OnSetDefault(defaults); \n Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWith(task => GetRemoteData());\n } ";
            return totalvalue;
        }

        string CreateInitDefaultString()
        {
            string totalvalue = "void InitDefault() { \n ";

            for (int t = 0; t < remoteRows.Length; t++)
            {
                string className = remoteRows[t].className.ToString();
                char[] firstChar = className[0].ToString().ToLower().ToCharArray();
                string finalName = className.Replace(className[0], firstChar[0]);
                string finalName2 = className.Replace(className[0], firstChar[0]) + "DF ";
                string value = " \n" + finalName + "=" + finalName2 + ";";
                totalvalue += value;
            }

            totalvalue += " \n} ";

            for (int t = 0; t < remoteRows.Length; t++)
            {
                string className = remoteRows[t].className.ToString();

                char[] firstChar = className[0].ToString().ToLower().ToCharArray();
                string finalName = className.Replace(className[0], firstChar[0]);
                string finalName2 = className.Replace(className[0], firstChar[0]) + "DF ";

                string value = "\n public " + className + " Get" + className + "() \n {\n try{\n return " + finalName +
                               "; \n } \n catch{ \n return " + finalName2 + "; \n } }";

                totalvalue += value;
            }

            return totalvalue;
        }

        string CreatePubicString()
        {
            string totalvalue = "";
            for (int t = 0; t < remoteRows.Length; t++)
            {
                string className = remoteRows[t].className.ToString();
                char[] firstChar = className[0].ToString().ToLower().ToCharArray();
                string finalName = className.Replace(className[0], firstChar[0]);
                totalvalue += " \n " + className + " " + finalName + "; ";
                totalvalue += " \n [SerializeField] \n " + className + " " + finalName + "DF; ";
            }

            for (int t = 0; t < singleKeys.Length; t++)
            {
                totalvalue += " \n  public string " + singleKeys[t] + ";";
            }

            return totalvalue;
        }

        string GetRemoteDataPair(string key, string className, string publicName)
        {
            string t = "";
            t = " try \n" +
                "{ \n " +
                " string key" + key + "= \"" + key + "\"; \n " +
                " string data" + key + " = FirebaseRemoteConfig.DefaultInstance.GetValue(key" + key +
                ").StringValue; \n" +
                "data" + key + "= data" + key + ".CorrectString(); \n" +
                " ApiDebug(string.Format(\" FirebaseRemote - key {0}:{1}\", key" + key + ", data" + key + ")); \n" +
                "" + publicName + " = JsonUtility.FromJson<" + className + ">(data" + key + ");\n" +
                "} \n" +
                " catch { } \n";

            return t;
        }

        string GetRemoteDataSinglePair(string key)
        {
            string t = "";
            t = " try \n" +
                "{" + key + "= FirebaseRemoteConfig.DefaultInstance.GetValue(\"" + key + "\").StringValue; \n" +
                " ApiDebug(string.Format(\" FirebaseRemote - key " + key + " :{0}\", " + key + "));\n}" +
                " \ncatch { } \n";

            return t;
        }

        string CreateGetDataString()
        {
            string totalvalue = "void GetRemoteData()\n {";
            for (int t = 0; t < remoteRows.Length; t++)
            {
                string className = remoteRows[t].className.ToString();
                char[] firstChar = className[0].ToString().ToLower().ToCharArray();
                string finalName = className.Replace(className[0], firstChar[0]);

                totalvalue += "\n" +
                              GetRemoteDataPair(
                                  (remoteRows[t].remoteKey != ""
                                      ? remoteRows[t].remoteKey
                                      : remoteRows[t].className.ToString()), remoteRows[t].className, finalName);
            }

            for (int t = 0; t < singleKeys.Length; t++)
            {
                totalvalue += GetRemoteDataSinglePair(singleKeys[t]);
            }

            return totalvalue += "IsFirebaseGetDataCompleted = true;}";
        }

        string UpdateCommonClass()
        {
            string totalvalue = "private void OnValidate() \n { \n commonClass.Clear(); \n";
            for (int t = 0; t < remoteRows.Length; t++)
            {
                string className = remoteRows[t].className.ToString();
                char[] firstChar = className[0].ToString().ToLower().ToCharArray();
                string finalName = className.Replace(className[0], firstChar[0]);
                totalvalue += "commonClass.Add(" + finalName + "DF);\n";
            }

            return totalvalue += "}\n";
        }

        public void ReadandWriteFile()
        {
            string csFilePath = Application.dataPath + "/GameToolSample/Scripts/FirebaseServices/FirebaseRemote.cs";
            FileChanger.WriteFile(csFilePath, "\n " + CreateClassString(), CreatePubicString(),
                CreateSetDefaultString(), CreateGetDataString(), UpdateCommonClass(), CreateInitDefaultString());
        }

        public void GetKey(string key)
        {
        }

        #endregion
    }

    public static class APIExtention
    {
        public static string CorrectString(this string input)
        {
            if (String.IsNullOrEmpty(input))
                return null;
            return input.Replace("\r", string.Empty);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    [Serializable]
    public class RemoteParam
    {
        public VariableType variableType;
        public string paramName;

        public string GetvariableType()
        {
            string type = "";

            switch (variableType)
            {
                case VariableType.STRING:
                    type = "string";
                    break;
                case VariableType.INT:
                    type = "int";
                    break;
                case VariableType.BOOL:
                    type = "bool";
                    break;
                case VariableType.FLOAT:
                    type = "float";
                    break;
                case VariableType.FLOAT_ARRAY:
                    type = "float[]";
                    break;
                case VariableType.INT_ARRAY:
                    type = "int[]";
                    break;
                case VariableType.STRING_ARRAY:
                    type = "string[]";
                    break;
            }

            return type;
        }
    }

    public enum VariableType
    {
        STRING,
        INT,
        FLOAT,
        BOOL,
        STRING_ARRAY,
        INT_ARRAY,
        FLOAT_ARRAY
    }

    [Serializable]
    public class RemoteRow
    {
        public string className;
        public string remoteKey;
        public bool customClass;
        public RemoteParam[] param;
    }

    //CreateClassStart 
 
[Serializable] 
  public class ApiInfor 
 {
 public bool IsEnableDebug;
 public bool UseGDPR;
 public float InterstitialInterval;
 public int AdmobInterstitialInterval;
 public float TimeLoading;
 public bool AppOpenAfterAds;
 public int BannerSizeId;
 public bool open_resume;
 public bool show_inter_on_aoa_fail;
 }
[Serializable] 
  public class StoreConfig 
 {
 public string GooglePlayLinkGame;
 public string AppStoreLinkGame;
 public bool ForceUpdate;
 public string[] ListVersion;
 }
[Serializable] 
  public class GameRemote 
 {
 public bool NoInternetBlock;
 public bool useAdsIngame;
 public bool ShowMoregameVictory;
 public bool useAppopenFloor;
 public int BonusDiamondRewardAds;
 }
[Serializable] 
  public class SpinConfig 
 {
 public int TimeResetSpin;
 public float[] ListPercentageSpin;
 public int[] ListTypeSpinReward;
 public int[] ListTypeSpin;
 public int[] ListValueSpin;
 }
[Serializable] 
  public class DailyRewardConfig 
 {
 public int dailyCoinDay1;
 public int dailyCoinDay2;
 public int dailyCoinDay3;
 public int dailyCoinDay4;
 public int dailyCoinDay5;
 public int dailyCoinDay6;
 public int dailyCoinDay7;
 }
[Serializable] 
  public class SkinConfig 
 {
 public int[] ListTotalSkin;
 public int[] Cost;
 public int[] TypeSkinResource;
 }
[Serializable] 
  public class IAPConfig 
 {
 public int CoinPackAmount;
 public int DiamondPackAmount;
 public int CoinAdsAmount;
 public int DiamondAdsAmount;
 }
[Serializable] 
  public class IAPId 
 {
 public string RemoveAds;
 public string SpecialOffer;
 public string LegendaryBundle;
 public string Coin1;
 public string Coin2;
 public string Coin3;
 public string Coin4;
 public string Coin5;
 public string PiggyBank;
 public string SpecialDeal;
 }
 
 //CreateClassEnd 
}




