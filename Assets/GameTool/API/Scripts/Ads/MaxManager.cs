#if USE_FALCON
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
#endif
#if USE_AF
using AppsFlyerSDK;
#endif
using GameTool.Assistants.DesignPattern;
#if USE_APPLOVIN_ADS
using DatdevUlts.Ults;
using System;
using System.Collections.Generic;
using DatdevUlts;
using GameTool.APIs.Analytics.Analytics;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.Toast;
using UnityEngine;
#endif

namespace GameTool.APIs.Scripts.Ads
{
    public class MaxManager : AdsManager
    {
#if USE_APPLOVIN_ADS
        public static MaxManager Instance { get; private set; }
        
        [SerializeField] bool showMediationDebugger;
        [SerializeField] bool aoaOnStart;

        [SerializeField] BannerPosition bannerPositionEditor = BannerPosition.Bottom;

        public bool AoaOnStart
        {
            get => aoaOnStart;
            set => aoaOnStart = value;
        }

        public GameToolSettings gameToolSettings => GameToolSettings.Instance;
        public string applovinSDKKey => gameToolSettings.applovinSDKKey;
        public string bannerID => gameToolSettings.applovinBannerID;
        public string interstitialID => gameToolSettings.applovinInterstitialID;
        public string rewardVideoID => gameToolSettings.applovinRewardVideoID;
        public string appOpenID => gameToolSettings.applovinAppOpenID;
        public string mrecID => gameToolSettings.applovinMrecID;

        private void Awake()
        {
            Instance = this;
        }

        public override void Init()
        {
            if (applovinSDKKey != "")
            {
#pragma warning disable CS0618 // Type or member is obsolete
                MaxSdk.SetSdkKey(applovinSDKKey);
#pragma warning restore CS0618 // Type or member is obsolete
                MaxSdk.InitializeSdk();
            }
            else
            {
                MaxLog("ApplovingSDKKey is empty! Nedd to add your ApplovingSDKKey ", MaxLogType.Error);
            }

            MaxSdkCallbacks.OnSdkInitializedEvent += _ =>
            {
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAoaAdLoadedEvent;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAoaAdLoadFailedEvent;
                MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAoaAdClickedEvent;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAoaAdRevenuePaidEvent;

                this.DelayedCall(3f, () =>
                {
                    aoaOnStart = false;
                });

                if (API.IsEditor())
                {
                    SetBannerPosition(bannerPositionEditor);
                }

                LoadAds();
            };

            if (showMediationDebugger)
            {
                MaxSdk.ShowMediationDebugger();
            }

            Debug.Log("MaxSdk Inited");
        }

        void LoadAds()
        {
            if (appOpenID != "")
            {
                MaxSdk.LoadAppOpenAd(appOpenID);
            }
            
            if (bannerID != "")
            {
                MaxSdkCallbacks.Banner.OnAdLoadedEvent += HandleOnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += HandleOnBannerAdLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += HandleOnBannerAdClickedEvent;
                MaxSdkCallbacks.Banner.OnAdCollapsedEvent += HandleOnBannerCollapsedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += HandleOnBannerOnAdRevenuePaidEvent;
                LoadBannerAd();
            }
            else
            {
                MaxLog("bannerID is empty! Nedd to add your bannerID ", MaxLogType.Error);
            }

            if (mrecID != "")
            {
                LoadMrec();
                MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
                MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
                MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
                MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
                MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
                MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;
            }
            else
            {
                MaxLog("mrecID is empty! Nedd to add your mrecID ", MaxLogType.Error);
            }

            if (interstitialID != "")
            {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += HandleOnInterstitialLoadedEvent;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += HandleOnInterstitialLoadFailedEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += HandleOnInterstitialAdFailedToDisplayEvent;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += HandleOnInterstitialHiddenEvent;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += HandleOnInterstitialDisplayedEvent;
                MaxSdkCallbacks.Interstitial.OnAdClickedEvent += HandleOnInterstitialClickedEvent;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += HandleOnInterstitialOnAdRevenuePaidEvent;
                LoadInterstitial();
            }
            else
            {
                MaxLog("interstitialID is empty! Nedd to add your interstitialID ", MaxLogType.Error);
            }

            if (rewardVideoID != "")
            {
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += HandleOnRewardedAdLoadedEvent;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += HandleOnRewardedAdLoadFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += HandleOnRewardedAdFailedToDisplayEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += HandleOnRewardedAdDisplayedEvent;
                MaxSdkCallbacks.Rewarded.OnAdClickedEvent += HandleOnRewardedAdClickedEvent;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += HandleOnRewardedAdHiddenEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += HandleOnRewardedAdReceivedRewardEvent;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += HandleOnRewardedAdRevenuePaidEvent;
                LoadRewardAd();
            }
            else
            {
                MaxLog("rewardVideoID is empty! Nedd to add your interstitialID ", MaxLogType.Error);
            }
        }

        #region AOA

        public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            MaxSdk.LoadAppOpenAd(appOpenID);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                AppOpenManager.Instance.ShowAdIfReady(false);
            }
        }

        public void OnAoaAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("MAX: OnAoaAdLoadedEvent");
        }

        public void OnAoaAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            Debug.Log("MAX: OnAoaAdLoadFailedEvent");
        }

        public void OnAoaAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log("MAX: OnAoaAdClickedEvent");
        }

        public void OnAoaAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
#if USE_FALCON
            new FAdLog(AdType.AppOpen, mRewardPlaceID.ToString(), adInfo.RevenuePrecision, "", adInfo.Revenue,
                adInfo.NetworkName, "Max").Send();
#endif
#if USE_AF
            Dictionary<string, string> purchaseEvent = new
                Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "applovin");
            purchaseEvent.Add("ad_unit", adInfo.AdUnitIdentifier);

            TrackingManager.Instance.TrackAfRevenue("applovin",MediationNetwork.ApplovinMax, adInfo.Revenue, purchaseEvent);
#endif

#if USE_SINGULAR
            SingularAdData data = new SingularAdData("applovin", "USD", adInfo.Revenue);
            SingularSDK.AdRevenue(data);
#endif

#if USE_ADJUST
            AdjustAdRevenue adRevenue =
                new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(adInfo.Revenue, "USD");
            adRevenue.setAdRevenueNetwork(adInfo.NetworkName);
            adRevenue.setAdRevenueUnit(adInfo.AdUnitIdentifier);
            adRevenue.setAdRevenuePlacement(adInfo.Placement);
            Adjust.trackAdRevenue(adRevenue);
#endif

#if USE_FIREBASE
            //rev tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters =
            {
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_mediation", AdRevenueParameters);
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdRevenueParameters);


            // level tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters2 =
            {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                    GameData.Instance.CurrentLevel.ToString()),
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_revenue_sdk", AdRevenueParameters2);
#endif
        }

        #endregion

        #region Mrec

        public override void LoadMrec()
        {
            // MRECs are sized to 300x250 on phones and tablets
            MaxSdk.CreateMRec(mrecID, MaxSdkBase.AdViewPosition.BottomCenter);
        }

        public override void ShowMrec(AnalyticID.LocationTracking mrecPlaceID)
        {
            MaxSdk.ShowMRec(mrecID);
        }

        public override void HideMrec()
        {
            MaxSdk.HideMRec(mrecID);
        }

        public void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            base.MrecAdLoadedEvent();
        }

        public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
        {
            base.MrecAdLoadFailEvent();
        }

        public void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            base.MrecAdClickEvent();
        }

        public void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
#if USE_AF
            Dictionary<string, string> purchaseEvent = new
                Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "applovin");
            purchaseEvent.Add("ad_unit", adInfo.AdUnitIdentifier);

            TrackingManager.Instance.TrackAfRevenue("applovin", MediationNetwork.ApplovinMax, adInfo.Revenue, purchaseEvent);
#endif

#if USE_SINGULAR
            SingularAdData data = new SingularAdData("applovin", "USD", adInfo.Revenue);
            SingularSDK.AdRevenue(data);
#endif

#if USE_ADJUST
            AdjustAdRevenue adRevenue =
                new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(adInfo.Revenue, "USD");
            adRevenue.setAdRevenueNetwork(adInfo.NetworkName);
            adRevenue.setAdRevenueUnit(adInfo.AdUnitIdentifier);
            adRevenue.setAdRevenuePlacement(adInfo.Placement);
            Adjust.trackAdRevenue(adRevenue);
#endif

#if USE_FIREBASE
            //rev tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters =
            {
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_mediation", AdRevenueParameters);
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdRevenueParameters);


            // level tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters2 =
            {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                    GameData.Instance.CurrentLevel.ToString()),
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_revenue_sdk", AdRevenueParameters2);
#endif
        }

        public void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
        }

        public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            base.MrecAdCloseEvent();
        }

        #endregion

        #region Banner

        MaxSdkBase.BannerPosition AdapterBannerPosition(BannerPosition position)
        {
            switch (position)
            {
                case BannerPosition.Top:
                    return MaxSdkBase.BannerPosition.TopCenter;
                case BannerPosition.Bottom:
                    return MaxSdkBase.BannerPosition.BottomCenter;
                case BannerPosition.TopLeft:
                    return MaxSdkBase.BannerPosition.TopLeft;
                case BannerPosition.TopRight:
                    return MaxSdkBase.BannerPosition.TopRight;
                case BannerPosition.BottomLeft:
                    return MaxSdkBase.BannerPosition.BottomLeft;
                case BannerPosition.BottomRight:
                    return MaxSdkBase.BannerPosition.BottomRight;
            }

            return MaxSdkBase.BannerPosition.BottomCenter;
        }

        public override void LoadBannerAd()
        {
            MaxSdk.SetBannerExtraParameter(bannerID, "adaptive_banner", "true");
            MaxSdk.CreateBanner(bannerID, AdapterBannerPosition(mBannerPosition));
            MaxSdk.SetBannerBackgroundColor(bannerID, Color.black);
        }

        public void SetBannerPosition(BannerPosition position)
        {
            mBannerPosition = position;
            MaxSdk.UpdateBannerPosition(bannerID, AdapterBannerPosition(position));
        }

        public override void ShowAdsBanner(BannerPosition position, AnalyticID.LocationTracking bannerPlaceID,
            AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null, bool normalBanner = true)
        {
            base.ShowAdsBanner(position, bannerPlaceID, key, addingTracking, normalBanner);
            mBannerPlaceID = bannerPlaceID;
            bannerDictTracking = addingTracking;
            isShowingBanner = true;
            SetBannerPosition(position);
            MaxSdk.ShowBanner(bannerID);
            MaxLog("ShowAdsBanner" + bannerID);
        }

        public override void HideAdsBanner()
        {
            isShowingBanner = true;
            this.PostEvent(EventID.BannerShowing, new object[] { false });
            MaxSdk.HideBanner(bannerID);
            if (isShowingBanner)
            {
                CanvasDetechBanner.isShowBanner = false;
                isShowingBanner = false;
            }

            MaxLog("HideAdsBanner" + bannerID);
        }

        private void HandleOnBannerAdLoadedEvent(string adId, MaxSdkBase.AdInfo apiInfor)
        {
            isShowingBanner = true;
            BannerAdLoadedEvent();
        }

        private void HandleOnBannerAdClickedEvent(string adId, MaxSdkBase.AdInfo apiInfor)
        {
            BannerAdClickEvent();
        }

        private void HandleOnBannerCollapsedEvent(string adId, MaxSdkBase.AdInfo apiInfor)
        {
            BannerAdCloseEvent();
            isShowingBanner = false;
        }

        private void HandleOnBannerAdLoadFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo)
        {
            BannerAdShowFailEvent();
        }

        private void HandleOnBannerOnAdRevenuePaidEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
#if USE_FALCON
            new FAdLog(AdType.Banner, mRewardPlaceID.ToString(), adInfo.RevenuePrecision, "", adInfo.Revenue,
                adInfo.NetworkName, "Max").Send();
#endif
#if USE_AF
            Dictionary<string, string> purchaseEvent = new
                Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "applovin");
            purchaseEvent.Add("ad_unit", adInfo.AdUnitIdentifier);

            TrackingManager.Instance.TrackAfRevenue("applovin", MediationNetwork.ApplovinMax, adInfo.Revenue, purchaseEvent);
#endif

#if USE_SINGULAR
            SingularAdData data = new SingularAdData("applovin", "USD", adInfo.Revenue);
            SingularSDK.AdRevenue(data);
#endif

#if USE_FIREBASE
            //rev tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters =
            {
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_mediation", AdRevenueParameters);
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdRevenueParameters);


            // level tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters2 =
            {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                    GameData.Instance.CurrentLevel.ToString()),
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_revenue_sdk", AdRevenueParameters2);
#endif
        }

        #endregion

        #region InterstitialAd

        protected override void LoadInterstitial(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            base.LoadInterstitial(key);
            if (MaxSdk.IsInterstitialReady(interstitialID)) return;
            Debug.Log("LoadInter");
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial, AnalyticID.MonetizationState.request,
                mBannerPlaceID);
            MaxSdk.LoadInterstitial(interstitialID);
        }

        public override bool IsInterAdReady(AnalyticID.KeyAds key = AnalyticID.KeyAds.none, bool ignoreCappingTime = false)
        {
            if (CanShowFull(ignoreCappingTime))
            {
                var interReady = MaxSdk.IsInterstitialReady(interstitialID);
                TrackingManager.Instance.TrackLimitedEvent(interReady ? "Inter_Ready" : "Inter_Failed");
                return interReady;
            }

            return false;
        }


        public override void ShowAdsInterstitial(Action<bool> callback, AnalyticID.LocationTracking interPlaceID,
            AnalyticID.KeyAds key, Dictionary<string, object> addingTracking = null, bool ignoreCappingTime = false)
        {
            base.ShowAdsInterstitial(callback, interPlaceID, key, addingTracking, ignoreCappingTime);
            mInterstitialAdCallback = callback;
            mInterPlaceID = interPlaceID;
            interDictTracking = addingTracking;

            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial,
                AnalyticID.MonetizationState.show_request, interPlaceID,
                addingTracking);

            if (!IsInterAdReady(AnalyticID.KeyAds.none, ignoreCappingTime))
            {
                if (CanCallFallback(AnalyticID.AdsType.interstitial, callback, interPlaceID, key, addingTracking))
                {
                    CallFallback(AnalyticID.AdsType.interstitial, callback, interPlaceID, key, addingTracking);
                }
                else
                {
                    mInterstitialAdCallback(false);
                }

                LoadInterstitial();
            }
            else
            {
                API.Instance.ShowLoading(() =>
                {
                    TrackingManager.Instance.ShowAdsEvent(AnalyticID.AdsType.interstitial);
                    MaxSdk.ShowInterstitial(interstitialID);
                    LoadInterstitial();
                });
            }
        }

        private void HandleOnInterstitialHiddenEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            InterAdCloseEvent();
        }

        private void HandleOnInterstitialDisplayedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            InterAdImpressionEvent();
        }

        private void HandleOnInterstitialClickedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            InterAdClickEvent();
        }

        private void HandleOnInterstitialAdFailedToDisplayEvent(string adId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            InterAdShowFailEvent();
        }

        private void HandleOnInterstitialLoadFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo)
        {
            InterAdLoadFailEvent();
        }

        private void HandleOnInterstitialLoadedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            InterAdLoadedEvent();
        }

        private void HandleOnInterstitialOnAdRevenuePaidEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            MaxLog("HandleOnInterstitialOnAdRevenuePaidEvent: " + interstitialID);
#if USE_FALCON
            new FAdLog(AdType.Interstitial, mRewardPlaceID.ToString(), adInfo.RevenuePrecision, "", adInfo.Revenue,
                adInfo.NetworkName, "Max").Send();
#endif
#if USE_AF
            Dictionary<string, string> purchaseEvent = new
                Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "applovin");
            purchaseEvent.Add("ad_unit", adInfo.AdUnitIdentifier);

            TrackingManager.Instance.TrackAfRevenue("applovin", MediationNetwork.ApplovinMax, adInfo.Revenue, purchaseEvent);
#endif

#if USE_SINGULAR
        SingularAdData data = new SingularAdData("applovin", "USD", adInfo.Revenue);
        SingularSDK.AdRevenue(data);
#endif

#if USE_FIREBASE
            //rev tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters =
            {
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_mediation", AdRevenueParameters);
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdRevenueParameters);


            // level tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters2 =
            {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                    GameData.Instance.CurrentLevel.ToString()),
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_revenue_sdk", AdRevenueParameters2);
#endif
        }

        #endregion

        #region RewardAd

        public override bool IsRewardAdReady(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            return (rewardVideoID != "" && MaxSdk.IsRewardedAdReady(rewardVideoID));
        }

        public override void ShowAdsReward(Action<bool> callback, AnalyticID.LocationTracking location,
            AnalyticID.KeyAds key = AnalyticID.KeyAds.none, Dictionary<string, object> addingTracking = null)
        {
            mHaveRewarded = false;
            mRewardedAdCallback = callback;
            mRewardPlaceID = location;
            rewardDictTracking = addingTracking;

            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.show_request,
                location, addingTracking);
            if (!IsRewardAdReady())
            {
                if (CanCallFallback(AnalyticID.AdsType.rewarded, callback, location, key, addingTracking))
                {
                    CallFallback(AnalyticID.AdsType.rewarded, callback, location, key, addingTracking);
                }
                else
                {
                    if (API.Instance.ShowToast)
                    {
                        ToastMgr.Instance.Show(new[] { "Video Ads is unavailable at the moment" });
                    }

                    mRewardedAdCallback(false);
                }
            }
            else
            {
                API.Instance.ShowLoading(() =>
                {
                    TrackingManager.Instance.ShowAdsEvent(AnalyticID.AdsType.rewarded);
                    MaxSdk.ShowRewardedAd(rewardVideoID);
                });
            }
        }

        public override void LoadRewardAd(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            MaxSdk.LoadRewardedAd(rewardVideoID);
        }

        private void HandleOnRewardedAdReceivedRewardEvent(string adId, MaxSdkBase.Reward reward,
            MaxSdkBase.AdInfo adInfo)
        {
            RewardAdRewardedEvent();
        }

        private void HandleOnRewardedAdHiddenEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            RewardAdCloseEvent();
        }

        private void HandleOnRewardedAdClickedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            RewardAdClickedEvent();
        }

        private void HandleOnRewardedAdDisplayedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            RewardAdImpressionEvent();
        }

        private void HandleOnRewardedAdFailedToDisplayEvent(string adId, MaxSdkBase.ErrorInfo errorInfo,
            MaxSdkBase.AdInfo adInfo)
        {
            RewardAdShowFailedEvent();
        }

        private void HandleOnRewardedAdLoadFailedEvent(string adId, MaxSdkBase.ErrorInfo errorInfo)
        {
            RewardAdLoadFailedEvent();
        }

        private void HandleOnRewardedAdLoadedEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            RewardAdLoadedEvent();
        }

        private void HandleOnRewardedAdRevenuePaidEvent(string adId, MaxSdkBase.AdInfo adInfo)
        {
            MaxLog("HandleOnRewardedAdRevenuePaidEvent: " + rewardVideoID);
#if USE_FALCON
            new FAdLog(AdType.Reward, mRewardPlaceID.ToString(), adInfo.RevenuePrecision, "", adInfo.Revenue,
                adInfo.NetworkName, "Max").Send();
#endif
#if USE_AF
            Dictionary<string, string> purchaseEvent = new
                Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "applovin");
            purchaseEvent.Add("ad_unit", adInfo.AdUnitIdentifier);

            TrackingManager.Instance.TrackAfRevenue("applovin", MediationNetwork.ApplovinMax, adInfo.Revenue, purchaseEvent);
#endif

#if USE_SINGULAR
        SingularAdData data = new SingularAdData("applovin", "USD", adInfo.Revenue);
        SingularSDK.AdRevenue(data);
#endif

#if USE_FIREBASE
            //rev tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters =
            {
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_mediation", AdRevenueParameters);
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdRevenueParameters);


            // level tracking
            Firebase.Analytics.Parameter[] AdRevenueParameters2 =
            {
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                    GameData.Instance.CurrentLevel.ToString()),
                new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, adInfo.Revenue),
                new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"),
            };

            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_revenue_sdk", AdRevenueParameters2);
#endif
        }

        #endregion

        #region Utility

        public enum MaxLogType
        {
            Debug,
            Warning,
            Error
        }

        public void MaxLog(string logContent, MaxLogType maxLogType = MaxLogType.Debug)
        {
            switch (maxLogType)
            {
                case MaxLogType.Debug:
                    Debug.Log(logContent);
                    break;
                case MaxLogType.Warning:
                    Debug.LogWarning(logContent);
                    break;
                case MaxLogType.Error:
                    Debug.LogWarning(logContent);
                    break;
            }
        }

        #endregion

#endif
        public override MediationType MediationType => MediationType.Applovin;
    }

    public class AppOpenManager : Singleton<AppOpenManager>
    {
        public string AppOpenAdUnitId => GameToolSettings.Instance.applovinAppOpenID;

        public void ShowAdIfReady(bool isStart)
        {
#if USE_APPLOVIN_ADS
            if (string.IsNullOrEmpty(AppOpenAdUnitId))
            {
                return;
            }


            if (isStart != MaxManager.Instance.AoaOnStart)
            {
                return;
            }

            if (isStart)
            {
                MaxManager.Instance.AoaOnStart = false;
            }

            CoroutineRunner.Instance.DelayedCall(0.5f, () =>
            {
                if (MaxSdk.IsAppOpenAdReady(AppOpenAdUnitId))
                {
                    if (!API.Instance.IsRemoveAds)
                    {
                        MaxSdk.ShowAppOpenAd(AppOpenAdUnitId);
                    }
                }
                else
                {
                    MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
                }
            });
#endif
        }
    }
}