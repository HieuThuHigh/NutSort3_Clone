#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor;
using VInspector;

[ExecuteAlways]
public class TextMeshProManager : ScriptableObject
{
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    public List<TMP_FontAsset> fontAssets = new List<TMP_FontAsset>();
    public TMP_FontAsset selectedFontAsset;
    public List<Material> listMaterialsOfSelectedFontAsset = new List<Material>();
    public string textCheck;

    // Called automatically in Editor whenever values change
    private void OnValidate()
    {
        RefreshFontAssets();
        ClearAllFontData();
    }

    /// <summary>
    /// Tìm tất cả TMP_FontAsset trong project và lưu vào list
    /// </summary>
    [Button]
    public void RefreshFontAssets()
    {
        fontAssets.Clear();

        // Tìm tất cả GUID của các font asset
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (asset != null)
            {
                if (asset.atlasPopulationMode == AtlasPopulationMode.Dynamic)
                {
                    fontAssets.Add(asset);
                }
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[TextMeshProManager] Found {fontAssets.Count} TMP Font Assets");
    }

    /// <summary>
    /// Ví dụ hàm dùng để clear font atlas + glyph (giống warning trong ảnh)
    /// </summary>
    [Button]
    public void ClearAllFontData()
    {
        foreach (var font in fontAssets)
        {
            if (font != null)
            {
                Undo.RecordObject(font, "Clear TMP Font Data");
                font.ClearFontAssetData(true); // true = reset atlas texture
                EditorUtility.SetDirty(font);
            }
        }

        EditorUtility.SetDirty(this);
        Debug.LogWarning("[TextMeshProManager] All TMP font atlas + glyph data cleared!");
    }
    
    [Button]
    public void GetAllMaterialsOfSelectedFontAsset()
    {
        listMaterialsOfSelectedFontAsset.Clear();
        listMaterialsOfSelectedFontAsset.AddRange(GetMaterialsUsingFontAtlas(selectedFontAsset));
        EditorUtility.SetDirty(this);
        
        // Thêm lệnh selection tt cả trong listMaterialsOfSelectedFontAsset
        Selection.objects = listMaterialsOfSelectedFontAsset.ToArray();
    }
    
    public static List<Material> GetMaterialsUsingFontAtlas(TMP_FontAsset fontAsset)
    {
        List<Material> results = new List<Material>();

        if (fontAsset == null || fontAsset.atlasTexture == null)
            return results;

        Texture2D targetAtlas = fontAsset.atlasTexture;

        // Lấy toàn bộ material trong project
        string[] guids = AssetDatabase.FindAssets("t:Material");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat == null)
                continue;

            // Kiểm tra shader TMP
            if (!mat.shader.name.Contains("TextMeshPro"))
                continue;

            // Texture atlas mà material đang dùng
            Texture tex = mat.GetTexture(MainTex);

            if (tex == targetAtlas)
            {
                results.Add(mat);
            }
        }

        return results;
    }
    
    [Button]
    public void CheckText()
    {
        // Kiểm tra xem source font của font selected có hỗ trợ ký tự trong textCheck không
        var sourceFont = selectedFontAsset.sourceFontFile;
        var hasChar = sourceFont.HasCharacter(textCheck[0]);
        Debug.LogError($"hasChar: {hasChar}");
    }
}
#else
    public class TextMeshProManager : ScriptableObject
    {
    }
#endif