using System.Linq;
using GameTool.APIs.Analytics.Analytics;
using GameTool.APIs.Scripts;
using GameTool.APIs.Scripts.Ads;
using GameTool.Assistants;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameTool.APIs
{
    public class APIPlayerSetting : ScriptableObject
    {
        public static APIPlayerSetting Instance => Resources.Load<APIPlayerSetting>("APIPlayerSetting");
        [SerializeField] private GameToolSettings _gameToolSetting;

        [Tooltip("Tự động đổi setting dựa trên platform")] [SerializeField]
        private bool _autoChangeSettingBasePlatform = true;

        private void Awake()
        {
            OnValidate();
        }

        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if (!_autoChangeSettingBasePlatform)
            {
                return;
            }
#if UNITY_IOS
            _gameToolSetting = Resources.Load<GameToolSettings>("GameToolSettingsIOS");
#else
            _gameToolSetting = Resources.Load<GameToolSettings>("GameToolSettings");
#endif
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public GameToolSettings GameToolSetting
        {
            get => _gameToolSetting;
            set => _gameToolSetting = value;
        }

        public void SaveDefineSymbols()
        {
            ApplyDefines();
        }

        public void CheckComponent()
        {
            ApplyMediation();
            ApplyTracking();
            API.Instance.InitSettings();
        }

        public void ApplyDefines()
        {
            bool appsflyer = GameToolSetting.mmpType == MMPType.Appsflyer;
            bool singular = GameToolSetting.mmpType == MMPType.Singular;
            bool adjust = GameToolSetting.mmpType == MMPType.Adjust;

            ChangeDefineSymbols("USE_AF", appsflyer);
            ChangeDefineSymbols("USE_SINGULAR", singular);
            ChangeDefineSymbols("USE_ADJUST", adjust);


            bool isAdmob = GameToolSetting.IsUseAnyMediation(MediationType.Admob);
            bool irs = GameToolSetting.IsUseAnyMediation(MediationType.IronSource);
            bool isMax = GameToolSetting.IsUseAnyMediation(MediationType.Applovin);

            ChangeDefineSymbols("USE_ADMOB_ADS", isAdmob);
            ChangeDefineSymbols("USE_IRS_ADS", irs);
            ChangeDefineSymbols("USE_APPLOVIN_ADS", isMax);

            bool admobAppOpen = GameToolSetting.aoaMediation == MediationType.Admob;
            bool useAdmobNativeAds = GameToolSetting.nativeMediation == MediationType.Admob;
            ChangeDefineSymbols("USE_ADMOB_APPOPEN", admobAppOpen);
            ChangeDefineSymbols("USE_ADMOB_NATIVE_ADS", useAdmobNativeAds);


            ChangeDefineSymbols("USE_FIREBASE", GameToolSetting.useFirebase);
            ChangeDefineSymbols("USE_ADVERTY", GameToolSetting.useAdverty);
            ChangeDefineSymbols("USE_GG_REVIVEW", GameToolSetting.useGGReview);
            ChangeDefineSymbols("USE_SPINE", GameToolSetting.useSpine);
            ChangeDefineSymbols("USE_IAP", GameToolSetting.useIap);
            ChangeDefineSymbols("USE_FIREBASE_IAP", GameToolSetting.useFirebaseIAP);
            ChangeDefineSymbols("USE_UNITY_NOTIFICATION", GameToolSetting.useLocalNotification);
        }

        public void ApplyMediation()
        {
            var apibase = API.Instance;
            bool isAdmob = GameToolSetting.IsUseAnyMediation(MediationType.Admob);
            bool irs = GameToolSetting.IsUseAnyMediation(MediationType.IronSource);
            bool isMax = GameToolSetting.IsUseAnyMediation(MediationType.Applovin);

            AddOrRemoveComponent<IronScrManager>(apibase, irs);
            AddOrRemoveComponent<MaxManager>(apibase, isMax);
            AddOrRemoveComponent<AdmobManager>(apibase, isAdmob);

            API.Instance.ListAdsManager = API.Instance.GetComponents<AdsManager>().ToList();

            bool admobAppOpen = GameToolSetting.aoaMediation == MediationType.Admob;
            bool useAdmobNativeAds = GameToolSetting.nativeMediation == MediationType.Admob;
            AddOrRemoveComponent<AppOpenAdManager>(apibase, admobAppOpen);
            AddOrRemoveComponent<NativeAdsAdmod>(apibase, useAdmobNativeAds);

            SetDirty(API.Instance);
        }

        private void ApplyTracking()
        {
            TrackingManager trackingManager = TrackingManager.Instance;
            if (trackingManager)
            {
#if USE_SINGULAR
                if (settings.mmpType == MMPType.Singular)
                {
                    SingularSDK singular = FindObjectOfType<SingularSDK>();

                    if (singular == null)
                    {
                        SingularSDK singularSDK = trackingManager.gameObject.AddComponent<SingularSDK>();
                        singularSDK.SingularAPIKey = settings.singularAPIKey;
                        singularSDK.SingularAPISecret = settings.singularAPISecret;
                        singularSDK.InitializeOnAwake = false;
                    }
                }
                else
                {
                    if(trackingManager.gameObject.GetComponent<SingularSDK>() != null)
                    {
                        DestroyImmediate(trackingManager.gameObject.GetComponent<SingularSDK>());
                    }
                }
#endif
            }
        }

        public void AddOrRemoveComponent<Comp>(MonoBehaviour monoBehaviour, bool isAdd) where Comp : MonoBehaviour
        {
            Comp have = API.Instance.GetComponent<Comp>();
            if (!have && isAdd)
            {
                API.Instance.gameObject.AddComponent<Comp>();
            }
            else if (have && !isAdd)
            {
                DestroyImmediate(have, true);
            }
        }

        public void ChangeDefineSymbols(string defineSymbol, bool isAdd = true)
        {
#if UNITY_EDITOR
            EditorUtils.ChangeDefineSymbols(defineSymbol, isAdd);
#endif
        }

        public void SetDirty(Object objectArg)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(objectArg);
#endif
        }
    }
}