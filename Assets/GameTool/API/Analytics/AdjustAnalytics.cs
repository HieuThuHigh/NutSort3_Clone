#if !Minify
namespace GameTool.APIs.Analytics
{
    namespace Analytics
    {
#if USE_ADJUST
            using com.adjust.sdk;
    
            public class AdjustAnalytics : IAnalytic
            {
    
                public bool InitSuccess { get; set; }
    
                string mUDID;
                public void ApplicationOnPause(bool Paused)
                {
    
                }
    
                public void Init(params string[] args)
                {
                    AdjustConfig adjustConfig = new AdjustConfig(args[0],AdjustEnvironment.Production);
                    adjustConfig.setLogLevel(AdjustLogLevel.Suppress);
                    adjustConfig.setSendInBackground(false);
                    adjustConfig.setEventBufferingEnabled(false);
                    adjustConfig.setLaunchDeferredDeeplink(true);
                    Adjust.start(adjustConfig);
                }
    
                public void TrackEvent(string eventName, Dictionary<string, object> parameters)
                {
    
                }
    
                public void TrackEvent(string eventName)
                {
                    AdjustEvent adjustEvent = new AdjustEvent(eventName);
                    Adjust.trackEvent(adjustEvent);
    
                    Debug.Log("AdjustAnalytics: "+eventName);
                }
            }
#endif
    }
}
#endif