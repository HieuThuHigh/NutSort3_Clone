using System.Collections;
using UnityEngine;

public static class ATT
{
    static bool isChecked = false;
    static bool _isOSready = false;

    public static bool IsOSReady()
    {
        if (!isChecked)
        {
            isChecked = true;
#if UNITY_IOS && !UNITY_EDITOR
            _isOSready = new System.Version(UnityEngine.iOS.Device.systemVersion) >= new System.Version("14.5");
#endif
        }
        return _isOSready;
    }

    public static IEnumerator CRRequestATTracking()
    {
        if (!IsOSReady()) yield break;
#if UNITY_IOS

        Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking();
        while (Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
               Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            yield return null;
        }
        var hasConsent = Unity.Advertisement.IosSupport.ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                         Unity.Advertisement.IosSupport.ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
        
        if (hasConsent)
        {
#if USE_APPLOVIN_ADS
            var cmpService = MaxSdk.CmpService;

            cmpService.ShowCmpForExistingUser(error =>
            {
                if (null == error)
                {
                    Debug.Log("The CMP alert was shown successfully");
                }
            });
#endif
        }
#endif
    }
}