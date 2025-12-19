#if !Minify

using System.Collections.Generic;
using UnityEngine;
#if USE_AF
using AppsFlyerSDK;
using GameTool.APIs.Scripts;
#endif
namespace GameTool.APIs.Analytics
{
    namespace Analytics
    {
#if USE_AF
    
            public class AFAnalytics : MonoBehaviour, IAnalytic, IAppsFlyerConversionData
            {
    
                public bool InitSuccess { get; set; }
    
                string mUDID;
                public void ApplicationOnPause(bool Paused)
                {
    
                }
    
                public void Init(params string[] args)
                {
                    AppsFlyerSDK.AppsFlyer.initSDK(args[0], args[1]);
#if USE_FALCON
                    AppsFlyerSDK.AppsFlyer.setCustomerUserId(Falcon.FalconCore.Scripts.Repositories.News.FDeviceInfoRepo.DeviceId);
#endif
                    AppsFlyerSDK.AppsFlyer.startSDK();
                    AppsFlyerSDK.AppsFlyer.setIsDebug(API.Instance.IsDebug);
                    AppsFlyerAdRevenue.start();
                    AppsFlyerAdRevenue.setIsDebug(API.Instance.IsDebug);
                }
    
                public void TrackEvent(string eventName, Dictionary<string, object> parameters)
                {
                    System.Collections.Generic.Dictionary<string, string> events = new
            System.Collections.Generic.Dictionary<string, string>();
    
                    int index = 0;
                    foreach (KeyValuePair<string, object> kv in parameters)
                    {
                        events.Add(kv.Key.ToString(), kv.Value.ToString());
                        Debug.Log("AFAnalytics: " + eventName + "- Parameters: " + kv.Key + "--" + kv.Value);
                        index++;
                    }
    
                    AppsFlyerSDK.AppsFlyer.sendEvent(eventName, events);
                }
    
                public void TrackEvent(string eventName)
                {
                    AppsFlyerSDK.AppsFlyer.sendEvent(eventName, null);
                    Debug.Log("AppsFlyer: " + eventName);
                }
                // Mark AppsFlyer CallBacks
                public void onConversionDataSuccess(string conversionData)
                {
                    AppsFlyer.AFLog("didReceiveConversionData", conversionData);
                    Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
                    // add deferred deeplink logic here
                }
    
                public void onConversionDataFail(string error)
                {
                    AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
                }
    
                public void onAppOpenAttribution(string attributionData)
                {
                    AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
                    Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
                    // add direct deeplink logic here
                }
    
                public void onAppOpenAttributionFailure(string error)
                {
                    AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
                }
    
    
            }
#endif
    }
}
#endif