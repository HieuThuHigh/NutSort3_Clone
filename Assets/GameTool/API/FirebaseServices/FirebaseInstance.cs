#if USE_FIREBASE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;

namespace GameTool.FirebaseService
{
    public static class FirebaseInstance
    {
        static Firebase.FirebaseApp sInstance = null;
        public static bool HasInstance { get { return sInstance != null; } }

        static System.Action EventOnInitSuccess;

        static bool sIsInitializing = false;

        public static void CheckAndTryInit(System.Action callback)
        {
            if (HasInstance)
            {
                callback();
            }
            else if (sIsInitializing)
            {
                EventOnInitSuccess += callback;
            }
            else
            {
                sIsInitializing = true;
                EventOnInitSuccess += callback;
                Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == Firebase.DependencyStatus.Available)
                    {
                        sInstance = Firebase.FirebaseApp.DefaultInstance;
                        if ((object)EventOnInitSuccess != null)
                        {
                            EventOnInitSuccess();
                            EventOnInitSuccess = null;
                        }
                        Debug.Log("Firebase Available!");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                        // Firebase Unity SDK is not safe to use here.

                        if ((object)EventOnInitSuccess != null)
                        {
                            EventOnInitSuccess();
                            EventOnInitSuccess = null;
                        }
                    }
                    sIsInitializing = false;
                });
            }
        }
    }
}
#endif