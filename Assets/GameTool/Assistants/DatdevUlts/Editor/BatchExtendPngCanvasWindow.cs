// Assets/Editor/BatchExtendPngCanvasWindow.cs
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class BatchExtendPngCanvasWindow : EditorWindow
{
    private List<Object> list = new List<Object>();
    private Vector2 scroll;

    private enum ExtendMode { Pixel, Percent }
    private ExtendMode extendMode = ExtendMode.Pixel;

    // Pixel mode: thêm X px mỗi cạnh
    private int pixelsPerSide = 16;

    // Percent mode: tăng tổng W/H lên %
    private float percentIncrease = 20f;

    // Trim options
    private bool enableTrim = false;
    private int trimAlphaThreshold = 1; // 0..255, cắt viền có alpha < threshold
    private bool trimThenExtend = true; // nếu bật Extend, thứ tự: Trim -> Extend

    // Tuỳ chọn nhỏ
    private bool makeBackup = false; // tạo file .bak cạnh file gốc

    [MenuItem("Tools/Batch Extend PNG Canvas")]
    public static void ShowWindow()
    {
        var w = GetWindow<BatchExtendPngCanvasWindow>("Batch Extend/Trim PNG");
        w.minSize = new Vector2(560, 360);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Drag & Drop Sprites or Textures Here", EditorStyles.boldLabel);

        Rect dropArea = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
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
                            list.Add(dragged);
                    }
                }
                evt.Use();
            }
        }

        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(180));
        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            list[i] = EditorGUILayout.ObjectField(list[i], typeof(Object), false);
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                list.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Clear List")) list.Clear();
            if (GUILayout.Button("Remove Null")) list.RemoveAll(o => o == null);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Trim Options", EditorStyles.boldLabel);
        enableTrim = EditorGUILayout.Toggle(new GUIContent("Enable Trim (alpha)","Cắt viền trong suốt dựa trên alpha"), enableTrim);
        using (new EditorGUI.DisabledScope(!enableTrim))
        {
            trimAlphaThreshold = EditorGUILayout.IntSlider(new GUIContent("Alpha Threshold (0-255)", "Pixel có alpha < threshold bị coi là trong suốt khi tìm biên"), trimAlphaThreshold, 0, 255);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Extend Options", EditorStyles.boldLabel);
        extendMode = (ExtendMode)EditorGUILayout.EnumPopup("Extend Mode", extendMode);
        if (extendMode == ExtendMode.Pixel)
        {
            pixelsPerSide = EditorGUILayout.IntField("Pixels per side", Mathf.Max(0, pixelsPerSide));
            EditorGUILayout.HelpBox("Pixel: thêm X px vào MỖI CẠNH (trái/phải/trên/dưới).", MessageType.Info);
        }
        else
        {
            percentIncrease = EditorGUILayout.FloatField("Increase (%)", Mathf.Max(0f, percentIncrease));
            EditorGUILayout.HelpBox("Percent: tăng TỔNG kích thước W & H theo %. Ảnh gốc đặt giữa, phần tăng chia đều 4 phía.", MessageType.Info);
        }

        trimThenExtend = EditorGUILayout.Toggle(new GUIContent("Trim → Extend order","Nếu bật Trim, chọn thứ tự xử lý. Bật = Trim rồi Extend."), trimThenExtend);
        makeBackup = EditorGUILayout.Toggle(new GUIContent("Make .bak copy", "Tạo file .bak trước khi ghi đè"), makeBackup);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(list.Count == 0))
        {
            if (GUILayout.Button("Process (Overwrite PNGs)", GUILayout.Height(40)))
                ProcessAll();
        }
    }

    private void ProcessAll()
    {
        if (list.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "List trống.", "OK");
            return;
        }

        var uniqueTexturePaths = new HashSet<string>();
        foreach (var obj in list)
        {
            if (obj == null) continue;
            Texture2D tex = obj as Texture2D;
            if (obj is Sprite s) tex = s.texture;
            if (tex == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(assetPath)) continue;
            if (!assetPath.ToLowerInvariant().EndsWith(".png")) continue; // chỉ PNG

            uniqueTexturePaths.Add(assetPath);
        }

        if (uniqueTexturePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Nothing To Do", "Không tìm thấy PNG hợp lệ trong danh sách.", "OK");
            return;
        }

        int success = 0, fail = 0, skip = 0;
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var assetPath in uniqueTexturePaths)
            {
                string absPath = Path.GetFullPath(assetPath);
                try
                {
                    if (!File.Exists(absPath)) { skip++; continue; }

                    byte[] bytes = File.ReadAllBytes(absPath);
                    if (bytes == null || bytes.Length == 0) { fail++; continue; }

                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                    if (!ImageConversion.LoadImage(tex, bytes, markNonReadable: false))
                    {
                        Object.DestroyImmediate(tex);
                        fail++; continue;
                    }

                    Texture2D result = tex;

                    // --- Pipeline: Trim / Extend theo lựa chọn
                    if (enableTrim && !trimThenExtend)
                    {
                        // Extend trước (hiếm khi cần, vẫn hỗ trợ)
                        result = ExtendTexture(result);
                        if (ReferenceEquals(result, tex) == false) Object.DestroyImmediate(tex);
                        tex = result;

                        result = TrimTexture(tex, trimAlphaThreshold);
                        if (ReferenceEquals(result, tex) == false) Object.DestroyImmediate(tex);
                        tex = result;
                    }
                    else
                    {
                        if (enableTrim)
                        {
                            result = TrimTexture(tex, trimAlphaThreshold);
                            if (ReferenceEquals(result, tex) == false) Object.DestroyImmediate(tex);
                            tex = result;
                        }
                        result = ExtendTexture(tex);
                        if (ReferenceEquals(result, tex) == false) Object.DestroyImmediate(tex);
                        tex = result;
                    }

                    // Backup & overwrite
                    if (makeBackup)
                    {
                        string bakPath = absPath + ".bak";
                        if (!File.Exists(bakPath))
                            File.Copy(absPath, bakPath, overwrite: false);
                    }

                    var outPng = tex.EncodeToPNG();
                    if (outPng == null || outPng.Length == 0)
                        throw new System.Exception("Encode PNG thất bại.");

                    File.WriteAllBytes(absPath, outPng);

                    Object.DestroyImmediate(tex);
                    success++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Process failed: {assetPath}\n{ex}");
                    fail++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Done", $"PNG processed: {success}\nFailed: {fail}\nSkipped: {skip}", "OK");
    }

    private Texture2D ExtendTexture(Texture2D src)
    {
        int srcW = src.width, srcH = src.height;
        int newW, newH, padLeft, padTop;

        if (extendMode == ExtendMode.Pixel)
        {
            int pad = Mathf.Max(0, pixelsPerSide);
            if (pad == 0) return src; // không đổi
            newW = srcW + pad * 2;
            newH = srcH + pad * 2;
            padLeft = pad;
            padTop = pad;
        }
        else
        {
            float k = 1f + Mathf.Max(0f, percentIncrease) / 100f;
            if (k <= 1f + 1e-6f) return src; // không đổi
            newW = Mathf.Max(1, Mathf.RoundToInt(srcW * k));
            newH = Mathf.Max(1, Mathf.RoundToInt(srcH * k));
            padLeft = Mathf.Max(0, (newW - srcW) / 2);
            padTop  = Mathf.Max(0, (newH - srcH) / 2);
        }

        Texture2D dst = new Texture2D(newW, newH, TextureFormat.RGBA32, false, true);
        var transparent = new Color32(0, 0, 0, 0);
        var fill = new Color32[newW * newH];
        for (int i = 0; i < fill.Length; i++) fill[i] = transparent;
        dst.SetPixels32(fill);

        var srcPixels = src.GetPixels32();
        for (int y = 0; y < srcH; y++)
        {
            int dstY = y + padTop;
            if ((uint)dstY >= (uint)newH) continue;

            int srcRowStart = y * srcW;
            int dstRowStart = dstY * newW + padLeft;
            for (int x = 0; x < srcW; x++)
            {
                int dstX = x + padLeft;
                if ((uint)dstX >= (uint)newW) continue;
                dst.SetPixel(dstX, dstY, srcPixels[srcRowStart + x]);
            }
        }

        dst.Apply(false, false);
        return dst;
    }

    /// <summary>
    /// Trim theo alpha: tìm bbox nhỏ nhất chứa pixel có alpha >= threshold; cắt sát bbox.
    /// threshold: 0..255 (byte)
    /// </summary>
    private Texture2D TrimTexture(Texture2D src, int threshold)
    {
        threshold = Mathf.Clamp(threshold, 0, 255);
        int w = src.width, h = src.height;

        Color32[] px = src.GetPixels32();
        int minX = w, minY = h, maxX = -1, maxY = -1;

        // Quét bounding box
        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                byte a = px[row + x].a;
                if (a >= threshold)
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // Không có pixel nào đạt ngưỡng -> giữ nguyên (hoặc có thể trả về 1x1 trong suốt nếu bạn muốn)
        if (maxX < minX || maxY < minY) return src;

        int newW = maxX - minX + 1;
        int newH = maxY - minY + 1;

        // Nếu bbox bằng kích thước cũ -> không cần cắt
        if (newW == w && newH == h) return src;

        Texture2D dst = new Texture2D(newW, newH, TextureFormat.RGBA32, false, true);
        // Copy vùng
        for (int y = 0; y < newH; y++)
        {
            int srcY = y + minY;
            int srcRowStart = srcY * w + minX;
            int dstRowStart = y * newW;
            for (int x = 0; x < newW; x++)
            {
                dst.SetPixel(x, y, px[srcRowStart + x]);
            }
        }

        dst.Apply(false, false);
        return dst;
    }
}
