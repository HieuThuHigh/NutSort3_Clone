namespace GameTool.APIs.Analytics
{
    #if !Minify
    namespace Analytics
    {
#if USE_SINGULAR
    
            public class SingularAnalytics : IAnalytic
            {
    
                public bool InitSuccess { get; set; }
    
                string mUDID;
                public void ApplicationOnPause(bool Paused)
                {
    
                }
    
                public void Init(params string[] args)
                {
                    SingularSDK.InitializeSingularSDK();
                }
    
                public void TrackEvent(string eventName, Dictionary<string, object> parameters)
                {
    
                }
    
                public void TrackEvent(string eventName)
                {
                    SingularSDK.Event(eventName);
    
                    Debug.Log("SingularSDK: "+eventName);
                }
            }
#endif
    }
#endif
}