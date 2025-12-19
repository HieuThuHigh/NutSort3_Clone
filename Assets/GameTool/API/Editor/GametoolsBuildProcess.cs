using System.IO;
using GameTool.APIs.Scripts;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
                         #if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.SearchService;
#endif

namespace GameTool.APIs.Editor
{
    public class GametoolsBuildProcess : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_ANDROID
        string[] result = AssetDatabase.FindAssets("GametoolSettings", new string[] { "Assets/GameToolSample/Resources" });

        string path = AssetDatabase.GUIDToAssetPath(result[0]);
        var config = (GameToolSettings)AssetDatabase.LoadAssetAtPath(path, typeof(GameToolSettings));

        config.buildVersion = Application.platform == RuntimePlatform.IPhonePlayer
            ? PlayerSettings.iOS.buildNumber
            : PlayerSettings.Android.bundleVersionCode.ToString();

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
        }
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
#if !UNITY_CLOUD_BUILD && UNITY_IOS
        Debug.Log("[iOS] OnPostprocessBuild");

        // Get plist
        var infoPlist = new UnityEditor.iOS.Xcode.PlistDocument();
        string infoPlistPath = Path.Combine(path, "Info.plist");
        infoPlist.ReadFromFile(infoPlistPath);
        // Register ios URL scheme for external apps to launch this app.
        PlistElementArray SKAdNetworkItems = null;
        // IronSource item
        SKAdNetworkItems = infoPlist.root.CreateArray("SKAdNetworkItems");
        SKAdNetworkItems.AddDict().SetString("SKAdNetworkIdentifier", "su67r6k2v3.skadnetwork");

        // FB item
        SKAdNetworkItems.AddDict().SetString("SKAdNetworkIdentifier", "v9wttpbfk9.skadnetwork");
        SKAdNetworkItems.AddDict().SetString("SKAdNetworkIdentifier", "n38lu8286q.skadnetwork");



        //bitcode
        if (buildTarget != BuildTarget.iOS) return;
        string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        PBXProject pbxProject = new PBXProject();
        pbxProject.ReadFromFile(projectPath);

        //Disabling Bitcode on all targets
        //Main
        string target = pbxProject.GetUnityMainTargetGuid();
        pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        //Unity Tests
        target = pbxProject.TargetGuidByName(PBXProject.GetUnityTestTargetName());
        pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        //Unity Framework
        target = pbxProject.GetUnityFrameworkTargetGuid();
        pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

        pbxProject.WriteToFile(projectPath);

        // App Tracking Consen
        infoPlist.root.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you");
        infoPlist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        File.WriteAllText(infoPlistPath, infoPlist.WriteToString());
#endif
        }
    }
}

