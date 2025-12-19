#if USE_FIREBASE
using GameToolSample.Scripts.FirebaseServices;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FirebaseRemote))]
public class FirebaseRemoteconfigEditor : Editor
{
    string[] data;
    GUILayoutOption[] guiLayoutOptions;
    public int index = 0;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        FirebaseRemote firebaseRemote = target as FirebaseRemote;

        GUIContent keyLabel = new GUIContent("Select Key");
        if (firebaseRemote.remoteRows.Length > 0)
        {
            data = new string[firebaseRemote.remoteRows.Length];
            for (int i = 0; i < firebaseRemote.remoteRows.Length; i++)
            {
                if (firebaseRemote.remoteRows[i].remoteKey != "")
                {
                    data[i] = firebaseRemote.remoteRows[i].remoteKey;
                }
                else
                    data[i] = firebaseRemote.remoteRows[i].className;
            }

            GUILayout.BeginHorizontal();
            index = EditorGUILayout.Popup(index, data);
            if (GUILayout.Button("Copy key"))
                CopyKey();

            if (GUILayout.Button("Copy Data"))
                CopyData();
            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Update Script", GUILayout.Height(40)))
            UpdateScript();

    }
    public void CopyKey()
    {
        EditorGUIUtility.systemCopyBuffer = data[index];

    }
    public void CopyData()
    {
        FirebaseRemote firebaseRemote = target as FirebaseRemote;

        EditorGUIUtility.systemCopyBuffer = firebaseRemote.CopyDefaultValue(index);
    }
    public void UpdateScript()
    {
        FirebaseRemote firebaseRemote = target as FirebaseRemote;
        firebaseRemote.ReadandWriteFile();
        AssetDatabase.Refresh();
    }
}
#endif
