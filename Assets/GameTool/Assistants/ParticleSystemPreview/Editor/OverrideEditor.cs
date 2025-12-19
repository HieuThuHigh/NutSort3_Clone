using System.Reflection;
using UnityEngine;

// thanks to Sprite Animation Preview
namespace GameTool.Assistants.ParticleSystemPreview.Editor
{
    public abstract class OverrideEditor : UnityEditor.Editor
    {
        readonly MethodInfo methodInfo = typeof (UnityEditor.Editor).GetMethod("OnHeaderGUI",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private UnityEditor.Editor m_BaseEditor;

        protected UnityEditor.Editor baseEditor
        {
            get => m_BaseEditor ? m_BaseEditor : (m_BaseEditor = GetBaseEditor());
            set => m_BaseEditor = value;
        }

        protected abstract UnityEditor.Editor GetBaseEditor();

        public override void OnInspectorGUI()
        {
            baseEditor.OnInspectorGUI();
        }

        public override bool HasPreviewGUI()
        {
            return baseEditor.HasPreviewGUI();
        }

        public override GUIContent GetPreviewTitle()
        {
            return baseEditor.GetPreviewTitle();
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return baseEditor.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            baseEditor.OnPreviewGUI(r, background);
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            baseEditor.OnInteractivePreviewGUI(r, background);
        }

        public override void OnPreviewSettings()
        {
            baseEditor.OnPreviewSettings();
        }

        public override string GetInfoString()
        {
            return baseEditor.GetInfoString();
        }

        public override void ReloadPreviewInstances()
        {
            baseEditor.ReloadPreviewInstances();
        }

        protected override void OnHeaderGUI()
        {
            methodInfo.Invoke(baseEditor, new object[0]);
        }

        public override bool RequiresConstantRepaint()
        {
            return baseEditor.RequiresConstantRepaint();
        }

        public override bool UseDefaultMargins()
        {
            return baseEditor.UseDefaultMargins();
        }
    }
}