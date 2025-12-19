#if USE_IRS_ADS
using GameTool.API.Analytics.Analytics;
using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using com.adjust.sdk;
using GameToolSample.Scripts.Toast;
using UnityEngine;
using static GameToolSample.Scripts.Enum.AnalyticID;

#endif

namespace GameTool.APIs.Scripts.Ads
{
    public class IronScrManager : AdsManager
    {
#if USE_IRS_ADS
        public override void Init()
        {
            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

            //Init AdQuality để check các Creative
            IronSourceAdQuality.Initialize(GameToolSettings.irsAppKey);
            IronSource.Agent.shouldTrackNetworkState(true);

            if (GameToolSettings != null && !string.IsNullOrEmpty((GameToolSettings.irsAppKey)))
            {
                IronSource.Agent.init(GameToolSettings.irsAppKey, IronSourceAdUnits.REWARDED_VIDEO,
                    IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
            }
            else
            {
                Debug.LogError("GameTool Settings is Error");
            }
        }

        private void SdkInitializationCompletedEvent()
        {
            adsInitializationCompleted = true;
            AddCallBack();
            LoadBannerAd();
            LoadInterstitial();
            StartCoroutine(WaitForDataLoaded());
        }

        void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
        }

        public void AddCallBack()
        {
            //Add Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardAdImpressionEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardAdCloseEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoAdClickedEvent;

            // Add Interstitial Events
            IronSourceInterstitialEvents.onAdReadyEvent += InterAdLoadedEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdOpenedEvent += InterAdImpressionEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent += InterAdCloseEvent;

            // Add Banner Events
            IronSourceBannerEvents.onAdLoadedEvent += BannerAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceBannerEvents.onAdClickedEvent += BannerAdClickedEvent;
            IronSourceBannerEvents.onAdScreenPresentedEvent += BannerAdPresentedEvent;
            IronSourceBannerEvents.onAdScreenDismissedEvent += BannerAdDismissedEvent;

            IronSourceEvents.onImpressionDataReadyEvent += ImpressionSuccessEvent;
        }
        void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
            //#if USE_ADJUST
            // rev realtime của Adjust
            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceIronSource);
            adjustAdRevenue.setRevenue((double)impressionData.revenue, "USD");
            adjustAdRevenue.setAdRevenueNetwork(impressionData.adNetwork);
            adjustAdRevenue.setAdRevenueUnit(impressionData.adUnit);
            adjustAdRevenue.setAdRevenuePlacement(impressionData.placement);
            Adjust.trackAdRevenue(adjustAdRevenue);
            //#endif

#if USE_FIREBASE
        // tính rev cho check revenue realtime trên CC 
        Firebase.Analytics.Parameter[] parameters =
                   {
            new Firebase.Analytics.Parameter("ad_platform", "ironsource"),
            new Firebase.Analytics.Parameter("ad_source", impressionData.adNetwork),
            new Firebase.Analytics.Parameter("ad_format", impressionData.adUnit),
            new Firebase.Analytics.Parameter("currency", "USD"),
            new Firebase.Analytics.Parameter("value", (double)impressionData.revenue)
        };
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", parameters);//
        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression_mediation", parameters);//


        // tính rev cho check revenue và ads imdau theo level trên CC 
        Firebase.Analytics.Parameter[] AdRevenueParameters = {
        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel, (GameData.Instance.CurrentLevel + 1).ToString()),
        new Firebase.Analytics.Parameter("ad_format", impressionData.adUnit),
        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, (double)impressionData.revenue),
        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterCurrency, "USD"), };

        Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_revenue_sdk", AdRevenueParameters);

#endif

#if USE_AF
        // rev realtime của AF
        System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
        System.Collections.Generic.Dictionary<string, string>();
        purchaseEvent.Add("ad_platform", "ironsource");
        purchaseEvent.Add("ad_unit", impressionData.adUnit);
        purchaseEvent.Add("ad_network", impressionData.adNetwork);

        TrackingManager.Instance.TrackAfRevenue("ironsource", AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
        (double)impressionData.revenue, purchaseEvent);
#endif

        }
        public bool IsRewardLoaded()
        {
            if (IronSource.Agent.isRewardedVideoAvailable())
                return true;
            return false;
        }

        #region Banner
        IEnumerator WaitForDataLoaded()
        {
            yield return new WaitUntil(() => API.adsConfigLoaded);
            if (API.CanShowBanner && _canShowBanner)
                ShowAdsBanner(mBannerPosition, mBannerPlaceID);

        }
        public override void LoadBannerAd()
        {
            // Set Size và Set AdaptiveBanner Size
            // IronSourceBannerSize ironSourceBannerSize = GetBannerSize();
            IronSourceBannerSize ironSourceBannerSize = IronSourceBannerSize.BANNER;
            ironSourceBannerSize.SetAdaptive(true);
            IronSource.Agent.loadBanner(ironSourceBannerSize, AdapterBannerPosition(mBannerPosition));
        }

        // Gán Vị trí banner từ Position chung vào từng loại Mediation (dùng cho kế thừa ở các loại Mediation và AdsManager)
        IronSourceBannerPosition AdapterBannerPosition(BannerPosition position)
        {
            switch(position)
            {
                case BannerPosition.Top:
                    return IronSourceBannerPosition.TOP;
                case BannerPosition.Bottom:
                    return IronSourceBannerPosition.BOTTOM;
            }
            return IronSourceBannerPosition.BOTTOM;
        }
        public override void ShowAdsBanner(BannerPosition position, LocationTracking location, Dictionary<string, object> addingTracking = null)
        {
            if (!adsInitializationCompleted) return;
            // Khi đổi vị trí của Banner ở IS không hỗ trợ việc đổi trực tiếp luôn như Max, mình cần destroy cái cũ đi và load lại cái Banner mới theo Pos mới.
            // ví dụ chuyển từ Banner Bottom sang Banner Top.

            if (position != mBannerPosition)
            {
                IronSource.Agent.destroyBanner();
                mBannerPosition = position;
                LoadBannerAd();
                return;
            }

            mBannerPlaceID = location;
            bannerDictTracking = addingTracking;
            IronSource.Agent.displayBanner();
        }

        public override void HideAdsBanner()
        {
            _canShowBanner = false;
            IronSource.Agent.hideBanner();
        }

        void BannerAdPresentedEvent(IronSourceAdInfo info)
        {
            BannerAdImpressionEvent();
            this.PostEvent(EventID.BannerShowing, new object[] { true });
        }
        void BannerAdDismissedEvent(IronSourceAdInfo info)
        {
            this.PostEvent(EventID.BannerShowing, new object[] { false });
            BannerAdCloseEvent();
        }
        void BannerAdClickedEvent(IronSourceAdInfo info)
        {
            BannerAdClickEvent();
        }

        // Banner của IS khi load xong sẽ tự show, mình cần xử lí thêm để phù hợp với các config ads của mình, để không cho Banner tự show khi load
        // Trường hợp hay gặp là muốn config show banner ở level 10, nhưng vào game ở level 1 gọi load banner xong nó sẽ tự show, lúc này cần check để ẩn Banner.
        void BannerAdLoadedEvent(IronSourceAdInfo info)
        {
            bannerLoaded = true;
            BannerAdLoadedEvent();
            if (!API.CanShowBanner || !_canShowBanner)
                IronSource.Agent.hideBanner();
        }

        void BannerAdLoadFailedEvent(IronSourceError info)
        {
            bannerLoaded = false;
            BannerAdLoadFailEvent();
        }

        // Nhận size từ Firebase, mặc định đang là Smart size
        IronSourceBannerSize GetBannerSize()
        {
#if USE_FIREBASE
        int bannerSizeId = FirebaseRemote.Instance.GetApiInfor().BannerSizeId;
        switch (bannerSizeId)
        {
            case 0:
                return IronSourceBannerSize.BANNER;
            case 1:
                return IronSourceBannerSize.LARGE;
            case 2:
                return IronSourceBannerSize.RECTANGLE;
            case 3:
                return IronSourceBannerSize.SMART;
            default:
                return IronSourceBannerSize.BANNER;
        }
#else
            return IronSourceBannerSize.BANNER;
#endif
        }
        #endregion

        #region Reward
        public override bool IsRewardAdReady()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }
        public override void ShowAdsReward(Action<bool> callback, LocationTracking location, Dictionary<string, object> addingTracking = null)
        {
            mHaveRewarded = false;
            mRewardedAdCallback = callback;
            mRewardPlaceID = location;
            rewardDictTracking = addingTracking;

            TrackingManager.Instance.TrackAds(AdsType.rewarded, MonetizationState.show_request, location, addingTracking);
            if (!IsRewardAdReady())
            {
                if (CanCallFallback(AnalyticID.AdsType.rewarded, callback, location, addingTracking))
                {
                    CallFallback(AnalyticID.AdsType.rewarded, callback, location, addingTracking);
                }
                else
                {
                    if (API.Instance.ShowToast)
                    {
                        ToastMgr.Instance.Show(new[] { "Video Ads is unavailable at the moment" });
                    }

                    mRewardedAdCallback(false);
                }

                return;
            }
            else
            {
                API.Instance.ShowLoading(() =>
                {
#if USE_ADMOD_APPOPEN
                AppOpenAdManager.Instance.ResetTimeShowAds();
#endif
                    TrackingManager.Instance.ShowAdsEvent(AdsType.rewarded);
                    IronSource.Agent.showRewardedVideo();
                });
            }
        }

        void RewardAdImpressionEvent(IronSourceAdInfo ironSourceAdInfo)
        {
            // để đảm bảo các trường hợp, nên cho thêm tắt loading khi ads reward show thành công
            API.Instance.ActiveLoading(false);
            base.RewardAdImpressionEvent();
        }
        void RewardAdCloseEvent(IronSourceAdInfo ironSourceAdInfo)
        {
            // tắt loading khi ads reward tắt
            API.Instance.ActiveLoading(false);
            base.RewardAdCloseEvent();
        }
        void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp, IronSourceAdInfo ironSourceAdInfo)
        {
            base.RewardAdRewardedEvent();
        }

        void RewardedVideoAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info)
        {
            base.RewardAdShowFailedEvent();
        }

        void RewardedVideoAdClickedEvent(IronSourcePlacement place, IronSourceAdInfo info)
        {
            base.RewardAdClickedEvent();
        }
        #endregion

        #region Interstitial
        protected override void LoadInterstitial()
        {
            if (!adsInitializationCompleted || IronSource.Agent.isInterstitialReady()) return;
            Debug.Log("LoadInter");
            TrackingManager.Instance.TrackAds(AdsType.interstitial, MonetizationState.request, mBannerPlaceID);
            IronSource.Agent.loadInterstitial();
        }
        public override bool IsInterAdReady()
        {
            var interReady = IronSource.Agent.isInterstitialReady();
            var timeIntervalReady = CanShowFull();
            if (interReady)
            {
                TrackingManager.Instance.TrackAds(AdsType.interstitial, MonetizationState.server_ready, mInterPlaceID, interDictTracking);
            }
            else
            {
                TrackingManager.Instance.TrackAds(AdsType.interstitial, MonetizationState.server_failed, mInterPlaceID, interDictTracking);
            }

            if (timeIntervalReady)
            {
                TrackingManager.Instance.TrackAds(AdsType.interstitial, MonetizationState.interval_ready, mInterPlaceID, interDictTracking);
            }
            else
            {
                TrackingManager.Instance.TrackAds(AdsType.interstitial, MonetizationState.interval_failed, mInterPlaceID, interDictTracking);
            }

            //IS trả về Inter và đủ điều kiện time interval
            return interReady && timeIntervalReady;
        }

        public override void ShowAdsInterstitial(Action<bool> callback, LocationTracking interPlaceID,
            Dictionary<string, object> addingTracking = null)
        {
            mInterstitialAdCallback = callback;
            mInterPlaceID = interPlaceID;
            interDictTracking = addingTracking;

            TrackingManager.Instance.TrackAds(AdsType.interstitial, MonetizationState.show_request, interPlaceID, addingTracking);

            if (!IsInterAdReady())
            {
                if (CanCallFallback(AnalyticID.AdsType.interstitial, callback, interPlaceID, addingTracking))
                {
                    CallFallback(AnalyticID.AdsType.interstitial, callback, interPlaceID, addingTracking);
                }
                else
                {
                    mInterstitialAdCallback(false);
                }
                
                LoadInterstitial();
                return;
            }
            else
            {
                API.Instance.ShowLoading(() =>
                {
#if USE_ADMOD_APPOPEN
                    AppOpenAdManager.Instance.ResetTimeShowAds();
#endif
                    TrackingManager.Instance.ShowAdsEvent(AdsType.interstitial);
                    IronSource.Agent.showInterstitial();
                });
            }
        }

        void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            base.InterAdLoadFailEvent();
        }
        void InterAdLoadedEvent(IronSourceAdInfo info)
        {
            base.InterAdLoadedEvent();
        }
        void InterAdImpressionEvent(IronSourceAdInfo info)
        {
            API.Instance.ActiveLoading(false);
            base.InterAdImpressionEvent();
        }
        void InterstitialOnAdClickedEvent(IronSourceAdInfo info)
        {
            base.InterAdClickEvent();
        }
        void InterAdCloseEvent(IronSourceAdInfo info)
        {
            API.Instance.ActiveLoading(false);
            base.InterAdCloseEvent();
        }

        void InterstitialAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info)
        {
            base.InterAdShowFailEvent();
        }
        #endregion

#endif
        public override MediationType MediationType => MediationType.IronSource;
    }
}