using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SpriteExporter
{
    public class SpriteExportWindow : EditorWindow
    {
        private List<Object> objectsToExport = new List<Object>();
        private Vector2 scroll;

        [MenuItem("Tools/Sprite Exporter")]
        public static void ShowWindow()
        {
            GetWindow<SpriteExportWindow>("Sprite Exporter");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Drag & Drop Sprites or Textures Here", EditorStyles.boldLabel);

            // Khu vực kéo thả
            Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop Sprites/Textures Here", EditorStyles.helpBox);

            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (Object dragged in DragAndDrop.objectReferences)
                        {
                            if (dragged is Texture2D || dragged is Sprite)
                            {
                                objectsToExport.Add(dragged);
                            }
                        }
                    }
                    evt.Use();
                }
            }

            // Hiển thị danh sách object đã thêm
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(150));
            for (int i = 0; i < objectsToExport.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                objectsToExport[i] = EditorGUILayout.ObjectField(objectsToExport[i], typeof(Object), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    objectsToExport.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear List"))
            {
                objectsToExport.Clear();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Export All to Folder"))
            {
                ExportAll();
            }
        }

        private void ExportAll()
        {
            if (objectsToExport.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No sprites or textures in the list!", "OK");
                return;
            }

            string path = EditorUtility.SaveFolderPanel("Select Export Folder", "", "");
            if (string.IsNullOrEmpty(path)) return;

            foreach (Object obj in objectsToExport)
            {
                if (obj is Texture2D tex)
                {
                    string texPath = AssetDatabase.GetAssetPath(tex);
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texPath);
                    foreach (var asset in assets)
                    {
                        if (asset is Sprite sprite)
                        {
                            ExportSprite(sprite, path);
                        }
                    }
                }
                else if (obj is Sprite sprite)
                {
                    ExportSprite(sprite, path);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", "Sprites exported successfully!", "OK");
        }

        private void ExportSprite(Sprite sprite, string outputDirectoryPath)
        {
            Texture2D source = sprite.texture;
            Rect rect = sprite.textureRect;

            Texture2D newTexture = new Texture2D((int)rect.width, (int)rect.height);
            Color[] pixels = source.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            );
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            string outPath = Path.Combine(outputDirectoryPath, sprite.name + ".png");
            File.WriteAllBytes(outPath, newTexture.EncodeToPNG());
        }
    }
}
