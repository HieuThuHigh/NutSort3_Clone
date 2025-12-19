using System;
using System.Collections;
using System.Collections.Generic;
using DatdevUlts.Ults;
using GameTool.APIs.Analytics.Analytics;
using GameToolSample.APIs;
using GameToolSample.Scripts.Enum;
using UnityEngine;

namespace GameTool.APIs.Scripts.Ads
{
    public abstract class AdsManager : MonoBehaviour
    {
        [Header("DEBUG")]
        [SerializeField] protected bool _loadedMrec;

        private string _mRewardedItem;

        public string MRewardedItem
        {
            get => _mRewardedItem;
            set => _mRewardedItem = value;
        }

        public bool LoadedMrec => _loadedMrec;

        protected static GameToolSettings GameToolSettings => API.Instance.GameToolSetting;

        public abstract MediationType MediationType { get; }
        
        private static List<int> _listTimeReloadMrec = new List<int>(){2, 4, 8, 16, 32};


        [Tooltip("Thời gian show inter trước đó")]
        protected static float _lastTimeShowInterstitial = -999;
        [Tooltip("Thời gian show inter trước đó")]
        protected static float _lastTimeShowAoa = -999;


        [Tooltip("Giới hạn số lần load Inter")]
        protected int retryAttemptInter;

        [Tooltip("Giới hạn số lần load Reward")]
        protected int retryAttemptReward;

        [Tooltip("Giới hạn số lần load Banner")]
        protected int retryAttemptBanner;

        [Tooltip("Giới hạn số lần load Banner")]
        protected int retryAttemptMrec;


        [Tooltip("Vị trí show Inter: GameOver, NextLevel, Replay...")]
        protected AnalyticID.LocationTracking mInterPlaceID;

        [Tooltip("Tracking đi kèm Inter")] protected Dictionary<string, object> interDictTracking;

        protected Action<bool> mInterstitialAdCallback;


        [Tooltip("Vị trí show Reward: Skip Level, x2 Reward...")]
        protected AnalyticID.LocationTracking mRewardPlaceID;

        [Tooltip("Tracking đi kèm Reward")] protected Dictionary<string, object> rewardDictTracking;

        protected Action<bool> mRewardedAdCallback;

        [Tooltip("Check user đã xem hết toàn bộ reward ads chưa để trả thưởng.")]
        protected bool mHaveRewarded;


        [Tooltip("Các config của Banner, tương tự với các adformat khác.")]
        protected BannerPosition mBannerPosition = BannerPosition.Bottom;

        protected AnalyticID.LocationTracking mBannerPlaceID;
        protected Dictionary<string, object> bannerDictTracking;
        protected bool bannerLoaded = false;

        protected bool _canShowBanner;
        protected bool isShowingBanner;
        protected bool isForceHideBanner;
        protected bool bannerIsNormal;
        protected bool adsInitializationCompleted;

        protected Coroutine _coroutineReloadMrec;

        protected AnalyticID.LocationTracking mMrecPlaceID;
        protected bool mIsMrecShowing;
   
        public virtual bool IsShowingBanner => isShowingBanner;

        public bool CanShowBanner
        {
            get => _canShowBanner;
            set => _canShowBanner = value;
        }

        public virtual void Init()
        {
        }

        public virtual void ShowAdsBanner(BannerPosition position, AnalyticID.LocationTracking bannerPlaceID,
            AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null, bool normalBanner = true)
        {
            bannerIsNormal = normalBanner;
        }

        public virtual void HideAdsBanner()
        {
        }

        public virtual bool IsMrecAdReady()
        {
            return false;
        }

        public virtual void ShowAdsReward(Action<bool> callback, AnalyticID.LocationTracking LocationTracking,
            AnalyticID.KeyAds key = AnalyticID.KeyAds.none,
            Dictionary<string, object> addingTracking = null)
        {
        }

        protected enum LogType
        {
            Debug,
            Warning,
            Error
        }

        #region MREC

        public virtual void ShowMrec(AnalyticID.LocationTracking mrecPlaceID)
        {
            mIsMrecShowing = true;
            mMrecPlaceID =  mrecPlaceID;
            Debug.Log("ShowMrec");
        }

        public virtual void HideMrec()
        {
            mIsMrecShowing = false;
            Debug.Log("HideMrec");
        }

        public virtual void LoadMrec()
        {
            Debug.Log("LoadMrec");
            _coroutineReloadMrec = null;
        }

        protected virtual void MrecAdCloseEvent()
        {
            Debug.Log("MrecAdCloseEvent");
            LoadMrec();
        }

        protected virtual void MrecAdLoadFailEvent()
        {
            Debug.Log("MrecAdLoadFailEvent");
            _loadedMrec = false;
            retryAttemptMrec++;
            if (retryAttemptMrec >= _listTimeReloadMrec.Count)
            {
                retryAttemptMrec = _listTimeReloadMrec.Count;
            }
            double retryDelay = _listTimeReloadMrec[retryAttemptMrec - 1];

            if (_coroutineReloadMrec != null)
            {
                return;
            }

            _coroutineReloadMrec = this.DelayedCall((float)retryDelay, () => LoadMrec(), true);
        }

        protected virtual void MrecAdShowFailEvent()
        {
            Debug.Log("MrecAdShowFailEvent");
            retryAttemptMrec++;
            if (retryAttemptMrec >= _listTimeReloadMrec.Count)
            {
                retryAttemptMrec = _listTimeReloadMrec.Count;
            }
            double retryDelay = _listTimeReloadMrec[retryAttemptMrec - 1];

            if (_coroutineReloadMrec != null)
            {
                return;
            }

            _coroutineReloadMrec = this.DelayedCall((float)retryDelay, () => LoadMrec());
        }

        protected virtual void MrecAdClickEvent()
        {
            Debug.Log("MrecAdClickEvent");
        }

        protected virtual void MrecAdLoadedEvent()
        {
            Debug.Log("MrecAdLoadedEvent");
            retryAttemptMrec = 0;
            _loadedMrec = true;
        }
        #endregion

        #region Banner

        public virtual void LoadBannerAd()
        {
        }

        //Banner ghi nhận sự kiện Impression
        protected void BannerAdImpressionEvent()
        {
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.banner, AnalyticID.MonetizationState.show_success,
                mBannerPlaceID,
                bannerDictTracking);
            //TrackingManager.Instance.TrackEvent("Banner_Impression");
            //TrackingManager.Instance.TrackLimitedEvent("Banner_Impression");
        }

        protected void BannerAdCloseEvent()
        {
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.banner, AnalyticID.MonetizationState.close,
                mBannerPlaceID, bannerDictTracking);
            double retryDelay = GameToolSettings.timeRetryLoadAds * Math.Pow(2, Math.Min(5, retryAttemptBanner));
            retryAttemptBanner++;

            this.DelayedCall((float)retryDelay, LoadBannerAd);
            LoadBannerAd();
        }

        protected void BannerAdLoadFailEvent()
        {
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.banner, AnalyticID.MonetizationState.load_failed,
                mBannerPlaceID,
                bannerDictTracking);
            double retryDelay = GameToolSettings.timeRetryLoadAds * Math.Pow(2, Math.Min(5, retryAttemptBanner));
            retryAttemptBanner++;

            this.DelayedCall((float)retryDelay, LoadBannerAd);
            LoadBannerAd();
        }

        protected void BannerAdShowFailEvent()
        {
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.banner, AnalyticID.MonetizationState.show_failed,
                mBannerPlaceID,
                bannerDictTracking);

            double retryDelay = GameToolSettings.timeRetryLoadAds * Math.Pow(2, Math.Min(5, retryAttemptBanner));
            retryAttemptBanner++;

            this.DelayedCall((float)retryDelay, LoadBannerAd);
            LoadBannerAd();
        }

        protected void BannerAdClickEvent()
        {
            APIHandle.BannerAdClick();
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.banner, AnalyticID.MonetizationState.click,
                mBannerPlaceID, bannerDictTracking);
        }

        protected void BannerAdLoadedEvent()
        {
            retryAttemptBanner = 0;
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.banner, AnalyticID.MonetizationState.loaded,
                mBannerPlaceID, bannerDictTracking);
        }

        #endregion

        #region Reward

        public virtual bool IsRewardAdReady(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            return false;
        }

        public virtual void LoadRewardAd(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
        }

        protected void RewardAdImpressionEvent()
        {
            if (AppOpenAdManager.IsInstanceValid())
            {
                AppOpenAdManager.Instance.ResetTimeShowAds();
            }

            _lastTimeShowInterstitial = Time.time;

            AudioListener.volume = 0;
            APIHandle.RewardAdImpressionEvent();
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.show_success,
                mRewardPlaceID,
                rewardDictTracking);
        }

        protected void RewardAdCloseEvent(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            AudioListener.volume = 1;
            StartCoroutine(CheckRewardAds());
            LoadRewardAd(key);
            TrackingManager.Instance.CloseAdsEvent();
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.close,
                mRewardPlaceID, rewardDictTracking);
        }

        IEnumerator CheckRewardAds()
        {
            yield return new WaitForSecondsRealtime(.1f);
            if (mRewardedAdCallback != null)
                mRewardedAdCallback(mHaveRewarded);

        }

        protected void RewardAdRewardedEvent()
        {
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.completed,
                mRewardPlaceID,
                rewardDictTracking);
            mHaveRewarded = true;
            APIHandle.RewardAdRewardedEvent();
        }

        protected void RewardAdClickedEvent()
        {
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.click,
                mRewardPlaceID, rewardDictTracking);
            APIHandle.RewardAdClickedEvent();
        }

        protected void RewardAdShowFailedEvent()
        {
            TrackingManager.Instance.CloseAdsEvent();
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.show_failed,
                mRewardPlaceID,
                rewardDictTracking);
            APIHandle.RewardAdShowFailedEvent();
            double retryDelay = GameToolSettings.timeRetryLoadAds * Math.Pow(2, Math.Min(5, retryAttemptReward));
            retryAttemptReward++;

            this.DelayedCall((float)retryDelay, () => LoadRewardAd());
            AudioListener.volume = 1;
            if (mRewardedAdCallback != null)
            {
                mRewardedAdCallback(false);
                mRewardedAdCallback = null;
            }
        }

        protected void RewardAdLoadFailedEvent()
        {
            APIHandle.RewardAdLoadFailEvent();
            double retryDelay = GameToolSettings.timeRetryLoadAds * Math.Pow(2, Math.Min(5, retryAttemptReward));
            retryAttemptReward++;

            this.DelayedCall((float)retryDelay, () => LoadRewardAd());
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.load_failed,
                mRewardPlaceID,
                rewardDictTracking);
        }

        protected void RewardAdLoadedEvent()
        {
            retryAttemptReward = 0;
            APIHandle.RewardAdLoadedEvent();
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.rewarded, AnalyticID.MonetizationState.loaded,
                mRewardPlaceID, rewardDictTracking);
        }

        #endregion

        #region Inter

        protected bool startAds;

        public virtual void ShowAdsInterstitial(Action<bool> callback, AnalyticID.LocationTracking interPlaceID,
            AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null, bool ignoreCappingTime = false)
        {
        }

        public virtual bool IsInterAdReady(AnalyticID.KeyAds key = AnalyticID.KeyAds.none, bool ignoreCappingTime = false)
        {
            return false;
        }

        public virtual bool CanShowFull(bool ignoreCappingTime = false)
        {
            if (!startAds && !ignoreCappingTime)
            {
                if (Time.time >= 30)
                {
                    startAds = true;
                    return true;
                }
                
                return false;
            }

            if (ignoreCappingTime)
            {
                return Time.time - _lastTimeShowInterstitial >= 15;
            }

            var capping = 30;
            return Time.time - _lastTimeShowInterstitial >= capping;
        }

        public virtual bool CanShowAoa()
        {
            return Time.time - _lastTimeShowAoa >= 15;
        }

        public void ResetTimeShowInterstitial()
        {
            _lastTimeShowInterstitial = Time.time;
            _lastTimeShowAoa = Time.time;
        }

        protected virtual void LoadInterstitial(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
        }

        protected void InterAdImpressionEvent()
        {
            if (AppOpenAdManager.IsInstanceValid())
            {
                AppOpenAdManager.Instance.ResetTimeShowAds();
            }

            AudioListener.volume = 0;
            retryAttemptInter = 0;
            ResetTimeShowInterstitial();
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial,
                AnalyticID.MonetizationState.show_success, mInterPlaceID,
                interDictTracking);
            //TrackingManager.Instance.TrackEvent("Inter_Impression");
            //TrackingManager.Instance.TrackEvent("Ads_Impression");
            //TrackingManager.Instance.TrackLimitedEvent("Inter_Impression");
            //TrackingManager.Instance.TrackLimitedEvent("Ads_Impression");
            APIHandle.InterAdImpressionEvent();
        }

        protected void InterAdCloseEvent(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            APIHandle.InterAdCloseEvent(mInterPlaceID.ToString().ToLower());
            TrackingManager.Instance.CloseAdsEvent();
            AudioListener.volume = 1;
            LoadInterstitial(key);
            Debug.LogError("Check callback inter");
            if (mInterstitialAdCallback != null)
            {
                Debug.LogError("Call callback inter");
                mInterstitialAdCallback(true);
            }

            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial, AnalyticID.MonetizationState.close,
                mInterPlaceID, interDictTracking);
        }

        protected void InterAdLoadedEvent()
        {
            retryAttemptInter = 0;
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial, AnalyticID.MonetizationState.loaded,
                mInterPlaceID, interDictTracking);
            APIHandle.InterAdLoadedEvent();
        }

        protected void InterAdLoadFailEvent(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            double retryDelay = GameToolSettings.timeRetryLoadAds * Math.Pow(2, Math.Min(5, retryAttemptInter));
            retryAttemptInter++;

            this.DelayedCall((float)retryDelay, () => LoadInterstitial(key));
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial, AnalyticID.MonetizationState.load_failed,
                mInterPlaceID,
                interDictTracking);
            APIHandle.InterAdLoadFailEvent();
        }

        protected void InterAdShowFailEvent(AnalyticID.KeyAds key = AnalyticID.KeyAds.none)
        {
            APIHandle.InterAdShowFailEvent(mInterPlaceID.ToString().ToLower());
            TrackingManager.Instance.CloseAdsEvent();
            AudioListener.volume = 1;

            if (mInterstitialAdCallback != null)
            {
                mInterstitialAdCallback(true);
            }

            double retryDelay = GameToolSettings.timeRetryLoadAds * Math.Pow(2, Math.Min(5, retryAttemptInter));
            retryAttemptInter++;

            this.DelayedCall((float)retryDelay, () => LoadInterstitial(key));
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial, AnalyticID.MonetizationState.show_failed,
                mInterPlaceID,
                interDictTracking);
        }

        protected void InterAdClickEvent()
        {
            //TrackingManager.Instance.TrackLimitedEvent("Inter_Click");
            //TrackingManager.Instance.TrackLimitedEvent("Ads_Click");
            //TrackingManager.Instance.TrackEvent("Inter_Click");
            //TrackingManager.Instance.TrackEvent("Ads_Click");
            TrackingManager.Instance.TrackAds(AnalyticID.AdsType.interstitial, AnalyticID.MonetizationState.click,
                mInterPlaceID, interDictTracking);
            APIHandle.InterAdClickEvent();
        }

        #endregion

        protected void AdsLog(string logContent, LogType maxLogType = LogType.Debug)
        {
            switch (maxLogType)
            {
                case LogType.Debug:
                    Debug.Log(logContent);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(logContent);
                    break;
                case LogType.Error:
                    Debug.LogWarning(logContent);
                    break;
            }
        }

        private bool CanCallFallback(AnalyticID.AdsType adsType, bool call, Action<bool> callback,
            AnalyticID.LocationTracking LocationTracking, AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null)
        {
            AdsManager manager = GameToolSettings.GetAdsFullManagerBackFill(MediationType, adsType, out var nextAds);

            if (manager)
            {
                switch (nextAds)
                {
                    case AnalyticID.AdsType.rewarded:
                        if (manager.IsRewardAdReady(key))
                        {
                            if (call)
                            {
                                manager.ShowAdsReward(callback, LocationTracking, key, addingTracking);
                            }

                            return true;
                        }

                        return CanCallFallback(AnalyticID.AdsType.rewarded, call, callback, LocationTracking, key,
                            addingTracking);
                    case AnalyticID.AdsType.interstitial:
                        if (manager.IsInterAdReady(key))
                        {
                            if (call)
                            {
                                manager.ShowAdsInterstitial(callback, LocationTracking, key, addingTracking);
                            }

                            return true;
                        }

                        return CanCallFallback(AnalyticID.AdsType.interstitial, call, callback, LocationTracking, key,
                            addingTracking);
                    default:
                        return false;
                }
            }

            return false;
        }

        public bool CanCallFallback(AnalyticID.AdsType adsType, Action<bool> callback,
            AnalyticID.LocationTracking LocationTracking, AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null)
        {
            return CanCallFallback(adsType, false, callback, LocationTracking, key, addingTracking);
        }

        public bool CallFallback(AnalyticID.AdsType adsType, Action<bool> callback,
            AnalyticID.LocationTracking LocationTracking, AnalyticID.KeyAds key,
            Dictionary<string, object> addingTracking = null)
        {
            return CanCallFallback(adsType, true, callback, LocationTracking, key, addingTracking);
        }

        public bool TestBackfill(AnalyticID.AdsType adsType, AnalyticID.KeyAds key)
        {
            if (API.Instance.IsTestBackfill)
            {
                return CanCallFallback(adsType, null, AnalyticID.LocationTracking.none, key);
            }

            return false;
        }

        public virtual bool NeedReloadReward(AnalyticID.KeyAds key)
        {
            return false;
        }

        public virtual bool NeedReloadInter(AnalyticID.KeyAds key)
        {
            return false;
        }
    }

    public enum BannerPosition
    {
        Top = 0,
        Bottom = 1,
        TopLeft = 2,
        TopRight = 3,
        BottomLeft = 4,
        BottomRight = 5,
        Center = 6
    }
}