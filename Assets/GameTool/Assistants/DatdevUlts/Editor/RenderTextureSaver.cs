using UnityEditor;
using UnityEngine;
using System.IO;

public class RenderTextureSaver : EditorWindow
{
    private Camera cam;
    private string fileName = "RenderTextureOutput";
    private string folderPath = "Assets";

    [MenuItem("Tools/RenderTexture Saver")]
    public static void ShowWindow()
    {
        GetWindow<RenderTextureSaver>("Save RenderTexture to PNG");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save RenderTexture as PNG", EditorStyles.boldLabel);

        cam = (Camera)EditorGUILayout.ObjectField("Render Texture", cam, typeof(Camera), true);
        fileName = EditorGUILayout.TextField("File Name", fileName);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        GUILayout.Space(10);

        if (GUILayout.Button("Save as PNG"))
        {
            if (cam == null || cam.targetTexture == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a RenderTexture.", "OK");
                return;
            }

            SaveRenderTextureAsPNG(cam.targetTexture, fileName, folderPath);
        }
    }

    private void SaveRenderTextureAsPNG(RenderTexture rt, string fileName, string folder)
    {
        // Ensure folder exists
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        // Save current RT
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        string fullPath = Path.Combine(folder, fileName + ".png");
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log("Saved PNG to: " + fullPath);
        AssetDatabase.Refresh();

        // Cleanup
        RenderTexture.active = currentRT;
        Object.DestroyImmediate(tex);
    }
}
