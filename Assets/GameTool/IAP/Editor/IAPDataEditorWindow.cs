#if USE_IAP
using System;
using System.IO;
using System.Linq;
using GameTool.Assistants;
using GameToolSample.Scripts.FirebaseServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPDataEditorWindow : EditorWindow
{
    private IAPManager iAPManager;
    private FirebaseRemote firebaseRemote;
    private Vector2 scr;

    [MenuItem("GameTool/API/IAP")]
    public static void ShowWindow()
    {
        GetWindow<IAPDataEditorWindow>("IAP Data Editor");
    }

    private void OnEnable()
    {
        iAPManager = FindObjectOfType<IAPManager>(true);
        firebaseRemote = FindObjectOfType<FirebaseRemote>(true);
    }

    private void OnGUI()
    {
        minSize = new Vector2(800, 400);

        if (iAPManager == null)
        {
            GUILayout.Label("IAPManager is not in the scene");
            return;
        }

        if (firebaseRemote == null)
        {
            GUILayout.Label("FirebaseRemote is not in the scene");
            return;
        }

        var param = firebaseRemote.remoteRows.First(row => row.className == "IAPId").param;

        if (GUILayout.Button("Save"))
        {
            firebaseRemote.remoteRows.First(row => row.className == "IAPId").param =
                new RemoteParam[iAPManager.products.Count];
            for (int i = 0; i < iAPManager.products.Count; i++)
            {
                var pa = firebaseRemote.remoteRows.First(row => row.className == "IAPId").param;
                pa[i] = new RemoteParam
                {
                    variableType = VariableType.STRING,
                    paramName = iAPManager.products[i].name
                };
            }

            EditorUtils.UpdateEnumInFile(EditorUtils.GetSystemFilePath(EditorUtils.FindFilePath("IAPManager", "cs")), "IAPItemName",
                firebaseRemote.remoteRows.First(row => row.className == "IAPId").param.Select(x => x.paramName)
                    .ToList(), typeof(IAPManager.IAPItemName));

            firebaseRemote.ReadandWriteFile();
            EditorUtility.SetDirty(iAPManager);
            EditorUtility.SetDirty(firebaseRemote);

            AssetDatabase.ImportAsset(EditorUtils.FindFilePath("FirebaseRemote", "cs"));
            AssetDatabase.ImportAsset(EditorUtils.FindFilePath("IAPManager", "cs"));
            UpdateProperties();
            AssetDatabase.ImportAsset(EditorUtils.FindFilePath("IAPManager", "cs"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (GUILayout.Button("Apply to Firebase"))
        {
            SerializedObject _firebaseRemote = new SerializedObject(firebaseRemote);
            for (int i = 0; i < param.Length; i++)
            {
                _firebaseRemote.FindProperty("iAPidDF").FindPropertyRelative(param[i].paramName).stringValue =
                    iAPManager.products[i].key;
            }

            _firebaseRemote.ApplyModifiedProperties();
            EditorUtility.SetDirty(firebaseRemote);
            AssetDatabase.SaveAssetIfDirty(firebaseRemote);
            AssetDatabase.Refresh();
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(EditorUtils.FindFilePath("IAPManager", "cs")),
                FindLine("public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)"));
        }

        if (GUILayout.Button("Go to Logic items"))
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(EditorUtils.FindFilePath("BuyIAP", "cs")),
                FindLine("public virtual void LoadIAPReward()", "BuyIAP"));
        }

        if (GUILayout.Button("Go to Logic subscription"))
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(EditorUtils.FindFilePath("IAPManager", "cs")),
                FindLine("public void SubscriptionReward(int indexProduct)"));
        }

        GUILayout.Space(20);

        scr = EditorGUILayout.BeginScrollView(scr);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Index", GUILayout.Width(50));
        GUILayout.Label("Key (eg: com.game.removeads)", GUILayout.Width(300));
        GUILayout.Label("Name (eg: RemoveAds)", GUILayout.Width(200));
        GUILayout.Label("Type", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < iAPManager.products.Count; i++)
        {
            IAPManager.IAPProduct product = iAPManager.products[i];

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(50));
            product.key = EditorGUILayout.TextField(product.key, GUILayout.Width(300));
            product.name = EditorGUILayout.TextField(product.name, GUILayout.Width(200));
            product.productType =
                (ProductType)EditorGUILayout.EnumPopup(product.productType, GUILayout.Width(100));
            if (GUILayout.Button("Remove"))
            {
                iAPManager.products.RemoveAt(i);
                i--;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Product"))
        {
            iAPManager.products.Add(new IAPManager.IAPProduct());
        }

        EditorGUILayout.EndScrollView();
    }

    private int FindLine(string lineContent, string path = "IAPManager")
    {
        var txt = File.ReadAllLines(EditorUtils.GetSystemFilePath(EditorUtils.FindFilePath(path, "cs")));
        for (int i = 0; i < txt.Length; i++)
        {
            if (txt[i + 1].Contains(lineContent))
            {
                return i + 2;
            }
        }

        return 0;
    }

    public void UpdateProperties()
    {
        var iapManager = iAPManager;
        string propertyCode = "";
        for (int i = 0; i < iapManager.products.Count; i++)
        {
            var pro = iapManager.products[i];
            if (iAPManager.GetType().GetProperty(pro.name) == null)
            {
                propertyCode += $"public IAPProduct {pro.name} => IAPUlts.GetProduct(IAPItemName.{pro.name});\n";
            }
        }

        string unityScriptPath = EditorUtils.FindFilePath("IAPManager", "cs");

        string scriptContent = AssetDatabase.LoadAssetAtPath<TextAsset>(unityScriptPath).text;
        int autoGenerateIndex =
            scriptContent.IndexOf("// AUTO GENERATE", StringComparison.Ordinal);

        if (autoGenerateIndex != -1)
        {
            scriptContent = scriptContent.Insert(autoGenerateIndex, propertyCode);
            File.WriteAllText(EditorUtils.GetSystemFilePath(unityScriptPath), scriptContent);
            EditorUtils.ReImportAsset(unityScriptPath);
        }
        else
        {
            Debug.LogError("Không thấy dòng //AUTO GENERATE trong script IAPManager.");
        }
    }
}
#endif