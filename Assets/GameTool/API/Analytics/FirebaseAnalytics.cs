#if !Minify
using System.Collections.Generic;

#if USE_FIREBASE
using GameTool.FirebaseService;
#endif

using UnityEngine;

namespace GameTool.APIs.Analytics
{
    namespace Analytics
    {
        public class FirebaseAnalytics : IAnalytic
        {
            public bool InitSuccess { get; set; }

            string mUDID;

            public void ApplicationOnPause(bool Paused) { }

            public void Init(params string[] args)
            {
#if USE_FIREBASE
                FirebaseService.FirebaseInstance.CheckAndTryInit(() =>
                {
                    Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#if USE_FALCON
                    Firebase.Analytics.FirebaseAnalytics.SetUserId(Falcon.FalconCore.Scripts.Repositories.News.FDeviceInfoRepo.DeviceId);
#else
                    mUDID = SystemInfo.deviceUniqueIdentifier;
                    Firebase.Analytics.FirebaseAnalytics.SetUserId(mUDID);
#endif
                    //  Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
                    InitSuccess = true;
                    Debug.Log("FirebaseAnalytics Inited");
                });
#endif
            }

            public void TrackEvent(string eventName, Dictionary<string, object> parameters)
            {
#if USE_FIREBASE
                if (!FirebaseInstance.HasInstance)
                {
                    return;
                }
                
                Firebase.Analytics.Parameter[] fireBaseParameters = new Firebase.Analytics.Parameter[parameters.Count];

                int index = 0;
                string log = $"FirebaseAnalytics: Event name: {eventName}\n";
                foreach (KeyValuePair<string, object> kv in parameters)
                {
                    fireBaseParameters[index] = ParseParameter(kv.Key, kv.Value);
                    log += $"\n{kv.Key} : {kv.Value}";
                    index++;
                }
                Debug.Log(log);
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, fireBaseParameters);
#endif
            }

            public void TrackEvent(string eventName)
            {
#if USE_FIREBASE
                if (!FirebaseInstance.HasInstance)
                {
                    return;
                }
                Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName);

                Debug.Log("FirebaseAnalytics: " + eventName);
#endif
            }
#if USE_FIREBASE
            Firebase.Analytics.Parameter ParseParameter(string paramName, object paramValue)
            {

                if (paramValue is string)
                {
                    return new Firebase.Analytics.Parameter(paramName, paramValue as string);
                }

                if (paramValue is float)
                {
                    return new Firebase.Analytics.Parameter(paramName, (float)paramValue);
                }

                if (paramValue is double)
                {
                    return new Firebase.Analytics.Parameter(paramName, (double)paramValue);
                }

                if (paramValue is decimal)
                {
                    return new Firebase.Analytics.Parameter(paramName, (double)((decimal)paramValue));
                }

                if (paramValue is int)
                {
                    return new Firebase.Analytics.Parameter(paramName, (int)paramValue);
                }

                if (paramValue is long)
                {
                    return new Firebase.Analytics.Parameter(paramName, (long)paramValue);
                }

                return new Firebase.Analytics.Parameter(paramName, paramValue.ToString());
            }
#endif
        }
    }
}
#endif