using DatdevUlts.InspectorUtils;
using DatdevUlts.Ults;
using GameTool.APIs.Analytics.Analytics;
using GameTool.Assistants.DesignPattern;

#if USE_ADMOB_NATIVE_ADS
using System;
using System.Collections.Generic;
using GameTool.Assistants.DictionarySerialize;
using GameToolSample.GameDataScripts.Scripts;
using GameToolSample.Scripts.Enum;
using UnityEngine;
using GoogleMobileAds.Api;
#endif

namespace GameTool.APIs.Scripts.Ads
{
    public class NativeAdsAdmod : SingletonMonoBehaviour<NativeAdsAdmod>
    {
#if USE_ADMOB_NATIVE_ADS
        [HideInNormalInspector] public Dict<AnalyticID.KeyAds, bool> nativeLoaded = new Dict<AnalyticID.KeyAds, bool>();
        private Dict<AnalyticID.KeyAds, int> currentNativeIndexID = new Dict<AnalyticID.KeyAds, int>();
        [HideInNormalInspector] public Dict<AnalyticID.KeyAds, NativeAd> adNative = new Dict<AnalyticID.KeyAds, NativeAd>();

        private Dict<AnalyticID.KeyAds, string[]> admobNativeID => _gameToolSettings.admobNativeID;
        private GameToolSettings _gameToolSettings => GameToolSettings.Instance;

        protected override void Awake()
        {
            base.Awake();
            foreach (var data in admobNativeID)
            {
                nativeLoaded.Add(data.Key, false);
                currentNativeIndexID.Add(data.Key, 0);
                adNative.Add(data.Key, null);
            }
        }

        private void Start()
        {
            foreach (var data in admobNativeID)
            {
                RequestNativeAd(data.Key);
            }
        }

        public void RequestNativeAd(AnalyticID.KeyAds key, Action<NativeAd> onLoaded = null, Action onFailed = null,
            bool autoReload = true)
        {
            if (!API.Instance.IsRemoveAds)
            {
                AdLoader adLoader = new AdLoader.Builder(admobNativeID[key][currentNativeIndexID[key]])
                    .ForNativeAd()
                    .Build();
                adLoader.OnNativeAdLoaded += (sender, args) => HandleNativeAdLoaded(sender, args, key);
                adLoader.OnNativeAdLoaded += (_, args) => { onLoaded?.Invoke(args.nativeAd); };

                adLoader.OnAdFailedToLoad += (_, _) =>
                {
                    onFailed?.Invoke();
                    if (autoReload)
                    {
                        Reload(key, onLoaded, onFailed);
                    }
                };

                adLoader.LoadAd(new AdRequest());
            }
        }

        private void Reload(AnalyticID.KeyAds key, Action<NativeAd> onLoaded = null, Action onFailed = null)
        {
            this.DelayedCall(1f, () =>
            {
                if (currentNativeIndexID[key] < admobNativeID[key].Length - 1)
                {
                    currentNativeIndexID[key]++;
                }
                else
                {
                    currentNativeIndexID[key] = 0;
                }

                RequestNativeAd(key, onLoaded, onFailed);
            });
        }

        private void HandleNativeAdLoaded(object sender, NativeAdEventArgs args, AnalyticID.KeyAds key)
        {
            currentNativeIndexID[key] = 0;
            Debug.Log("Native ad loaded.");
            adNative[key] = args.nativeAd;
            nativeLoaded[key] = true;

            adNative[key].OnPaidEvent += HandleNativeAdPaid;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private void HandleNativeAdPaid(object sender, AdValueEventArgs args)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            double revenue = args.AdValue.Value / (double)1000000;

#if USE_FIREBASE
            TrackingManager.Instance.TrackFirebaseEvent("ad_revenue_sdk", new Dictionary<string, object>()
            {
                {
                    Firebase.Analytics.FirebaseAnalytics.ParameterLevel, (GameData.Instance.CurrentLevel + 1).ToString()
                },
                { "ad_platform", "admob" },
                { "ad_format", "nativeads" },
                { "currency", "USD" },
                { "value", revenue },
            });

            TrackingManager.Instance.TrackFirebaseEvent("ad_impression_admob", new Dictionary<string, object>()
            {
                { "ad_platform", "admob" },
                { "ad_format", "nativeads" },
                { "currency", "USD" },
                { "value", revenue },
            });
#endif

#if USE_AF
            System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
                System.Collections.Generic.Dictionary<string, string>();
            purchaseEvent.Add("ad_platform", "admod");
            purchaseEvent.Add("ad_unit", "native_ads");
            TrackingManager.Instance.TrackAfRevenue("admod",
                AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob, (double)(revenue),
                purchaseEvent);
#endif

#if USE_ADJUST
            AdValue adValue = args.AdValue;
            // send ad revenue info to Adjust
            com.adjust.sdk.AdjustAdRevenue adRevenue =
 new com.adjust.sdk.AdjustAdRevenue(com.adjust.sdk.AdjustConfig.AdjustAdRevenueSourceAdMob);
            adRevenue.setRevenue(revenue, adValue.CurrencyCode);
            com.adjust.sdk.Adjust.trackAdRevenue(adRevenue);
#endif
        }
#endif
    }
}