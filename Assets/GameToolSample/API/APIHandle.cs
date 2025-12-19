using GameTool.APIs.Analytics.Analytics;

namespace GameToolSample.APIs
{
    public static class APIHandle
    {
        public static void InterAdImpressionEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackAfEvent("af_inters_displayed");
            TrackingManager.Instance.TrackFirebaseEvent("ad_inter_show");
#endif
        }

        public static void RewardAdImpressionEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackAfEvent("af_rewarded_displayed");
            TrackingManager.Instance.TrackFirebaseEvent("ads_reward_show_success");
#endif
        }

        public static void ShowReward()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackAfEvent("af_rewarded_show");
#endif
        }

        public static void ShowFull()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackAfEvent("af_inters_show");
#endif
        }

        public static void InterAdLoad()
        {
        }

        public static void RewardAdLoad()
        {
        }

        public static void RewardAdLoadedEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackFirebaseEvent("ads_reward_load");
#endif
        }

        public static void RewardAdClickedEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackFirebaseEvent("ads_reward_click");
#endif
        }

        public static void RewardAdShowFailedEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackFirebaseEvent("ads_reward_show_fail");
#endif
        }

        public static void RewardAdRewardedEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackFirebaseEvent("ads_reward_complete");
#endif
        }

        public static void InterAdClickEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackFirebaseEvent("ad_inter_click");
#endif
        }

        public static void InterAdLoadedEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackFirebaseEvent("ad_inter_load_success");
#endif
        }

        public static void InterAdLoadFailEvent()
        {
#if USE_FALCON
            TrackingManager.Instance.TrackFirebaseEvent("ad_inter_load_fail");
#endif
        }

        public static bool IsRemoveAds => false;

        public static bool CanShowInter => true;

        public static bool CanShowBanner => true;

        public static bool CanShowNative => true;

        public static bool CanShowMrec => true;

        public static bool CanShowAOA => true;

        public static bool FailMrecApplovin { get; set; } = false;
        public static bool FailAoaApplovin { get; set; } = false;

        public static void InitAdsDone()
        {
        }

        public static void InterAdShowFailEvent(string location)
        {
        }

        public static void InterAdCloseEvent(string location)
        {
        }

        public static void RewardAdLoadFailEvent()
        {
        }

        public static void BannerAdClick()
        {
            TrackingManager.Instance.TrackFirebaseEvent("bannerads_click");
        }

        public static void BannerAdShow()
        {
            TrackingManager.Instance.TrackFirebaseEvent("bannerads_show");
        }
    }
}