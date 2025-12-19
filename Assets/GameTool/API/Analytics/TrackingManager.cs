#if USE_ADJUST
using com.adjust.sdk;
#endif

#if USE_AF
using AppsFlyerSDK;
#endif

using System.Collections;
using System.Collections.Generic;
using DatdevUlts.Ults;
using GameTool.APIs.Scripts;
using GameTool.Assistants.DesignPattern;
using UnityEngine;
using static GameToolSample.Scripts.Enum.AnalyticID;

namespace GameTool.APIs.Analytics
{
    namespace Analytics
    {
        public class TrackingManager : SingletonMonoBehaviour<TrackingManager>
        {
            private string currentScreen = "spl";
#if USE_ADJUST
            string adjustAppToken;
#endif
#if USE_AF
            string afDevKey;
            string afAppID;
            private AFAnalytics afAnalytics;
#endif

#if USE_SINGULAR
            string singularAPIKey;
            string singularAPISecret;
#endif
            protected List<IAnalytic> Analytics = new List<IAnalytic>();
            protected List<IAnalytic> LimitedAnalytics = new List<IAnalytic>();
            private FirebaseAnalytics firebaseAnalytics;

            public bool IsInited
            {
                get => _isInited;
                private set => _isInited = value;
            }

            public GameToolSettings GameToolSettings => APIs.Scripts.API.Instance.GameToolSetting;
            private AdsType adsType = AdsType.none;
            private bool _isInited;

            void Start()
            {
#if USE_ADJUST
                adjustAppToken = GameToolSettings.adjustAppToken;
#endif
#if USE_AF
                afDevKey = GameToolSettings.afDevKey;
                afAppID = GameToolSettings.afAppID;
#endif

#if USE_SINGULAR
                 singularAPIKey = GameToolSettings.singularAPIKey;
                 singularAPISecret = GameToolSettings.singularAPISecret;
#endif
                InitTracking();
            }

            public void ShowAdsEvent(AdsType adsTypeInt)
            {
                adsType = adsTypeInt;
                StartCoroutine(nameof(CheckAdsBack));
            }

            public void CloseAdsEvent()
            {
                StartCoroutine(nameof(CheckAdsBack));
            }

            IEnumerator CheckAdsBack()
            {
                yield return new WaitForSecondsRealtime(2);
                adsType = AdsType.none;
            }

            private void OnApplicationFocus(bool focus)
            {
                string screen = currentScreen;
                switch (adsType)
                {
                    case AdsType.none:
                        screen = currentScreen;
                        break;

                    case AdsType.app_open:
                        screen = "appopen";
                        break;

                    case AdsType.interstitial:
                        screen = "inter";
                        break;

                    case AdsType.rewarded:
                        screen = "reward";
                        break;
                }

                if (focus)
                {
                    TrackEvent("app_back", new Dictionary<string, object>()
                    {
                        {
                            "screen", screen
                        }
                    });
                }
                else
                {
                    TrackEvent("app_ignore", new Dictionary<string, object>()
                    {
                        {
                            "screen", screen
                        }
                    });
                }
            }

            void InitTracking()
            {
                Analytics.Clear();

#if USE_FIREBASE
                firebaseAnalytics = new FirebaseAnalytics();
                Analytics.Add(firebaseAnalytics);
                firebaseAnalytics.Init();
#endif
                
#if USE_AF
                if (afDevKey == "")
                {
                    Debug.LogError("AF Key is empty ! Can't Publish this version ");
                }

                afAnalytics = new AFAnalytics();
                LimitedAnalytics.Add(afAnalytics);
                afAnalytics.Init(afDevKey, afAppID);
#endif

#if USE_SINGULAR
                limitedAnalytic = new GameTool.Analytics.SingularAnalytics();
                LimitedAnalytics.Add(limitedAnalytic);
                limitedAnalytic.Init();
#endif

#if USE_ADJUST
                if (adjustAppToken == "")
                {
                    Debug.LogError("Adjut Apptoken is empty ! Can't Publish this version ");
                }

                Adjust adjust = this.gameObject.AddComponent<Adjust>();

                AdjustConfig adjustConfig = new AdjustConfig(this.adjustAppToken, AdjustEnvironment.Production);
                Adjust.start(adjustConfig);

                limitedAnalytic = new GameTool.Analytics.AdjustAnalytics();
                LimitedAnalytics.Add(limitedAnalytic);
                limitedAnalytic.Init(adjustAppToken);
#endif
                IsInited = true;
            }

            public void TrackEvent(string eventName, Dictionary<string, object> parameters)
            {
#if USE_GAMETOOL_TRACKING
                eventName = eventName.Replace(" ", string.Empty);
                
                foreach (IAnalytic analytic in Analytics)
                {
                    analytic.TrackEvent(eventName, parameters);
                }
#endif
            }

            public void TrackEvent(string eventName)
            {
#if USE_GAMETOOL_TRACKING
                eventName = eventName.Replace(" ", string.Empty);

                foreach (IAnalytic analytic in Analytics)
                {
                    analytic.TrackEvent(eventName);
                }
#endif
            }

            public void TrackAjustEvent(string eventName)
            {
#if USE_ADJUST
                eventName = eventName.Replace(" ", string.Empty);
                foreach (IAnalytic analytic in LimitedAnalytics)
                {
                    analytic.TrackEvent(eventName);
                }
#endif
            }

            public void TrackLimitedEvent(string eventName)
            {
#if USE_GAMETOOL_TRACKING
                eventName = eventName.Replace(" ", string.Empty);

                foreach (IAnalytic analytic in LimitedAnalytics)
                {
                    analytic.TrackEvent(eventName);
                }
#endif
            }

            public void TrackLimitedEvent(string eventName, Dictionary<string, object> parameters)
            {
#if USE_GAMETOOL_TRACKING
                eventName = eventName.Replace(" ", string.Empty);

                foreach (IAnalytic analytic in LimitedAnalytics)
                {
                    analytic.TrackEvent(eventName, parameters);
                }
#endif
            }

            public void TrackSingularEvent(string eventName)
            {
#if USE_SINGULAR
                eventName = eventName.Replace(" ", string.Empty);
                SingularSDK.Event(eventName);
#endif
            }

#if USE_AF
            public void TrackAfRevenue(string monetizationNetwork, AppsFlyerAdRevenueMediationNetworkType appsFlyerAdRevenueMediationNetworkType,
                double eventRevenue, Dictionary<string, string> additionalParameters)
            {
                MediationNetwork mediationNetwork = MediationNetwork.GoogleAdMob;
                if (appsFlyerAdRevenueMediationNetworkType == AppsFlyerAdRevenueMediationNetworkType
                        .AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob)
                {
                    mediationNetwork = MediationNetwork.GoogleAdMob;
                }
                else if (appsFlyerAdRevenueMediationNetworkType == AppsFlyerAdRevenueMediationNetworkType
                             .AppsFlyerAdRevenueMediationNetworkTypeApplovinMax)
                {
                    mediationNetwork = MediationNetwork.ApplovinMax;
                }
                else if (appsFlyerAdRevenueMediationNetworkType == AppsFlyerAdRevenueMediationNetworkType
                             .AppsFlyerAdRevenueMediationNetworkTypeIronSource)
                {
                    mediationNetwork = MediationNetwork.IronSource;
                }
                var logRevenue = new AFAdRevenueData(monetizationNetwork, mediationNetwork, "USD", eventRevenue);
                AppsFlyer.logAdRevenue(logRevenue, additionalParameters);
            }

            public void TrackAfIAPRevenue(string contentype, string currency, string revenue)
            {
                System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
                    System.Collections.Generic.Dictionary<string, string>();
                purchaseEvent.Add(AFInAppEvents.REVENUE, revenue);
                purchaseEvent.Add(AFInAppEvents.CURRENCY, currency);
                purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, contentype);
                AppsFlyer.sendEvent("af_purchase", purchaseEvent);

            }
#endif
            [ContextMenu("ADD Tracking")]
            private void ADDTracking()
            {
#if USE_SINGULAR
                if (singularAPIKey == "" || singularAPISecret == "")
                {
                    Debug.LogError("Singular is empty ! Can't Publish this version ");
                }

                if (!this.GetComponent<SingularSDK>())
                {
                    SingularSDK singularSDK = this.gameObject.AddComponent<SingularSDK>();
                    singularSDK.SingularAPIKey = singularAPIKey;
                    singularSDK.SingularAPISecret = singularAPISecret;
                    singularSDK.autoIAPComplete = true;
                }
#endif
#if USE_AF
                if (!this.GetComponent<AppsFlyerObjectScript>())
                {
                    AppsFlyerObjectScript singularSDK = this.gameObject.AddComponent<AppsFlyerObjectScript>();
                }
#endif
            }

            #region TRACK

            public void TrackLevelFirstPlay(int value, GameModeName gameModeName = GameModeName.none)
            {
#if USE_GAMETOOL_TRACKING
                string limitedEventName = "lv_start";
                string eventName = string.Format("Lv_Start_{0}", value);

                if (gameModeName != GameModeName.none)
                {
                    limitedEventName = string.Format("lv_start_{0}", gameModeName);
                    eventName = string.Format("Lv_Start_{0}_{1}", gameModeName, value);
                }

                TrackLimitedEvent(limitedEventName);
                TrackLevelStartCostCenter(value, gameModeName);
                TrackEvent(eventName);
#endif
            }

            public void TrackLevelFirstVictory(int value, GameModeName gameModeName = GameModeName.none)
            {
#if USE_GAMETOOL_TRACKING
                string limitedEventName = "lv_com";
                string eventName = string.Format("Lv_Com_{0}", value);

                if (gameModeName != GameModeName.none)
                {
                    limitedEventName = string.Format("lv_com_{0}", gameModeName);
                    eventName = string.Format("Lv_Com_{0}_{1}", gameModeName, value);
                }

                TrackLimitedEvent(limitedEventName);
                TrackLevelEndCostCenter(value, gameModeName, true);
                TrackEvent(eventName);
#endif
            }

            public void TrackLevelFirstLose(int value, GameModeName gameModeName = GameModeName.none)
            {
#if USE_GAMETOOL_TRACKING
                string limitedEventName = "lv_lose";
                string eventName = string.Format("Lv_Lose_{0}", value);

                if (gameModeName != GameModeName.none)
                {
                    limitedEventName = string.Format("lv_lose_{0}", gameModeName);
                    eventName = string.Format("Lv_Lose_{0}_{1}", gameModeName, value);
                }

                TrackLimitedEvent(limitedEventName);
                TrackLevelEndCostCenter(value, gameModeName);
                TrackEvent(eventName);
#endif
            }

            public void TrackLevelFirstSkip(int value, GameModeName gameModeName = GameModeName.none)
            {
#if USE_GAMETOOL_TRACKING
                string limitedEventName = "lv_skip";
                string eventName = string.Format("Lv_Skip_{0}", value);

                if (gameModeName != GameModeName.none)
                {
                    limitedEventName = string.Format("lv_skip_{0}", gameModeName);
                    eventName = string.Format("Lv_Skip_{0}_{1}", gameModeName, value);
                }

                TrackLimitedEvent(limitedEventName);
                TrackLevelEndCostCenter(value, gameModeName, true);
                TrackEvent(eventName);
#endif
            }

            public void TrackLevelFirstReplay(int value, GameModeName gameModeName = GameModeName.none)
            {
#if USE_GAMETOOL_TRACKING
                string limitedEventName = "lv_replay";
                string eventName = string.Format("Lv_Replay_{0}", value);

                if (gameModeName != GameModeName.none)
                {
                    limitedEventName = string.Format("lv_replay_{0}", gameModeName);
                    eventName = string.Format("Lv_Replay_{0}_{1}", gameModeName, value);
                }

                TrackLimitedEvent(limitedEventName);
                TrackLevelEndCostCenter(value, gameModeName);
                TrackEvent(eventName);
#endif
            }

            #region GAMEPLAY

            public void TrackGamePlay(GamePlayEvent gamePlayEvent, int level,
                GameModeName gameModeName = GameModeName.none, Dictionary<string, object> addingTracking = null)
            {
#if USE_GAMETOOL_TRACKING
                Dictionary<string, object> defaultData = new Dictionary<string, object>()
                {
                    {
                        GamePlayParam.level.ToString(), level.ToString()
                    }
                };

                if (addingTracking != null)
                {
                    IListExtensions.AddRange(defaultData, addingTracking);
                }

                string eventName = gamePlayEvent.ToString();
                if (gameModeName != GameModeName.none)
                {
                    eventName = string.Format("{0}_{1}", gamePlayEvent, gameModeName);
                }

                TrackEvent(eventName, defaultData);
#endif
            }

            public void TrackLevelStartCostCenter(int currentLevel = 0, GameModeName gameModeName = GameModeName.none)
            {
#if USE_FIREBASE
                if (gameModeName == GameModeName.none)
                {
                    Firebase.Analytics.Parameter[] LevelStartParameters =
                    {
                        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                            currentLevel.ToString()),
                    };

                    Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelStart,
                        LevelStartParameters);

                    Debug.Log("FirebaseAnalytics - CostCenter: " +
                              Firebase.Analytics.FirebaseAnalytics.EventLevelStart + "- Parameters: " +
                              Firebase.Analytics.FirebaseAnalytics.ParameterLevel + "--" + currentLevel);
                }
                else
                {
                    Firebase.Analytics.Parameter[] LevelStartParameters =
                    {
                        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                            currentLevel.ToString()),
                        new Firebase.Analytics.Parameter("level_mode", gameModeName.ToString())
                    };

                    Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelStart,
                        LevelStartParameters);

                    Debug.Log("FirebaseAnalytics - CostCenter: " +
                              Firebase.Analytics.FirebaseAnalytics.EventLevelStart + "- Parameters: " +
                              Firebase.Analytics.FirebaseAnalytics.ParameterLevel + "--" + currentLevel);

                    Debug.Log("FirebaseAnalytics - CostCenter: " +
                              Firebase.Analytics.FirebaseAnalytics.EventLevelStart + "- Parameters: level_mode --" +
                              gameModeName);
                }
#endif
            }

            public void TrackLevelEndCostCenter(int currentLevel = 0, GameModeName gameModeName = GameModeName.none,
                bool isSuccess = false)
            {
#if USE_FIREBASE
                if (gameModeName == GameModeName.none)
                {
                    Firebase.Analytics.Parameter[] LevelEndParameters =
                    {
                        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                            currentLevel.ToString()),
                        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterSuccess,
                            isSuccess.ToString())
                    };

                    Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelEnd,
                        LevelEndParameters);

                    Debug.Log("FirebaseAnalytics - CostCenter: " + Firebase.Analytics.FirebaseAnalytics.EventLevelEnd +
                              "- Parameters: " +
                              Firebase.Analytics.FirebaseAnalytics.ParameterLevel + "--" + currentLevel);

                    Debug.Log("FirebaseAnalytics - CostCenter: " + Firebase.Analytics.FirebaseAnalytics.EventLevelEnd +
                              "- Parameters: " +
                              Firebase.Analytics.FirebaseAnalytics.ParameterSuccess + "--" + isSuccess);
                }
                else
                {
                    Firebase.Analytics.Parameter[] LevelEndParameters =
                    {
                        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterLevel,
                            currentLevel.ToString()),
                        new Firebase.Analytics.Parameter("level_mode", gameModeName.ToString()),
                        new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterSuccess,
                            isSuccess.ToString())
                    };

                    Firebase.Analytics.FirebaseAnalytics.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLevelEnd,
                        LevelEndParameters);

                    Debug.Log("FirebaseAnalytics - CostCenter: " + Firebase.Analytics.FirebaseAnalytics.EventLevelEnd +
                              "- Parameters: " +
                              Firebase.Analytics.FirebaseAnalytics.ParameterLevel + "--" + currentLevel);

                    Debug.Log("FirebaseAnalytics - CostCenter: " + Firebase.Analytics.FirebaseAnalytics.EventLevelEnd +
                              "- Parameters: level_mode --" + gameModeName);

                    Debug.Log("FirebaseAnalytics - CostCenter: " + Firebase.Analytics.FirebaseAnalytics.EventLevelEnd +
                              "- Parameters: " +
                              Firebase.Analytics.FirebaseAnalytics.ParameterSuccess + "--" + isSuccess);
                }
#endif
            }

            #endregion

            #region SCREEN

            public void TrackButtonGameScreen(ScreenID screenName, ButtonID buttonName)
            {
#if USE_GAMETOOL_TRACKING
                currentScreen = screenName.ToString();

                TrackEvent("game_screen", new Dictionary<string, object>()
                {
                    {
                        "screen", screenName
                    },
                    {
                        "button", buttonName
                    }
                });
#endif
            }

            #endregion

            #region ECONOMY

            public void TrackGameEconomy(string currency, int value, GameEconomyState state, LocationTracking location,
                Dictionary<string, object> addingTracking = null)
            {
#if USE_GAMETOOL_TRACKING
                Dictionary<string, object> defaultData = new Dictionary<string, object>()
                {
                    {
                        "currency", currency
                    },
                    {
                        "value", value.ToString()
                    },
                    {
                        "state", state
                    },
                    {
                        "location", location
                    }
                };

                if (addingTracking != null)
                {
                    IListExtensions.AddRange(defaultData, addingTracking);
                }

                TrackEvent("game_economy", defaultData);
#endif
            }

            #endregion

            #region SKIN

            public void TrackGameSkin(string type, int id, GameItemState state, LocationTracking location,
                Dictionary<string, object> addingTracking = null)
            {
#if USE_GAMETOOL_TRACKING
                Dictionary<string, object> defaultData = new Dictionary<string, object>()
                {
                    {
                        "type", type
                    },
                    {
                        "id", id.ToString()
                    },
                    {
                        "state", state
                    },
                    {
                        "location", location
                    }
                };

                if (addingTracking != null)
                {
                    IListExtensions.AddRange(defaultData, addingTracking);
                }

                TrackEvent("game_skin", defaultData);
#endif
            }

            public void TrackGameSkill(string type, GameItemState state, LocationTracking location,
                Dictionary<string, object> addingTracking = null)
            {
#if USE_GAMETOOL_TRACKING
                Dictionary<string, object> defaultData = new Dictionary<string, object>()
                {
                    {
                        "type", type
                    },
                    {
                        "state", state
                    },
                    {
                        "location", location
                    }
                };

                if (addingTracking != null)
                {
                    IListExtensions.AddRange(defaultData, addingTracking);
                }

                TrackEvent("game_skill", defaultData);
#endif
            }

            #endregion

            #region ADS

            public void TrackAds(AdsType monetizationType, MonetizationState monetizationState,
                LocationTracking location, Dictionary<string, object> addingTracking = null)
            {
#if USE_GAMETOOL_TRACKING
                Dictionary<string, object> defaultData = new Dictionary<string, object>()
                {
                    {
                        "type", monetizationType
                    },
                    {
                        "state", monetizationState
                    },
                    {
                        "location", location
                    }
                };

                if (addingTracking != null)
                {
                    IListExtensions.AddRange(defaultData, addingTracking);
                }

                TrackEvent("ads", defaultData);
#endif
            }

            public void TrackIAP(string id, LocationTracking location, MonetizationState monetizationState,
                Dictionary<string, object> addingTracking = null)
            {
#if USE_GAMETOOL_TRACKING
                Dictionary<string, object> defaultData = new Dictionary<string, object>()
                {
                    {
                        "id", id
                    },
                    {
                        "location", location
                    },
                    {
                        "state", monetizationState
                    }
                };

                if (addingTracking != null)
                {
                    IListExtensions.AddRange(defaultData, addingTracking);
                }

                TrackEvent("iap", defaultData);
#endif
            }

            #endregion

            #region USER BEHAVIOUR

            public void TrackUserBehavior(UseBehaviourState state, LocationTracking location,
                Dictionary<string, object> addingTracking = null)
            {
#if USE_GAMETOOL_TRACKING
                Dictionary<string, object> defaultData = new Dictionary<string, object>()
                {
                    {
                        "state", state
                    },
                    {
                        "location", location
                    }
                };

                if (addingTracking != null)
                {
                    IListExtensions.AddRange(defaultData, addingTracking);
                }

                TrackEvent("user_behaviour", defaultData);
#endif
            }

            #endregion

            #endregion

            public void TrackFirebaseEvent(string eventName)
            {
                eventName = eventName.Replace(" ", string.Empty);
                firebaseAnalytics.TrackEvent(eventName);
            }

            public void TrackFirebaseEvent(string eventName, Dictionary<string, object> parameters)
            {
                eventName = eventName.Replace(" ", string.Empty);
                firebaseAnalytics.TrackEvent(eventName, parameters);
            }

            public void TrackAfEvent(string eventName)
            {
                eventName = eventName.Replace(" ", string.Empty);
#if USE_AF
                afAnalytics.TrackEvent(eventName);
#endif
            }

            public void TrackAfEvent(string eventName, Dictionary<string, object> parameters)
            {
                eventName = eventName.Replace(" ", string.Empty);
#if USE_AF
                afAnalytics.TrackEvent(eventName, parameters);
#endif
            }
        }
    }
}