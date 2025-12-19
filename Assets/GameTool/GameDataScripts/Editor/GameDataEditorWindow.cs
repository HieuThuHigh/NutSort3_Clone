using System;
using System.IO;
using System.Reflection;
using GameToolSample.GameDataScripts.Scripts;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2021_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace GameTool.GameDataScripts.Editor
{
    public class GameDataEditorWindow : EditorWindow
    {
        private const string filePathGameData = "Assets/GameToolSample/GameDataScripts/Scripts/GameData.cs";
        private const string filePathField = "Assets/GameToolSample/GameDataScripts/Scripts/DataField.cs";
        private const string gamedataPrefab = "Assets/GameToolSample/GameDataScripts/Prefabs/GameData.prefab";

        private GameData targetObject;
        private UnityEditor.Editor objectEditor;
        private Vector2 pos;
        #if UNITY_2021_1_OR_NEWER
        private ScrollView scrollView;
        #endif


        [MenuItem("GameTool/Game Data")]
        private static void ShowWindow()
        {
            var window = GetWindow<GameDataEditorWindow>();
            window.titleContent = new GUIContent("Game Data");
            window.Show();
        }

        private void OnEnable()
        {
            targetObject = null;

            if (Application.isPlaying)
            {
                targetObject = FindObjectOfType<GameData>(true);
            }

            if (!targetObject)
            {
                targetObject =
                    AssetDatabase.LoadAssetAtPath<GameData>(gamedataPrefab);
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange playMode)
        {
            if (playMode == PlayModeStateChange.EnteredPlayMode)
            {
                EditorUtility.SetDirty(targetObject);
                AssetDatabase.SaveAssetIfDirty(targetObject);
            }

            targetObject = null;
            if (playMode == PlayModeStateChange.EnteredPlayMode || playMode == PlayModeStateChange.ExitingEditMode)
            {
                targetObject = FindObjectOfType<GameData>(true);
                #if UNITY_2021_1_OR_NEWER
                RepaintScroll();
                #endif
            }

            if (!targetObject)
            {
                targetObject =
                    AssetDatabase.LoadAssetAtPath<GameData>(
                        gamedataPrefab);
                #if UNITY_2021_1_OR_NEWER
                RepaintScroll();
                #endif
            }
        }

        #if UNITY_2021_1_OR_NEWER
        private void RepaintScroll()
        {
            if (scrollView != null)
            {
                scrollView.Clear();
                scrollView.Add(new InspectorElement(targetObject));
            }
        }
        #endif

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        #if UNITY_2021_1_OR_NEWER
        private void CreateGUI()
        {
            var root = rootVisualElement;
            var horizontal = new VisualElement();
            horizontal.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            horizontal.style.justifyContent = new StyleEnum<Justify>(Justify.Center);
            horizontal.style.alignItems = new StyleEnum<Align>(Align.Center);
            horizontal.style.flexGrow = 1;
            horizontal.style.minHeight = new StyleLength(21);
            root.Add(horizontal);

            var percentBtn = 97 / 3f;

            horizontal.Add(CreateButtonPercentWidth(() =>
            {
                LoadData();
                targetObject.Data = GameData.Instance.Data;
            }, "Load Data", percentBtn));

            horizontal.Add(CreateButtonPercentWidth(() =>
            {
                GameData.Instance.Data = targetObject.Data;
                SaveData();
            }, "Save Data", percentBtn));

            horizontal.Add(CreateButtonPercentWidth(() =>
            {
                ClearData();
                targetObject.Data = GameData.Instance.Data;
            }, "Clear Data", percentBtn));

            root.Add(new Button(() => { OpenCodeFile(filePathField, 30); })
            {
                text = "Step 1: Go to Declare field"
            });

            root.Add(new Button(() =>
            {
                UpdateScript();
                GenerateProperties();
            })
            {
                text = "Step 2: Update Script and Done"
            });

            root.Add(new Button(() => { OpenCodeFile(filePathGameData, 60); })
            {
                text = "Go to Declare Getter & Setter (If you want see and format code)"
            });

            root.Add(new Button(() => { Application.OpenURL("https://wiki.GameTool.studio/doc/game-data-aiHz8FBfJj"); })
            {
                text = "DOCUMENT"
            });

            scrollView = new ScrollView(ScrollViewMode.Vertical);
            root.Add(scrollView);
            scrollView.Add(new InspectorElement(targetObject));
        }

        private Button CreateButtonPercentWidth(Action clickEvent, string text, float percent)
        {
            var btn = new Button(clickEvent)
            {
                text = text,
                style =
                {
                    width = new StyleLength(new Length(percent, LengthUnit.Percent)),
                    height = new StyleLength(StyleKeyword.Auto)
                }
            };

            return btn;
        }
        #else
        private void OnGUI()
        {
            if (targetObject.gameObject == null)
            {
                targetObject = GameData.Instance;
            }
            
            if (targetObject != null)
            {
                if (objectEditor == null || objectEditor.target != targetObject)
                {
                    // Tạo một Editor mới nếu không tồn tại hoặc đối tượng đã thay đổi
                    objectEditor = UnityEditor.Editor.CreateEditor(targetObject);
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Load Data"))
                {
                    LoadData();
                    targetObject.Data = GameData.Instance.Data;
                    EditorUtility.SetDirty(targetObject);
                }

                if (GUILayout.Button("Save Data"))
                {
                    GameData.Instance.Data = targetObject.Data;
                    EditorUtility.SetDirty(GameData.Instance);
                    SaveData();
                }

                if (GUILayout.Button("Clear Data"))
                {
                    ClearData();
                    targetObject.Data = GameData.Instance.Data;
                    EditorUtility.SetDirty(targetObject);
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Step 1: Go to Declare field"))
                {
                    OpenCodeFile(filePathField, 30);
                }

                if (GUILayout.Button("Step 2: Update Script and Done"))
                {
                    UpdateScript();
                    GenerateProperties();
                }

                if (GUILayout.Button("Go to Declare Getter & Setter (If you want see and format code)"))
                {
                    OpenCodeFile(filePathGameData, 60);
                }

                if (GUILayout.Button("DOCUMENT"))
                {
                    Application.OpenURL("https://wiki.gameTool.studio/doc/game-data-aiHz8FBfJj");
                }

                pos = EditorGUILayout.BeginScrollView(pos);
                // Hiển thị Inspector của đối tượng trong Editor Window
                try
                {
                    objectEditor.OnInspectorGUI();
                }
                catch (Exception)
                {
                    targetObject = GameData.Instance;
                    objectEditor = UnityEditor.Editor.CreateEditor(targetObject);
                    objectEditor.OnInspectorGUI();
                }
                EditorGUILayout.EndScrollView();
            }
        }
        #endif

        private void GenerateProperties()
        {
            Type gameDataType = typeof(GameData);
            FieldInfo[] dataFieldInfos = typeof(DataField).GetFields();

            foreach (FieldInfo fieldInfo in dataFieldInfos)
            {
                string fieldName = fieldInfo.Name;
                Type fieldType = fieldInfo.FieldType;

                PropertyInfo existingProperty = gameDataType.GetProperty(fieldName);
                if (existingProperty != null)
                {
                    continue;
                }

                string propertyCode = $"public {fieldType.Name} {fieldName}\n";

                if (fieldType.IsGenericType)
                {
                    string type = fieldType.Name.Substring(0, fieldType.Name.Length - 2) + "<";
                    for (int i = 0; i < fieldType.GenericTypeArguments.Length; i++)
                    {
                        if (i == fieldType.GenericTypeArguments.Length - 1)
                        {
                            type += fieldType.GenericTypeArguments[i].Name + ">";
                        }
                        else
                        {
                            type += fieldType.GenericTypeArguments[i].Name + ", ";
                        }
                    }

                    propertyCode = $"public {type} {fieldName}\n";
                }

                propertyCode += "{\n";
                propertyCode += "    get => Data." + fieldName + ";\n";
                propertyCode += "    set\n";
                propertyCode += "    {\n";
                propertyCode += "        Data." + fieldName + " = value;\n";
                propertyCode += "        SaveGameData.SaveData(eData." + fieldName + ", Data." + fieldName + ");\n";
                propertyCode += "    }\n";
                propertyCode += "}\n";

                var guids = AssetDatabase.FindAssets("GameData");
                string scriptPath = "";
                if (guids.Length <= 0)
                {
                    Debug.LogError("Game Data is Null");
                    return;
                }

                foreach (string guid in guids)
                {
                    scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                    string scriptName = Path.GetFileName(scriptPath);

                    if (scriptName == "GameData.cs")
                    {
                        scriptPath = scriptPath.Remove(0, "Assets".Length);
                        break;
                    }
                }

                string scriptContent = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets" + scriptPath).text;
                int autoGenerateIndex =
                    scriptContent.IndexOf("// AUTO GENERATE", StringComparison.Ordinal);

                if (autoGenerateIndex != -1)
                {
                    scriptContent = scriptContent.Insert(autoGenerateIndex, propertyCode);
                    File.WriteAllText(Application.dataPath + scriptPath, scriptContent);
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError("AUTO GENERATE section not found in the script.");
                }
            }
        }


        private static void OpenCodeFile(string filePath, int line)
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(filePath), line);
        }


        public static void UpdateScript()
        {
            string path = Application.dataPath + FindFilePath("GameDataControl");

            string pathEnum = Application.dataPath + FindFilePath("eData");

            List<string> fieldName = typeof(DataField).GetFields().Select(field => field.Name).ToList();

            List<string> textEnum = new List<string>();
            textEnum.Add("namespace GameToolSample.GameDataScripts.Scripts");
            textEnum.Add("{");
            textEnum.Add("    public enum eData");
            textEnum.Add("    {");
            textEnum.Add("        None,");

            List<string> texts = new List<string>();
            texts.Add("using GameTool.GameDataScripts;\n");
            texts.Add("namespace GameToolSample.GameDataScripts.Scripts");
            texts.Add("{");
            texts.Add("    public static class GameDataControl");
            texts.Add("    {");
            texts.Add("        public static void SaveAllData()");
            texts.Add("        {");

            foreach (string value in fieldName)
            {
                texts.Add(string.Format("            SaveGameData.SaveData(eData.{0}, GameData.Instance.Data.{0});",
                    value));
                textEnum.Add("        " + value + ",");
            }

            textEnum.Add("    }");
            textEnum.Add("}");

            texts.Add("        }");

            texts.Add("");

            texts.Add("        public static void LoadAllData()");
            texts.Add("        {");

            foreach (string value in fieldName)
            {
                texts.Add(string.Format("            SaveGameData.LoadData(eData.{0}, ref GameData.Instance.Data.{0});",
                    value));
            }

            texts.Add("        }");

            texts.Add("    }");

            texts.Add("}");

            File.WriteAllLines(pathEnum, textEnum.ToArray());
            File.WriteAllLines(path, texts.ToArray());
            AssetDatabase.ImportAsset(@"Assets" + FindFilePath("eData"));
            AssetDatabase.ImportAsset(@"Assets" + FindFilePath("GameDataControl"));
            AssetDatabase.Refresh();
        }

        public static void SaveData()
        {
            SaveGameData.SaveAllData();
        }

        public static void LoadData()
        {
            SaveGameData.LoadAllData();
        }

        public static void ClearData()
        {
            PlayerPrefs.DeleteAll();
            SaveGameManager.DeleteAllSave();
            SaveGameData.ClearData();
        }


        private static string FindFilePath(string fileName)
        {
            string[] paths = AssetDatabase.FindAssets(fileName);

            foreach (string guid in paths)
            {
                var scriptPath = AssetDatabase.GUIDToAssetPath(guid);
                string scriptName = Path.GetFileName(scriptPath);

                if (scriptName == fileName + ".cs")
                {
                    return scriptPath.Remove(0, "Assets".Length);
                }
            }

            Debug.LogWarning("Không tìm thấy tệp " + fileName);
            return "";
        }
    }
}