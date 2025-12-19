using System;
using DatdevUlts.InspectorUtils;

#if USE_FALCON
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
#endif

using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.Enum;
using UnityEngine;

#if USE_ADMOB_APPOPEN
using GameTool.APIs.Analytics.Analytics;
using GameTool.Assistants.DictionarySerialize;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using System.Collections;
using System.Collections.Generic;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.FirebaseServices;
#endif

namespace GameTool.APIs.Scripts.Ads
{
    public class AppOpenAdManager : SingletonMonoBehaviour<AppOpenAdManager>
    {
        [HideInNormalInspector] public bool splAdsShowed;
        public bool autoShowInSpl = true;
        public AnalyticID.KeyAds _keyAdsSPL = AnalyticID.KeyAds.none;
        public AnalyticID.KeyAds _interBackFillSpl = AnalyticID.KeyAds.none;

        [Space] public bool autoRegistAppStateEventNotifier = true;
        public AnalyticID.KeyAds _keyAdsAppBack = AnalyticID.KeyAds.none;
        public AnalyticID.KeyAds _interBackFillAppBack = AnalyticID.KeyAds.none;

        [Space] [SerializeField] private float _interval;
#if USE_ADMOB_APPOPEN
        private const string LOG_TAG = "AppOpenAdManager";
        private int indexAdsOpen;
        private AppOpenAd ads_open;
        private AppOpenPlacement appOpenPlacement = AppOpenPlacement.appopen_splash;
        private float lastTimeAdShowed = -10;
        private bool LoadAdAtStart;

        private AnalyticID.KeyAds _keyAds = AnalyticID.KeyAds.none;

        public AnalyticID.KeyAds KeyAds
        {
            get => _keyAds;
            set => _keyAds = value;
        }


        GameToolSettings GameToolSettings => APIPlayerSetting.Instance.GameToolSetting;
        public Dict<AnalyticID.KeyAds, string[]> adsIds => GameToolSettings.admobAppOpenID;

        public float LastTimeAdShowed => lastTimeAdShowed;

        public int CurrentPlayDay
        {
            get => PlayerPrefs.GetInt("CurrentPlayDay", -1);
            set => PlayerPrefs.SetInt("CurrentPlayDay", value);
        }

        public static int CurrentCohortDay
        {
            get => PlayerPrefs.GetInt("CurrentCohortDay", -1);
            set => PlayerPrefs.SetInt("CurrentCohortDay", value);
        }

        protected override void Awake()
        {
            base.Awake();
            KeyAds = _keyAdsSPL;
        }


        public bool IsAdAvailable()
        {
            return ads_open != null;
        }

        private void Start()
        {
            appOpenPlacement = AppOpenPlacement.appopen_splash;

            if ((DateTime.Now.DayOfYear + DateTime.Now.Year) > CurrentPlayDay)
            {
                CurrentCohortDay++;
                CurrentPlayDay = (DateTime.Now.DayOfYear + DateTime.Now.Year);
            }

            if (GameToolSettings)
            {
                LoadAd();

                if (autoRegistAppStateEventNotifier)
                {
                    RegistAppStateEventNotifier();
                }

                StartCoroutine(LimitSplAdsTime());
            }
        }

        public void RegistAppStateEventNotifier()
        {
            AppStateEventNotifier.AppStateChanged += (state) =>
            {
                if (state == AppState.Foreground)
                {
                    appOpenPlacement = AppOpenPlacement.open_resume;

                    if (FirebaseRemote.Instance.GetApiInfor().open_resume)
                    {
                        if (autoRegistAppStateEventNotifier)
                        {
                            KeyAds = _keyAdsAppBack;
                        }
                        ShowAdIfAvailable(KeyAds, _interBackFillAppBack, onAdClose: () => { });
                    }
                }
            };
        }

        IEnumerator LimitSplAdsTime()
        {
            yield return new WaitForSecondsRealtime(5);
            Debug.Log(LOG_TAG + ": splAdsShowed true on limit");
            splAdsShowed = true;
        }

        IEnumerator LoadAdInternal()
        {
            if (API.Instance.IsRemoveAds)
                yield return null;

            if (indexAdsOpen >= adsIds[KeyAds].Length)
            {
                Debug.Log(LOG_TAG + ": No ads found for placement: " + appOpenPlacement);
                indexAdsOpen = 0;
                if (splAdsShowed) yield return null;
                yield return new WaitForSecondsRealtime(0.2f);
            }

            indexAdsOpen = Mathf.Clamp(indexAdsOpen, 0, adsIds[KeyAds].Length - 1);
            Debug.Log(LOG_TAG + ": Requesting ad:" + appOpenPlacement);
            AdRequest request = new AdRequest();
            AppOpenAd.Load(
                adsIds[KeyAds][indexAdsOpen],
                request,
                (
                    (appOpenAd, error) =>
                    {
                        if (error != null)
                        {
                            TrackingManager.Instance.TrackEvent(appOpenPlacement + "_loadfailed");
                            Debug.LogFormat(LOG_TAG + ": Failed to load the ad {0}. (reason: {1})", appOpenPlacement,
                                error.GetMessage());

                            indexAdsOpen++;
                            StartCoroutine(LoadAdInternal());
                        }
                        else
                        {
                            Debug.Log(LOG_TAG + ": Load ads successfully - placement: " + appOpenPlacement);
                            ads_open = appOpenAd;
                            indexAdsOpen = 0;
                            if (!splAdsShowed && autoShowInSpl)
                            {
                                TrackingManager.Instance.TrackEvent(appOpenPlacement + "_loaded");
                                splAdsShowed = true;
                            }
                        }
                    }
                )
            );
        }

        public void ShowAdIfAvailable(AnalyticID.KeyAds key, AnalyticID.KeyAds keyInter, Action onAdClose = null, Action onAdImpression = null,
            Action onAdShowFailed = null)
        {
            Debug.Log("AOA: Call Show AOA");
            if (Time.time - lastTimeAdShowed < _interval || API.Instance.IsRemoveAds)
            {
                if (Time.time - lastTimeAdShowed < _interval)
                {
                    Debug.Log("AOA Failed: Interval check false");
                }
                else
                {
                    Debug.Log("AOA Failed: Removed Ads");
                }

                return;
            }

            if (!IsAdAvailable())
            {
                Debug.Log("AOA Failed: Ads not available");
                onAdShowFailed?.Invoke();

                if (FirebaseRemote.Instance.GetApiInfor().show_inter_on_aoa_fail)
                {
                    API.Instance.ShowFull(_ => { }, AnalyticID.LocationTracking.aoa_failed, keyInter);
                }
                
                return;
            }

            if (API.IsEditor())
            {
                API.Instance.ShowAdsFake(AnalyticID.AdsType.app_open, (_) => { onAdClose?.Invoke(); });
                onAdImpression?.Invoke();
                return;
            }

            TrackingManager.Instance.TrackEvent("AppOpen_Impression");

            ads_open.OnAdFullScreenContentOpened += () => { onAdImpression?.Invoke(); };
            ads_open.OnAdFullScreenContentOpened += HandleAdDidPresentFullScreenContent;
            ads_open.OnAdPaid += HandlePaidEvent;
            ads_open.OnAdFullScreenContentClosed += () =>
            {
                onAdClose?.Invoke();

                SetUpGameForAds(false);
            };

            ads_open.OnAdFullScreenContentFailed += (args) =>
            {
                onAdShowFailed?.Invoke();
                SetUpGameForAds(false);
                Debug.LogFormat(LOG_TAG + ": Failed to present the ad (reason: {0})", args.GetMessage());
            };


            TrackingManager.Instance.TrackEvent(appOpenPlacement.ToString());

            if (CurrentCohortDay <= 7)
                TrackingManager.Instance.TrackEvent(appOpenPlacement + "_d" + CurrentCohortDay.ToString());
            lastTimeAdShowed = Time.time;
            ads_open.Show();
            ads_open = null;
            StartCoroutine(LoadAdInternal());
        }


        private void HandlePaidEvent(AdValue args)
        {
            double revenue = (double)args.Value / 1000000;

#if USE_ADJUST
            com.adjust.sdk.AdjustAdRevenue adRevenue =
 new com.adjust.sdk.AdjustAdRevenue(com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(revenue, args.CurrencyCode);
            com.adjust.sdk.Adjust.trackAdRevenue(adRevenue);
#endif

#if USE_FIREBASE
            TrackingManager.Instance.TrackFirebaseEvent("ad_revenue_sdk", new Dictionary<string, object>()
            {
                { Firebase.Analytics.FirebaseAnalytics.ParameterLevel, GameData.Instance.CurrentLevel.ToString() },
                { "ad_platform", "admob" },
                { "ad_format", "appopen" },
                { "currency", "USD" },
                { "value", revenue },
            });

            TrackingManager.Instance.TrackFirebaseEvent("ad_impression_admob", new Dictionary<string, object>()
            {
                { "ad_platform", "admob" },
                { "ad_format", "appopen" },
                { "currency", "USD" },
                { "value", revenue },
            });
#endif

#if USE_FALCON
            new FAdLog(AdType.Interstitial, "aoa", args.Precision.ToString(), args.CurrencyCode, args.Value, "admob", "admob").Send();
#endif

#if USE_AF
            System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
                System.Collections.Generic.Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "admod");
            purchaseEvent.Add("ad_unit", "app_open");
            TrackingManager.Instance.TrackAfRevenue("admod",
                AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, (double)(revenue),
                purchaseEvent);
#endif

#if USE_SINGULAR
            SingularAdData data = new SingularAdData("admod", "USD", revenue);
            SingularSDK.AdRevenue(data);
#endif
        }

        private void HandleAdDidPresentFullScreenContent()
        {
            SetUpGameForAds(true);
            TrackingManager.Instance.ShowAdsEvent(AnalyticID.AdsType.app_open);
        }

        void SetUpGameForAds(bool adsShow)
        {
            if (adsShow)
            {
                TimeManager.Instance.Add(0);
                AudioListener.volume = 0;
            }
            else
            {
                TimeManager.Instance.Remove(0);
                AudioListener.volume = 1;
            }
        }

        public void LoadAd()
        {
            if (LoadAdAtStart)
            {
                return;
            }

            LoadAdAtStart = true;
            StartCoroutine(LoadAdInternal());
        }

        public void ResetTimeShowAds()
        {
            lastTimeAdShowed = Time.time;
        }

        public enum AppOpenPlacement
        {
            appopen_splash,
            open_resume,
        }
#else
        public void LoadAd() { }

        public void ResetTimeShowAds() { }

        public void ShowAdIfAvailable(Action action) { }
#endif
    }
}