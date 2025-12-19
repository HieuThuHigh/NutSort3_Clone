using System;
using System.Collections.Generic;
using DatdevUlts.Ults;

#if USE_FALCON
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
#endif

using GameTool.APIs.Analytics.Analytics;
using GameTool.Assistants.DesignPattern;
using GameTool.Assistants.DictionarySerialize;
using GameToolSample.Scripts.Enum;
using GameToolSample.Scripts.FirebaseServices;
using GameToolSample.Scripts.Toast;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
#if USE_ADMOB_ADS
using System.Collections.Generic;
using GameTool.Assistants.DesignPattern;
using GameToolSample.Scripts.Enum;
using System;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.FirebaseServices;
using GameToolSample.Scripts.Toast;
using UnityEngine;
using GoogleMobileAds.Api;
#endif

#if USE_FIREBASE
using FirebaseAnalytics = Firebase.Analytics.FirebaseAnalytics;
#endif

namespace GameTool.APIs.Scripts.Ads
{
    public class AdmobManager : AdsManager
    {
#if USE_ADMOB_ADS
        public Dict<AnalyticID.KeyAds, string[]> bannerIDs => GameToolSettings.admobBannerID;
        public Dict<AnalyticID.KeyAds, string[]> collBannerIDs => GameToolSettings.admobCollapsibleBannerID;

        private Dict<AnalyticID.KeyAds, string[]> interstitialIDs => GameToolSettings.admobInterstitialID;

        private Dict<AnalyticID.KeyAds, string[]> rewardVideoIDs => GameToolSettings.admobRewardVideoID;

        private BannerView bannerView;
        
        private Dict<AnalyticID.KeyAds, InterstitialAd> interstitial = new Dict<AnalyticID.KeyAds, InterstitialAd>();
        private Dict<AnalyticID.KeyAds,int> currentInterIndexID = new Dict<AnalyticID.KeyAds, int>();
        private Dict<AnalyticID.KeyAds,Coroutine> coroutinesInter = new Dict<AnalyticID.KeyAds, Coroutine>();
        private Dict<AnalyticID.KeyAds, int> retryAttemptInterAdmob = new Dict<AnalyticID.KeyAds, int>();

        private Dict<AnalyticID.KeyAds, RewardedAd> rewardedAd = new Dict<AnalyticID.KeyAds, RewardedAd>();
        private Dict<AnalyticID.KeyAds, int> currentRewardIndexID = new Dict<AnalyticID.KeyAds, int>();
        private Dict<AnalyticID.KeyAds, Coroutine> coroutinesReward = new Dict<AnalyticID.KeyAds, Coroutine>();
        private Dict<AnalyticID.KeyAds, int> retryAttemptRewardAdmob = new Dict<AnalyticID.KeyAds, int>();


        public override void Init()
        {
            foreach (var data in interstitialIDs)
            {
                interstitial.Add(data.Key, null);
                currentInterIndexID.Add(data.Key, 0);
                retryAttemptInterAdmob.Add(data.Key, 0);
                coroutinesInter.Add(data.Key, null);
            }
            
            foreach (var data in rewardVideoIDs)
            {
                rewardedAd.Add(data.Key, null);
                currentRewardIndexID.Add(data.Key, 0);
                retryAttemptRewardAdmob.Add(data.Key, 0);
                coroutinesReward.Add(data.Key, null);
            }
            
            MobileAds.Initialize(_ => { LoadAds(); });
        }


        private void LoadAds()
        {
            adsInitializationCompleted = true;

            foreach (var data in interstitialIDs)
            {
                LoadInterstitialInternal(data.Key);
            }

            foreach (var data in rewardVideoIDs)
            {
                LoadRewardAd(data.Key);
            }

            if (AppOpenAdManager.IsInstanceValid())
            {
                AppOpenAdManager.Instance.LoadAd();
            }
        }

        #region Banner

        private AdPosition AdapterBannerPosition(BannerPosition position)
        {
            switch (position)
            {
                case BannerPosition.Top:
                    return AdPosition.Top;
                case BannerPosition.Bottom:
                    return AdPosition.Bottom;
            }

            return AdPosition.Bottom;
        }


        public void LoadBanner(BannerPosition position, bool normalBanner, AnalyticID.KeyAds key)
        {
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            AdSize adaptiveSize =
                AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

            bannerView = new BannerView(normalBanner ? bannerIDs[key][0] : collBannerIDs[key][0], adaptiveSize,
                AdapterBannerPosition(position));

            bannerView.OnBannerAdLoaded += HandleAdLoaded;
            bannerView.OnBannerAdLoadFailed += error => HandleAdFailedToLoad(error, key);
            bannerView.OnAdFullScreenContentOpened += HandleAdOpened;
            bannerView.OnAdFullScreenContentClosed += HandleAdClosed;
            bannerView.OnAdPaid += HandleOnBannerOnAdRevenuePaidEvent;

            AdRequest adRequest = new AdRequest();
            if (!normalBanner)
            {
                if (position.ToString().Contains("Top"))
                {
                    if (position != BannerPosition.Top)
                    {
                        Debug.LogWarning($"Sai position của Collapsible Banner: position = {position}");
                    }

                    adRequest.Extras.Add("collapsible", "top");
                }
                else
                {
                    adRequest.Extras.Add("collapsible", "bottom");
                }
            }

            bannerView.LoadAd(adRequest);
        }


        public override void ShowAdsBanner(BannerPosition position, AnalyticID.LocationTracking bannerPlaceID,
            AnalyticID.KeyAds key, Dictionary<string, object> addingTracking = null, bool normalBanner = true)
        {
            base.ShowAdsBanner(position, bannerPlaceID, key, addingTracking, normalBanner);
            mBannerPlaceID = bannerPlaceID;
            mBannerPosition = position;
            LoadBanner(position, normalBanner, key);
        }


        public override void HideAdsBanner()
        {
            this.PostEvent(EventID.BannerShowing, new object[] { false });
            if (bannerView != null)
            {
                bannerView.Hide();
                bannerView.Destroy();
            }

            isShowingBanner = false;
        }


        public void HandleAdLoaded()
        {
            print("HandleAdLoaded event received");
            this.PostEvent(EventID.BannerShowing, new object[] { true });
            isShowingBanner = true;
        }


        public void HandleAdFailedToLoad(AdError args, AnalyticID.KeyAds key)
        {
            LoadBanner(mBannerPosition, bannerIsNormal, key);
            TrackingManager.Instance.TrackEvent("BannerAd",
                new Dictionary<string, object> { { "AdLoadFailed", mBannerPlaceID.ToString() } });

            print(
                "HandleFailedToReceiveAd event received with message: " + args.GetMessage());
        }


        public void HandleAdOpened()
        {
            print("HandleAdOpened event received");
        }


        public void HandleAdClosed()
        {
            print("HandleAdClosed event received");
            isShowingBanner = false;
        }


        private void HandleOnBannerOnAdRevenuePaidEvent(AdValue adValue)
        {
            double revenue = (double)adValue.Value / 1000000;
#if USE_ADJUST
            com.adjust.sdk.AdjustAdRevenue adRevenue =
 new com.adjust.sdk.AdjustAdRevenue(com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(revenue, adValue.CurrencyCode);
            com.adjust.sdk.Adjust.trackAdRevenue(adRevenue);
#endif
#if USE_FIREBASE
            TrackingManager.Instance.TrackFirebaseEvent("ad_revenue_sdk", new Dictionary<string, object>()
            {
                { FirebaseAnalytics.ParameterLevel, GameData.Instance.CurrentLevel.ToString() },
                { "ad_platform", "admob" },
                { "ad_format", "banner" },
                { "currency", adValue.CurrencyCode },
                { "value", revenue },
            });

            //rev tracking
            TrackingManager.Instance.TrackFirebaseEvent("ad_impression_admob", new Dictionary<string, object>()
            {
                { "ad_platform", "admob" },
                { "ad_format", "banner" },
                { "currency", "USD" },
                { "value", revenue },
            });
#endif

#if USE_AF
            System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
                System.Collections.Generic.Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "admod");

            TrackingManager.Instance.TrackAfRevenue("admod",
                AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, (double)(revenue),
                purchaseEvent);
#endif

#if USE_SINGULAR
            SingularAdData data = new SingularAdData("admod", "USD", revenue);
            SingularSDK.AdRevenue(data);
#endif
        }

        #endregion


        #region InterstitialAd
        
        protected override void LoadInterstitial(AnalyticID.KeyAds key)
        {
            base.LoadInterstitial(key);
            LoadInterstitialInternal(key);
        }

        private void LoadInterstitialInternal(AnalyticID.KeyAds key)
        {
            AdRequest adRequest = new AdRequest();

            InterstitialAd.Load(interstitialIDs[key][currentInterIndexID[key]], adRequest,
                (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        HandleOnAdFailedToLoad(error, key);
                        return;
                    }

                    HandleOnAdLoaded(key);

                    interstitial[key] = ad;

                    interstitial[key].OnAdFullScreenContentOpened += HandleOnAdOpening;
                    interstitial[key].OnAdFullScreenContentClosed += () => HandleOnAdClosed(key);
                    interstitial[key].OnAdFullScreenContentFailed += args => HandleOnAdFailedToShow(args, key);
                    interstitial[key].OnAdPaid += HandleOnAdRevenuePaidEvent;
                });
        }


        public override bool CanShowFull()
        {
            if (!startAds)
            {
                startAds = true;
                return true;
            }

            return Time.time - _lastTimeShowInterstitial >=
                   FirebaseRemote.Instance.GetApiInfor().AdmobInterstitialInterval;
        }

        public override bool IsInterAdReady(AnalyticID.KeyAds key)
        {
            return !NeedReloadInter(key) && CanShowFull();
        }

        public override void ShowAdsInterstitial(Action<bool> callback, AnalyticID.LocationTracking interPlaceID,
            AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null)
        {
            mInterstitialAdCallback = callback;
            mInterPlaceID = interPlaceID;
            interDictTracking = addingTracking;
            TrackingManager.Instance.TrackEvent("InterstitialAd",
                new Dictionary<string, object> { { "AdRequest", mInterPlaceID.ToString() } });

            Debug.LogError("Call show inter Admob");

            if (IsInterAdReady(key))
            {
                API.Instance.ShowLoading(() =>
                {
                    interstitial[key].Show();
                    interstitial[key] = null;
                });
            }
            else
            {
                Debug.LogError("Check can show");

                if (NeedReloadInter(key))
                {
                    Debug.LogError("Reload Interstitial");
                    retryAttemptInterAdmob[key] = 0;
                    ReloadInterAd(key);
                }

                if (CanCallFallback(AnalyticID.AdsType.interstitial, callback, interPlaceID, key, addingTracking))
                {
                    CallFallback(AnalyticID.AdsType.interstitial, callback, interPlaceID, key, addingTracking);
                }
                else
                {
                    Debug.LogError("Admob Interstitial Cannot Show");
                    mInterstitialAdCallback(false);
                }
            }
        }

        public override bool NeedReloadInter(AnalyticID.KeyAds key)
        {
            return !(interstitial[key] != null && interstitial[key].CanShowAd());
        }

        public void HandleOnAdLoaded(AnalyticID.KeyAds key)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                currentInterIndexID[key] = 0;
                retryAttemptInterAdmob[key] = 0;
                print("HandleAdLoaded event received");
            });
        }

        private void ReloadInterAd(AnalyticID.KeyAds key)
        {
            if (coroutinesInter[key] != null)
            {
                return;
            }

            currentInterIndexID[key]++;
            currentInterIndexID[key] %= interstitialIDs[key].Length;

            double retryDelay;

            if (retryAttemptInterAdmob[key] == 0)
            {
                retryDelay = 0;
            }
            else
            {
                retryDelay = Math.Pow(2, Math.Min(5, retryAttemptInterAdmob[key] - 1)) * GameToolSettings.timeRetryLoadAds;
            }

            retryAttemptInterAdmob[key]++;
            coroutinesInter[key] = this.DelayedCall((float)retryDelay, () =>
            {
                coroutinesInter[key] = null;
                LoadInterstitialInternal(key);
            });
        }

        public void HandleOnAdFailedToLoad(LoadAdError args, AnalyticID.KeyAds key)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                ReloadInterAd(key);
                TrackingManager.Instance.TrackEvent("InterstitialAd",
                    new Dictionary<string, object> { { "AdLoadFailed", mInterPlaceID.ToString() } });
                print("HandleOnAdFailedToLoad event received with message: " + args.GetMessage());
            });
        }

        public void HandleOnAdFailedToShow(AdError args, AnalyticID.KeyAds key)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                TrackingManager.Instance.TrackEvent("InterstitialAd",
                    new Dictionary<string, object> { { "AdFailedToDisplay", mInterPlaceID.ToString() } });
                ReloadInterAd(key);
                AudioListener.volume = 1;
                if (mInterstitialAdCallback != null)
                {
                    mInterstitialAdCallback(false);
                    mInterstitialAdCallback = null;
                }

                print("HandleOnAdFailedToShow event received with message: " + args.GetMessage());
            });
        }

        public void HandleOnAdOpening()
        {
            UnityMainThreadDispatcher.Instance.Enqueue(InterAdImpressionEvent);
        }

        public void HandleOnAdClosed(AnalyticID.KeyAds key)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() => InterAdCloseEvent(key));
        }

        private void HandleOnAdRevenuePaidEvent(AdValue adValue)
        {
            double revenue = adValue.Value / (double)1000000;
#if USE_ADJUST
            com.adjust.sdk.AdjustAdRevenue adRevenue =
 new com.adjust.sdk.AdjustAdRevenue(com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(revenue, adValue.CurrencyCode);
            com.adjust.sdk.Adjust.trackAdRevenue(adRevenue);
#endif
#if USE_FIREBASE
            TrackingManager.Instance.TrackFirebaseEvent("ad_revenue_sdk", new Dictionary<string, object>()
            {
                { FirebaseAnalytics.ParameterLevel, GameData.Instance.CurrentLevel.ToString() },
                { "ad_platform", "admod" },
                { "ad_format", "interstitial" },
                { "currency", adValue.CurrencyCode },
                { "value", revenue },
            });

            //rev tracking
            TrackingManager.Instance.TrackFirebaseEvent("ad_impression_admob", new Dictionary<string, object>()
            {
                { "ad_platform", "admob" },
                { "ad_format", "interstitial" },
                { "currency", "USD" },
                { "value", revenue }
            });
#endif

#if USE_FALCON
            new FAdLog(AdType.Interstitial, mInterPlaceID.ToString(), adValue.Precision.ToString(), adValue.CurrencyCode, adValue.Value, "admob", "admob").Send();
#endif

#if USE_AF
            System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
                System.Collections.Generic.Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "admod");

            TrackingManager.Instance.TrackAfRevenue("admod",
                AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, (double)(revenue),
                purchaseEvent);
#endif

#if USE_SINGULAR
            SingularAdData data = new SingularAdData("admod", "USD", revenue);
            SingularSDK.AdRevenue(data);
#endif

            print("HandleOnAdRevenuePaidEvent event received: " + revenue);
        }

        #endregion


        #region RewardAd


        public override void LoadRewardAd(AnalyticID.KeyAds key)
        {
            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            RewardedAd.Load(rewardVideoIDs[key][currentRewardIndexID[key]], adRequest,
                (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        HandleRewardedAdFailedToLoad(error, key);
                        return;
                    }

                    HandleRewardedAdLoaded(key);
                    rewardedAd[key] = ad;

                    rewardedAd[key].OnAdFullScreenContentOpened += HandleRewardedAdOpening;
                    rewardedAd[key].OnAdFullScreenContentFailed += HandleRewardedAdFailedToShow;
                    rewardedAd[key].OnAdFullScreenContentClosed += () => HandleRewardedAdClosed(key);
                    rewardedAd[key].OnAdPaid += HandleOnRewardedAdRevenuePaidEvent;
                });
        }

        public override bool IsRewardAdReady(AnalyticID.KeyAds key)
        {
            if (TestBackfill(AnalyticID.AdsType.rewarded, key))
            {
                return false;
            }

            return !NeedReloadReward(key);
        }

        public override bool NeedReloadReward(AnalyticID.KeyAds key)
        {
            if (rewardedAd == null)
                return true;
            return !rewardedAd[key].CanShowAd();
        }

        public override void ShowAdsReward(Action<bool> callback, AnalyticID.LocationTracking LocationTracking,
            AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null)
        {
            mHaveRewarded = false;
            mRewardedAdCallback = callback;
            mRewardPlaceID = LocationTracking;
            rewardDictTracking = addingTracking;
            if (!IsRewardAdReady(key))
            {
                if (NeedReloadReward(key))
                {
                    FreshLoadReward(key);
                }

                if (CanCallFallback(AnalyticID.AdsType.rewarded, callback, LocationTracking, key, addingTracking))
                {
                    CallFallback(AnalyticID.AdsType.rewarded, callback, LocationTracking, key, addingTracking);
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

            API.Instance.ShowLoading(() => { rewardedAd[key].Show(_ => { RewardAdRewardedEvent(); }); });
        }

        private void FreshLoadReward(AnalyticID.KeyAds key)
        {
            if (coroutinesReward[key] != null)
            {
                return;
            }

            currentRewardIndexID[key]++;
            currentRewardIndexID[key] %= rewardVideoIDs[key].Length;
            double retryDelay;
            if (retryAttemptRewardAdmob[key] == 0)
            {
                retryDelay = 0;
            }
            else
            {
                retryDelay = Math.Pow(2, Math.Min(5, retryAttemptRewardAdmob[key] - 1) * GameToolSettings.timeRetryLoadAds);
            }
            retryAttemptRewardAdmob[key]++;
            coroutinesReward[key] = this.DelayedCall((float)retryDelay, () =>
            {
                coroutinesReward[key] = null;
                LoadRewardAd(key);
            });
        }

        public void HandleRewardedAdLoaded(AnalyticID.KeyAds key)
        {
            currentRewardIndexID[key] = 0;
            retryAttemptRewardAdmob[key] = 0;
            print("HandleRewardedAdLoaded event received");
        }

        public void HandleRewardedAdFailedToLoad(AdError args, AnalyticID.KeyAds key)
        {
            TrackingManager.Instance.TrackEvent("RewardVideoAd",
                new Dictionary<string, object> { { "AdLoadFailed", mRewardPlaceID.ToString() } });
            FreshLoadReward(key);
            print(
                "HandleRewardedAdFailedToLoad event received with message: "
                + args.GetMessage());
        }

        public void HandleRewardedAdOpening()
        {
            UnityMainThreadDispatcher.Instance.Enqueue(RewardAdImpressionEvent);
        }

        public void HandleRewardedAdFailedToShow(AdError args)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(RewardAdShowFailedEvent);
        }

        public void HandleRewardedAdClosed(AnalyticID.KeyAds key)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() => RewardAdCloseEvent(key));
        }

        public void HandleUserEarnedReward(object sender, Reward args)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(RewardAdRewardedEvent);
        }

        private void HandleOnRewardedAdRevenuePaidEvent(AdValue adValue)
        {
            double revenue = adValue.Value / (double)1000000;
#if USE_ADJUST
            com.adjust.sdk.AdjustAdRevenue adRevenue =
 new com.adjust.sdk.AdjustAdRevenue(com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(revenue, adValue.CurrencyCode);
            com.adjust.sdk.Adjust.trackAdRevenue(adRevenue);
#endif
#if USE_FIREBASE
            TrackingManager.Instance.TrackFirebaseEvent("ad_revenue_sdk", new Dictionary<string, object>()
            {
                { FirebaseAnalytics.ParameterLevel, GameData.Instance.CurrentLevel.ToString() },
                { "ad_platform", "admod" },
                { "ad_format", "rewardads" },
                { "currency", adValue.CurrencyCode },
                { "value", revenue },
            });


            //rev tracking
            TrackingManager.Instance.TrackFirebaseEvent("ad_impression_admob", new Dictionary<string, object>()
            {
                { "ad_platform", "admob" },
                { "ad_format", "rewardads" },
                { "currency", "USD" },
                { "value", revenue },
            });
#endif
            
#if USE_FALCON
            new FAdLog(AdType.Reward, mRewardPlaceID.ToString(), adValue.Precision.ToString(), adValue.CurrencyCode, adValue.Value, "admob", "admob").Send();
#endif

#if USE_AF
            System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
                System.Collections.Generic.Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "admod");

            TrackingManager.Instance.TrackAfRevenue("admod",
                AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, (double)(revenue),
                purchaseEvent);
#endif
#if USE_SINGULAR
            SingularAdData data = new SingularAdData("admod", "USD", revenue);
            SingularSDK.AdRevenue(data);
#endif
        }

        #endregion

#endif
        public override MediationType MediationType => MediationType.Admob;
    }
}