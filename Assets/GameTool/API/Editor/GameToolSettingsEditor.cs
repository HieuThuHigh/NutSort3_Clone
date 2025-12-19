using System.Reflection;
using GameTool.APIs.Scripts;
using GameTool.Assistants;
using UnityEditor;
using UnityEngine;

#if USE_ADMOB_ADS
using GoogleMobileAds.Editor;
#endif

namespace GameTool.APIs.Editor
{
    [CustomEditor(typeof(GameToolSettings))]
    public class GameToolSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GameToolSettings gameToolSettings = (GameToolSettings)target;
            APIPlayerSetting apiPlayerSetting = APIPlayerSetting.Instance;

            DrawVariables(gameToolSettings);

            DrawSaveContent(apiPlayerSetting, gameToolSettings);


            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("DOWNLOAD SDKs", CenteredBoldStyle);
            if (GameToolSettings.Instance.useFirebase && GUILayout.Button("Firebase"))
            {
                Application.OpenURL("https://github.com/firebase/firebase-unity-sdk/releases");
            }

            if (gameToolSettings.IsUseAnyMediation(MediationType.Admob) && GUILayout.Button("Admob"))
            {
                Application.OpenURL("https://github.com/googleads/googleads-mobile-unity/releases");
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawVariables(GameToolSettings gameToolSettings)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useSpine"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useGGReview"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useLocalNotification"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useFirebase"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useIap"));

            if (gameToolSettings.useIap && gameToolSettings.useFirebase)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useFirebaseIAP"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("mmpType"));

            if (gameToolSettings.mmpType == MMPType.Adjust)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("adjustAppToken"));
            }
            else if (gameToolSettings.mmpType == MMPType.Appsflyer)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("afDevKey"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("afAppID"));
            }
            else if (gameToolSettings.mmpType == MMPType.Singular)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("singularAPIKey"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("singularAPISecret"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("listAdsFullMediation"));

            if (gameToolSettings.IsUseAnyFull())
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("timeRetryLoadAds"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useBackfill"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("bannerMediation"));
            if (gameToolSettings.bannerMediation != MediationType.Admob && gameToolSettings.bannerMediation != MediationType.None)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useAdmobBanner"));
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("aoaMediation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("nativeMediation"));
            if (gameToolSettings.nativeMediation != MediationType.Admob && gameToolSettings.nativeMediation != MediationType.None)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useAdmobNative"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAdverty"));


            if (gameToolSettings.IsUseAnyMediation(MediationType.Admob))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("admobAppID"));

                EditorGUILayout.Space();
                if (gameToolSettings.aoaMediation == MediationType.Admob)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("admobAppOpenID"));
                }

                if (gameToolSettings.nativeMediation == MediationType.Admob || gameToolSettings.useAdmobNative)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("admobNativeID"));
                }

                if (gameToolSettings.bannerMediation == MediationType.Admob || gameToolSettings.useAdmobBanner)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("admobBannerID"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("admobCollapsibleBannerID"));
                }

                if (gameToolSettings.IsUseAnyFull(MediationType.Admob))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("admobInterstitialID"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("admobRewardVideoID"));
                }

                EditorUtility.SetDirty(gameToolSettings);
            }

            if (gameToolSettings.IsUseAnyMediation(MediationType.IronSource))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("irsAppKey"));
            }
            
            if (gameToolSettings.IsUseAnyMediation(MediationType.Applovin))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applovinSDKKey"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applovinBannerID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applovinInterstitialID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applovinRewardVideoID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applovinAppOpenID"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("applovinMrecID"));
            }
        }

        private static GUIStyle BoldTextStyle
        {
            get
            {
                GUIStyle guiStyle = new GUIStyle(GUI.skin.label);
                guiStyle.fontStyle = FontStyle.Bold;
                return guiStyle;
            }
        }

        private void DrawSaveContent(APIPlayerSetting apiPlayerSetting, GameToolSettings gameToolSettings)
        {
            if (apiPlayerSetting.GameToolSetting != gameToolSettings)
            {
                if (GUILayout.Button("ACTIVE"))
                {
                    apiPlayerSetting.GameToolSetting = gameToolSettings;
                    EditorUtility.SetDirty(apiPlayerSetting);
                    AssetDatabase.SaveAssetIfDirty(apiPlayerSetting);
                }
            }
            else
            {
                EditorGUILayout.Space(20);

                EditorGUILayout.LabelField("ACTIVED", CenteredBoldStyle);

                if (GUILayout.Button("SAVE"))
                {
                    EditorUtility.SetDirty(gameToolSettings);
                    string assetPath = AssetDatabase.GetAssetPath(gameToolSettings);
                    EditorUtils.ReImportAsset(assetPath);
                }

                if (GUILayout.Button("CHECK DEFINESYMBOLS"))
                {
                    apiPlayerSetting.SaveDefineSymbols();
                }

                if (GUILayout.Button("CHECK COMPONENTS"))
                {
                    apiPlayerSetting.CheckComponent();

                    SyncKeyAdmob(gameToolSettings);
                }
            }
        }

        private static void SyncKeyAdmob(GameToolSettings gameToolSettings)
        {
#if USE_ADMOB_ADS
            if (gameToolSettings.IsUseAnyMediation(MediationType.Admob))
            {
                ScriptableObject ggSetting = null;
                var assembly = Assembly.GetAssembly(typeof(GoogleMobileAdsSettingsEditor));
                var type =
                    assembly.GetType("GoogleMobileAds.Editor.GoogleMobileAdsSettings", throwOnError: true,
                        ignoreCase: true);

                if (type != null)
                {
                    var method = type.GetMethod("LoadInstance", BindingFlags.NonPublic | BindingFlags.Static);

                    if (method != null)
                    {
                        ggSetting = (ScriptableObject)method.Invoke(null, null);
                    }
                    else
                    {
                        Debug.LogError("Method not found");
                    }
                }
                else
                {
                    Debug.LogError("Type not found");
                }

                if (!ggSetting)
                {
                    Debug.LogError("Cannot find GoogleMobileAdsSettings");
                    return;
                }

                SerializedObject serGGSetting = new SerializedObject(ggSetting);

                Undo.RecordObject(ggSetting, "GoogleMobileAdsSettings");

                if (Scripts.API.IsAndroid())
                {
                    serGGSetting.FindProperty("adMobAndroidAppId").stringValue = gameToolSettings.admobAppID;
                }
                else if (Scripts.API.IsIOS())
                {
                    serGGSetting.FindProperty("adMobIOSAppId").stringValue = gameToolSettings.admobAppID;
                }

                serGGSetting.FindProperty("optimizeInitialization").boolValue = true;
                serGGSetting.FindProperty("optimizeAdLoading").boolValue = true;
                serGGSetting.ApplyModifiedProperties();

                EditorUtils.ReImportAsset(AssetDatabase.GetAssetPath(ggSetting));
            }
#endif
        }

        private static GUIStyle CenteredBoldStyle
        {
            get
            {
                GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                centeredStyle.fontStyle = FontStyle.Bold;
                return centeredStyle;
            }
        }

        [MenuItem("GameTool/API/GameTool Settings")]
        public static void OpenGameToolSettings()
        {
            Selection.activeObject = APIPlayerSetting.Instance.GameToolSetting;
        }
    }
}