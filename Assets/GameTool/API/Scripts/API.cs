using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameTool.APIs.Analytics.Analytics;
using GameTool.APIs.Scripts.Ads;
using GameTool.Assistants.DesignPattern;
using GameToolSample.APIs;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.FirebaseServices;
using UnityEngine;
using UnityEngine.UI;
using static GameToolSample.Scripts.Enum.AnalyticID;

#if USE_FIREBASE
using Firebase.Messaging;
using GameToolSample.Scripts.FirebaseServices;
#endif

#if USE_GG_REVIVEW && UNITY_ANDROID
using Google.Play.Review;
#endif

namespace GameTool.APIs.Scripts
{
    public class API : SingletonMonoBehaviour<API>
    {
        protected bool _isFirstOpenTime;
        protected bool _apiStarted;
        protected bool _adsConfigLoaded;
        protected bool _showToast;

        [SerializeField] protected bool _isRemoveAdsFull;
        [SerializeField] protected bool _isNotNetwork;
        [SerializeField] protected bool _isTestBackfill;
        [SerializeField] protected bool _isForceDebug = true;
        [SerializeField] protected bool _isDoneUmp = true;

        [SerializeField] protected GameObject loadingObj;
        [SerializeField] protected Text loadingInfoText;
        [SerializeField] protected int fakeAppOpenWaitingTime = 1;
        [SerializeField] protected int fakeInterWaitingTime = 1;
        [SerializeField] protected int fakeRewardWaitingTime = 1;
        [SerializeField] protected APIPlayerSetting _apiPlayerSetting;

        protected GameObject fullScreenAdsFake;
        protected GameObject bannerAdsFake;
        protected Action<bool> _onRewardSuccess;
        [SerializeField] protected List<AdsManager> _listAdsManager;


#if USE_FIREBASE
        protected ApiInfor apiInfo;

        public ApiInfor APIInfo => apiInfo;
#endif

        protected bool _finishSetData;

        public bool IsDebug
        {
            get => _isForceDebug || FirebaseRemote.Instance.GetApiInfor().IsEnableDebug;
            set => _isForceDebug = value;
        }

        public bool IsNotNetwork
        {
            get => _isNotNetwork;
            set => _isNotNetwork = value;
        }

        public bool IsRemoveAdsFull
        {
            get => _isRemoveAdsFull;
            set => _isRemoveAdsFull = value;
        }

        public bool IsTestBackfill
        {
            get => _isTestBackfill;
            set => _isTestBackfill = value;
        }

        public virtual bool IsRemoveAds
        {
            get => GameData.Instance.RemoveAds || IsRemoveAdsFull || APIHandle.IsRemoveAds;
            set => GameData.Instance.RemoveAds = value;
        }

        public virtual bool FirstOpen
        {
            get => GameData.Instance.FirstOpen;
            set => GameData.Instance.FirstOpen = value;
        }

        public bool IsFirstOpenTime => _isFirstOpenTime;

        public bool AdsConfigLoaded => _adsConfigLoaded;

        public virtual bool CanShowInter => _adsConfigLoaded && !IsRemoveAds && APIHandle.CanShowInter;

        public virtual bool CanShowBanner => _adsConfigLoaded && !IsRemoveAds && APIHandle.CanShowBanner;

        public virtual bool CanShowNative => _adsConfigLoaded && !IsRemoveAds && APIHandle.CanShowNative;

        public virtual bool CanShowMrec => _adsConfigLoaded && !IsRemoveAds && APIHandle.CanShowMrec;

        public virtual GameToolSettings GameToolSetting => APIPlayerSetting.GameToolSetting;

        public virtual bool APIStarted => _apiStarted;

        public bool ShowToast
        {
            get => _showToast;
            set => _showToast = value;
        }

        public bool IsDoneUmp
        {
            get => _isDoneUmp;
            set => _isDoneUmp = value;
        }


        public List<AdsManager> ListAdsManager
        {
            get => _listAdsManager;
            set => _listAdsManager = value;
        }

        public virtual APIPlayerSetting APIPlayerSetting
        {
            get
            {
                if (!_apiPlayerSetting)
                {
                    Resources.Load<APIPlayerSetting>("APIPlayerSetting");
                }

                return _apiPlayerSetting;
            }
        }

        public bool IsBannerShowing => _listAdsManager.Any(manager => manager.IsShowingBanner);

        protected override void Awake()
        {
            // Lấy các settings của API
            base.Awake();
            InitSettings();
        }

        protected virtual void Start()
        {
            Application.targetFrameRate = 60;
            ActiveLoading(false);

#if USE_GAMETOOL_UMP
            UMPManager.Instance.InitUMP(StartAPI);
#else
            StartAPI();
#endif
        }

        private void StartAPI()
        {
            StartCoroutine(nameof(CheckAdsInit));
            StartCoroutine(nameof(WaitDataStart));
#if USE_FIREBASE
            StartCoroutine(nameof(WaitForFirebaseGotData));
#endif
        }

        public virtual void InitSettings()
        {
            _apiPlayerSetting = Resources.Load<APIPlayerSetting>("APIPlayerSetting");
        }

        //Load Ads + Load Config Ads từ Firebase
        IEnumerator CheckAdsInit()
        {
            yield return new WaitUntil(() => _isDoneUmp);

            InitAds();
            yield return new WaitUntil(() => _apiStarted);

#if USE_FIREBASE
            yield return new WaitUntil(() => FirebaseRemote.IsFirebaseGetDataCompleted);

            apiInfo = FirebaseRemote.Instance.GetApiInfor();
#endif
            _adsConfigLoaded = true;
        }


        public void InitAds()
        {
            if (!_finishSetData)
            {
                foreach (AdsManager adsManager in _listAdsManager)
                {
                    adsManager.Init();
                }

                _finishSetData = true;
            }
        }


        //Load các Data đã lưu trong máy, set các biến check cần thiết cho các script khác trong game cần dùng
        IEnumerator WaitDataStart()
        {
            yield return new WaitUntil(() => GameData.allDataLoaded);

            if (!FirstOpen)
            {
                FirstOpen = true;
                _isFirstOpenTime = true;
            }

            _apiStarted = true;
        }

#if USE_FIREBASE
        //Action bắt buộc để nhận noti của FirebaseMessaging
        public event Action<TokenReceivedEventArgs> EventOnTokenReceived;
        public event Action<MessageReceivedEventArgs> EventOnMessageReceived;
        private string _firebaseMessagingToken;

        public string FirebaseMessagingToken
        {
            get => _firebaseMessagingToken;
            protected set => _firebaseMessagingToken = value;
        }

        IEnumerator WaitForFirebaseGotData()
        {
            yield return new WaitUntil(() => FirebaseRemote.IsFirebaseGetDataCompleted);
            InitFirebaseConfig();
        }


        void InitFirebaseConfig()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = IsDebug;
#endif
        }

        void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
#if UNITY_ANDROID && USE_AF
        AppsFlyerSDK.AppsFlyer.updateServerUninstallToken(token.Token);
#endif
            FirebaseMessagingToken = token.Token;
            if (EventOnTokenReceived != null) EventOnTokenReceived(token);
        }

        void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (EventOnMessageReceived != null) EventOnMessageReceived(e);
        }
#endif

        #region ADS

        public void ShowMrec()
        {
            if (!CanShowMrec)
            {
                return;
            }

            var manager = _listAdsManager.Find(manager => manager.MediationType == GameToolSetting.mrecMediation);

            manager.ShowMrec(LocationTracking.none);
        }

        public void HideMrec()
        {
            var manager = _listAdsManager.Find(manager => manager.MediationType == GameToolSetting.mrecMediation);

            manager.HideMrec();
        }

        public void ShowBanner(KeyAds key = KeyAds.none, BannerPosition position = BannerPosition.Bottom,
            LocationTracking location = LocationTracking.none,
            Dictionary<string, object> addingTracking = null, bool normalBanner = true,
            MediationType mediation = MediationType.None)
        {
            if (IsEditor())
            {
                if (CanShowBanner)
                {
                    if (normalBanner)
                    {
                        Debug.Log("Show Banner: " + position);
                    }
                    else
                    {
                        Debug.Log("Show Collapsible Banner: " + position);
                    }

                    ShowAdsFake(AdsType.banner, _ => { });
                    // Event này đã gọi ở CallBack trong ads, trên edtor không có nên gọi ở đây để test cho đúng
                    this.PostEvent(EventID.BannerShowing, new object[] { true });
                    return;
                }
            }

            var manager = _listAdsManager.Find(manager => manager.MediationType == GameToolSetting.bannerMediation);

            if (mediation != MediationType.None)
            {
                manager = _listAdsManager.Find(mngr => mngr.MediationType == mediation);
            }

            if (manager)
            {
                manager.CanShowBanner = true;
                if (CanShowBanner)
                {
                    manager.ShowAdsBanner(position, location, key, addingTracking, normalBanner);
                }
            }
        }

        public void HideBanner(MediationType mediation = MediationType.None)
        {
            if (IsEditor())
            {
                Debug.Log("Hide Banner");
                if (bannerAdsFake != null)
                {
                    this.PostEvent(EventID.BannerShowing, new object[] { false });
                    bannerAdsFake.gameObject.SetActive(false);
                }

                return;
            }

            var manager = _listAdsManager.Find(manager => manager.MediationType == GameToolSetting.bannerMediation);

            if (mediation != MediationType.None)
            {
                manager = _listAdsManager.Find(mngr => mngr.MediationType == mediation);
            }

            if (manager)
            {
                manager.HideAdsBanner();
            }
        }

        public void ShowFull(Action<bool> onClosed, LocationTracking location, KeyAds key = KeyAds.none,
            Dictionary<string, object> addingTracking = null, MediationType mediation = MediationType.None)
        {
            onClosed += (_) => { ActiveLoading(false); };

            if (!CanShowInter)
            {
                onClosed?.Invoke(false);
            }
            else
            {
                APIHandle.ShowFull();

                AdsManager manager = GameToolSetting.GetAdsFullManager(AdsType.interstitial);
                
                if (mediation != MediationType.None)
                {
                    manager = _listAdsManager.Find(mngr => mngr.MediationType == mediation);
                }

                if (IsEditor() && manager && manager.CanShowFull())
                {
                    ShowAdsFake(AdsType.interstitial, onClosed);
                    return;
                }

                if (IsEditor() && !manager)
                {
                    ShowAdsFake(AdsType.interstitial, onClosed);
                    return;
                }

                if (manager && manager.CanShowFull())
                {
                    manager.ShowAdsInterstitial(onClosed, location, key, addingTracking);
                }
                else
                {
                    onClosed?.Invoke(false);
                }
            }
        }

        public bool IsRewardLoaded(KeyAds key, MediationType mediation = MediationType.None)
        {
            if (HasTurnOffInternet())
            {
                return false;
            }

            if (IsEditor())
            {
                return true;
            }

            AdsManager manager = GameToolSetting.GetAdsFullManager(AdsType.rewarded);
            
            if (mediation != MediationType.None)
            {
                manager = _listAdsManager.Find(mngr => mngr.MediationType == mediation);
            }

            if (manager && manager.IsRewardAdReady(key))
            {
                return true;
            }

            return false;
        }

        public void ShowReward(Action<bool> onClosed, LocationTracking location,
            KeyAds key = KeyAds.none,
            Dictionary<string, object> addingTracking = null)
        {
            onClosed += _ => ActiveLoading(false);
            _onRewardSuccess = onClosed;

            if (IsRemoveAdsFull)
            {
                CallbackRewardVideo(true);
                return;
            }

            if (IsEditor())
            {
                ShowAdsFake(AdsType.rewarded, CallbackRewardVideo);
                return;
            }

            APIHandle.ShowReward();
            var manager = GameToolSetting.GetAdsFullManager(AdsType.rewarded);
            if (manager)
            {
                manager.ShowAdsReward(CallbackRewardVideo, location, key, addingTracking);
            }
        }

        void CallbackRewardVideo(bool Success)
        {
            ActiveLoading(false);
            if (Success)
            {
                _onRewardSuccess(true);
                this.PostEvent(EventID.UpdateData);
                this.PostEvent(EventID.ShowToast, new object[] { "Success Reward" });
            }
            else
            {
                _onRewardSuccess(false);
            }
        }

        #endregion

        #region Utility

        public void ShowAdsFake(AdsType adsType, Action<bool> onClosed)
        {
            string adsTitle = "banner";
            int waitingTime = 0;
            switch (adsType)
            {
                case AdsType.app_open:
                    adsTitle = "App Open Ads";
                    waitingTime = fakeAppOpenWaitingTime;
                    break;
                case AdsType.banner:
                    adsTitle = "Banner Ads";
                    break;
                case AdsType.interstitial:
                    foreach (AdsManager manager in _listAdsManager)
                    {
                        manager.ResetTimeShowInterstitial();
                    }

                    adsTitle = "Interstitial Ads";
                    waitingTime = fakeInterWaitingTime;
                    break;
                case AdsType.rewarded:
                    adsTitle = "Reward Ads";
                    waitingTime = fakeRewardWaitingTime;
                    break;
            }

            if (adsType != AdsType.banner)
            {
                ShowLoading(() =>
                {
                    if (fullScreenAdsFake == null)
                    {
                        fullScreenAdsFake = Instantiate(Resources.Load<GameObject>("FullScreenAdsFake"),
                            transform.GetChild(0));
                    }
                    else if (!fullScreenAdsFake.activeInHierarchy)
                    {
                        fullScreenAdsFake.gameObject.SetActive(true);
                    }
                    else
                    {
                        return;
                    }

                    fullScreenAdsFake.GetComponent<AdsFake>().Init(adsTitle, onClosed, waitingTime);
                });
            }
            else
            {
                if (bannerAdsFake == null)
                {
                    bannerAdsFake = Instantiate(Resources.Load<GameObject>("BannerAdsFake"), transform.GetChild(0));
                }
                else
                    bannerAdsFake.gameObject.SetActive(true);

                bannerAdsFake.GetComponent<AdsFake>().Init(adsTitle, onClosed, waitingTime);
            }
        }

        public bool CanShowGDPR()
        {
#if USE_FIREBASE
            return FirebaseRemote.Instance.GetApiInfor().UseGDPR;
#else
            return false;
#endif
        }

        public void ShowLoading(Action onClosed)
        {
            ActiveLoading(false);
            if (onClosed != null)
                onClosed();
        }

        public bool HasTurnOffInternet()
        {
#if UNITY_EDITOR
            return false;
#else
            return InternetChecker.SHasTurnOffInternet();
#endif
        }

        public virtual bool IsCanShowRate()
        {
            return !GameData.Instance.Rated;
        }

        public void RequestReview()
        {
            if (IsCanShowRate())
            {
#if USE_GG_REVIVEW
                var reviewManager = new ReviewManager();

                var playReviewInfoAsyncOperation = reviewManager.RequestReviewFlow();

                playReviewInfoAsyncOperation.Completed += playReviewInfoAsync =>
                {
                    if (playReviewInfoAsync.Error == ReviewErrorCode.NoError)
                    {
                        var playReviewInfo = playReviewInfoAsync.GetResult();
                        reviewManager.LaunchReviewFlow(playReviewInfo);
                        GameData.Instance.Rated = true;
                    }
                    else
                    {
                        // handle error when loading review prompt
                    }
                };
#endif
#if UNITY_IOS
                {
                    UnityEngine.iOS.Device.RequestStoreReview();
                }
#endif
            }
        }

        public static bool IsEditor()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.OSXEditor)
                return true;
            return false;
        }

        public static bool IsAndroid()
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }

        public static bool IsIOS()
        {
#if UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        public bool IsAppInstalled(string bundleID)
        {
#if UNITY_EDITOR
            return false;
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
                Debug.Log(" ********LaunchOtherApp " + bundleID);
                AndroidJavaObject launchIntent = null;
                //if the app is installed, no errors. Else, doesn't get past next line
                try
                {
                    launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleID);
                    //        
                    //        ca.Call("startActivity",launchIntent);
                }
                catch (Exception ex)
                {
                    Debug.Log("exception" + ex.Message);
                }

                if (launchIntent == null)
                {
                    Debug.Log("********* null " + bundleID);
                    return false;
                }

                Debug.Log("**** exist " + bundleID);
                return true;
            }

            return false;
#else
            return false;
#endif
        }

        public static string StoreLinkGame()
        {
#if USE_FIREBASE
#if UNITY_ANDROID
            return FirebaseRemote.Instance.GetStoreConfig().GooglePlayLinkGame;
#elif UNITY_IOS
            return FirebaseRemote.Instance.GetStoreConfig().AppStoreLinkGame;
#endif
#else
            return "";
#endif
        }

        #endregion

        public static bool NewVersionAvailable()
        {
#if USE_FIREBASE
            if (FirebaseRemote.Instance.GetStoreConfig().ListVersion.Length <= 0)
            {
                return false;
            }

            string version = Application.version;

            foreach (string txt in FirebaseRemote.Instance.GetStoreConfig().ListVersion)
            {
                if (version == txt)
                {
                    return false;
                }
            }

            return true;
#else
            return false;
#endif
        }

        #region LOADING

        public void ActiveLoading(bool active, string info = "LOADING...")
        {
            loadingObj.SetActive(active);
            SetLoadingInfoText(info);
        }

        public void SetLoadingInfoText(string info = "LOADING...")
        {
            loadingInfoText.text = info;
        }

        public void DisableLoadingWithTime(float time = 0.5f)
        {
            StopCoroutine(nameof(WaitDisableLoading));
            StartCoroutine(nameof(WaitDisableLoading), time);
        }

        IEnumerator WaitDisableLoading(float time = 0.5f)
        {
            yield return new WaitForSeconds(time);
            loadingObj.SetActive(false);
        }

        #endregion
    }
}