using System;
using System.Collections.Generic;
using System.Linq;
using DatdevUlts.InspectorUtils;
using GameTool.APIs.Scripts.Ads;
using GameTool.Assistants.DictionarySerialize;
using GameToolSample.Scripts.Enum;
using UnityEditor;
using UnityEngine;

namespace GameTool.APIs.Scripts
{
    [CreateAssetMenu(fileName = "GameToolSettings", menuName = "GameTool/GameToolSettings", order = 1)]
    public class GameToolSettings : ScriptableObject
    {
        public static GameToolSettings Instance => APIPlayerSetting.Instance.GameToolSetting;

        [HideInNormalInspector]
        public string buildVersion;

        [Header("DEFINES")]
        public bool useSpine;

        public bool useGGReview;
        public bool useLocalNotification;
        public bool useFirebase;
        public bool useIap;
        public bool useFirebaseIAP;


        [Header("MMP")]
        public MMPType mmpType = MMPType.None;

        public string adjustAppToken;

        public string afDevKey;
        public string afAppID;

        public string singularAPIKey;
        public string singularAPISecret;


        [Header("MEDIATION")]
        [Header("ADS FULL")]
        public List<AdsTypeElement> listAdsFullMediation = new List<AdsTypeElement>();

        public bool useBackfill;

        public MediationType bannerMediation;
        public bool useAdmobBanner;
        public MediationType aoaMediation;
        public MediationType nativeMediation;
        public MediationType mrecMediation;
        public bool useAdmobNative;

        [Tooltip("Sau khi load fail thì chờ 1 giây, 2 giây, 4 giây, ... * timeRetryLoadAds")]
        public float timeRetryLoadAds = 1;

        [Header("ADVERTY")]
        public bool useAdverty;


        [Header("ADMOB")]
        public string admobAppID;

        public Dict<AnalyticID.KeyAds,string[]> admobAppOpenID = new Dict<AnalyticID.KeyAds, string[]>();
        public Dict<AnalyticID.KeyAds,string[]> admobNativeID = new Dict<AnalyticID.KeyAds, string[]>();
        public Dict<AnalyticID.KeyAds,string[]> admobBannerID = new Dict<AnalyticID.KeyAds, string[]>();
        public Dict<AnalyticID.KeyAds,string[]> admobCollapsibleBannerID = new Dict<AnalyticID.KeyAds, string[]>();
        public Dict<AnalyticID.KeyAds,string[]> admobInterstitialID = new Dict<AnalyticID.KeyAds, string[]>();
        public Dict<AnalyticID.KeyAds,string[]> admobRewardVideoID = new Dict<AnalyticID.KeyAds, string[]>();


        [Header("IRON SOURCES")]
        public string irsAppKey;


        [Header("MAX SDK")]
        public string applovinSDKKey;

        public string applovinBannerID;
        public string applovinInterstitialID;
        public string applovinRewardVideoID;
        public string applovinAppOpenID;
        public string applovinMrecID;


        public string GetBuildVersion()
        {
#if UNITY_EDITOR
            return Application.platform == RuntimePlatform.IPhonePlayer
                ? PlayerSettings.iOS.buildNumber
                : PlayerSettings.Android.bundleVersionCode.ToString();
#else
            return buildVersion;
#endif
        }

        public bool IsUseAnyMediation(MediationType mediationType)
        {
            var full = listAdsFullMediation.Any(element => element.MediationType == mediationType);
            var banner = bannerMediation == mediationType;
            var aoa = aoaMediation == mediationType;
            var native = nativeMediation == mediationType;
            var mrec = mrecMediation == mediationType;

            if (mediationType == MediationType.Admob)
            {
                if (useAdmobNative)
                {
                    native = true;
                }
                
                if (useAdmobBanner)
                {
                    banner = true;
                }
            }

            return full || banner || aoa || native || mrec;
        }

        public bool IsUseAnyFull(MediationType mediationType)
        {
            var full = listAdsFullMediation.Any(element => element.MediationType == mediationType);

            return full;
        }

        public bool IsUseAnyMediation()
        {
            return IsUseAnyMediation(MediationType.Admob) || IsUseAnyMediation(MediationType.IronSource) || IsUseAnyMediation(MediationType.Applovin);
        }

        public bool IsUseAnyFull()
        {
            return IsUseAnyFull(MediationType.Admob) || IsUseAnyFull(MediationType.IronSource) || IsUseAnyFull(MediationType.Applovin);
        }

        public AdsManager GetManagerByMediation(MediationType mediationType)
        {
            return API.Instance.ListAdsManager.Find(manager => manager.MediationType == mediationType);
        }

        public AdsManager GetAdsFullManagerBackFill(MediationType mediationType, AnalyticID.AdsType adsType, out AnalyticID.AdsType outAdsType)
        {
            outAdsType = AnalyticID.AdsType.none;
            var index = listAdsFullMediation.FindIndex(element => element.MediationType == mediationType && element.AdsType == adsType);
            if (!useBackfill || index < 0 || index + 1 >= listAdsFullMediation.Count)
            {
                return null;
            }

            outAdsType = listAdsFullMediation[index + 1].AdsType;
            return GetManagerByMediation(listAdsFullMediation[index + 1].MediationType);
        }

        public AdsManager GetAdsFullManager(AnalyticID.AdsType adsType)
        {
            MediationType m = MediationType.None;
            try
            {
                m = listAdsFullMediation.Find(element => element.AdsType == adsType).MediationType;
            }
            catch
            {
                // ignored
            }

            if (m == MediationType.None)
            {
                return null;
            }
            
            return GetManagerByMediation(m);
        }
    }

    public enum MediationType
    {
        None,
        Admob,
        Applovin,
        IronSource
    }

    public enum MMPType
    {
        None,
        Appsflyer,
        Adjust,
        Singular
    }

    [Serializable]
    public class AdsTypeElement
    {
        public MediationType MediationType;
        public AnalyticID.AdsType AdsType;
    }
}