#if UNITY_IOS

using System.Runtime.InteropServices;

namespace AudienceNetwork
{
    public static class AdSettings
    {
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
        }
    }
}

#endif