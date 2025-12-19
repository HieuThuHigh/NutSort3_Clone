using UnityEditor;
using UnityEngine;

namespace DatdevUlts.Csv2CsharpTools.Editor
{
    public class Csv2CsharpEditorWindow : EditorWindow
    {
        private TextAsset _csv;
        private string _linkCsvGgSheetPublic;
        private string _textcsv;
        private Vector2 _Scrolltextcsv;
        private string _textGenerate = "";

        private Vector2 _ScrolltextGenerate;

        //https://docs.google.com/spreadsheets/d/11CZHKzbLsgYTBEPl8HiVbxS5eaXJfCxzDkovq3y2zSg/export?format=csv
        [MenuItem("Window/Csv2Csharp")]
        private static void ShowWindow()
        {
            var window = GetWindow<Csv2CsharpEditorWindow>();
            window.titleContent = new GUIContent("Csv2Csharp");
            window.Show();

#if !USE_DATDEVJSON
            Debug.LogError("com.unity.nuget.newtonsoft-json@3.0");
            Debug.LogError("USE_DATDEVJSON");
#endif
        }

        private void OnGUI()
        {
#if !USE_DATDEVJSON
            EditorGUILayout.LabelField(
                "Cần import package: com.unity.nuget.newtonsoft-json@3.0 bằng cách import package bằng git URL và thêm Scripting Define Symbol: USE_DATDEVJSON");
            EditorGUILayout.LabelField("Xem console");
#endif

            EditorGUILayout.LabelField("Drag file csv here");
            _csv = (TextAsset)EditorGUILayout.ObjectField(_csv, typeof(TextAsset), true);
            EditorGUILayout.LabelField("OR paste link google sheet PUBLIC and Generate");
            _linkCsvGgSheetPublic = EditorGUILayout.TextField(_linkCsvGgSheetPublic);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            _Scrolltextcsv = EditorGUILayout.BeginScrollView(_Scrolltextcsv);
            if (_csv)
            {
                _textcsv = _csv.text;
                _csv = null;
            }

            EditorGUILayout.LabelField("CSV Text:");
            _textcsv = EditorGUILayout.TextArea(_textcsv);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            _ScrolltextGenerate = EditorGUILayout.BeginScrollView(_ScrolltextGenerate);

            EditorGUILayout.LabelField("Generated code:");
            _textGenerate = EditorGUILayout.TextArea(_textGenerate).Trim();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate code sample"))
            {
                if (!string.IsNullOrEmpty(_linkCsvGgSheetPublic))
                {
                    Csv2Csharp.CompleteHandle completeHandle = new Csv2Csharp.CompleteHandle();
                    var request = Csv2Csharp.Await_GGs_GetCsvText(_linkCsvGgSheetPublic, completeHandle);
                    while (request.MoveNext()) { }

                    _textcsv = completeHandle.Output;
                }
                _textGenerate = Csv2Csharp.GenerateCodeCsv(_textcsv).Trim();

                GUI.FocusControl(null);
            }
        }
    }
}