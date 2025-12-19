using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameTool.Assistants.ParticleSystemPreview.Editor
{
    [CustomEditor(typeof(GameObject)), CanEditMultipleObjects]
    public class ParticleSystemGameObjectEditor : OverrideEditor
    {
        private class Styles
        {
            public GUIContent ps = new GUIContent("PS", "Show particle system preview");
            public GUIStyle preButton = "preButton";
        }

        private bool m_ShowParticlePreview;

        private int m_DefaultHasPreview;

        private ParticleSystemPreview m_Preview;

        private static Styles s_Styles;

        private ParticleSystemPreview preview
        {
            get
            {
                if (m_Preview == null)
                {
                    m_Preview = new ParticleSystemPreview();
                    m_Preview.SetEditor(this);
                    m_Preview.Initialize(targets);
                }

                return m_Preview;
            }
        }

        protected override UnityEditor.Editor GetBaseEditor()
        {
            UnityEditor.Editor editor = null;
            var baseType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
            CreateCachedEditor(targets, baseType, ref editor);
            return editor;
        }

        void OnEnable()
        {
            m_ShowParticlePreview = true;
        }

        void OnDisable()
        {
#if UNITY_2021_1_OR_NEWER
            preview.Cleanup();
#endif

            try
            {
                var action = new Action(() =>
                {
                    if (baseEditor)
                    {
                        DestroyImmediate(baseEditor);
                    }
                });
                if (this)
                {
                    action();
                }
            }
            catch
            {
                // ignored
            }
            
            preview?.OnDestroy();
        }

        private bool HasParticleSystemPreview()
        {
            return preview.HasPreviewGUI();
        }

        private bool HasBasePreview()
        {
            if (m_DefaultHasPreview == 0)
            {
                m_DefaultHasPreview = baseEditor.HasPreviewGUI() ? 1 : -1;
            }

            return m_DefaultHasPreview == 1;
        }

        private bool IsShowParticleSystemPreview()
        {
            return HasParticleSystemPreview() && m_ShowParticlePreview;
        }

        public override bool HasPreviewGUI()
        {
            return HasParticleSystemPreview() || HasBasePreview();
        }

        public override GUIContent GetPreviewTitle()
        {
            return IsShowParticleSystemPreview() ? preview.GetPreviewTitle() : baseEditor.GetPreviewTitle();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (IsShowParticleSystemPreview())
            {
                preview.OnPreviewGUI(r, background);
            }
            else
            {
                baseEditor.OnPreviewGUI(r, background);
            }
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (IsShowParticleSystemPreview())
            {
                preview.OnInteractivePreviewGUI(r, background);
            }
            else
            {
                baseEditor.OnInteractivePreviewGUI(r, background);
            }
        }

        public override void OnPreviewSettings()
        {
            s_Styles ??= new Styles();
            if (HasBasePreview() && HasParticleSystemPreview())
            {
                m_ShowParticlePreview = GUILayout.Toggle(m_ShowParticlePreview, s_Styles.ps, s_Styles.preButton);
                if (GUILayout.Button("2D"))
                {
                    if (preview != null)
                    {
                        preview.DoAvatarPreviewOrbit(Event.current, Vector2.zero);
                    }
                }

                if (GUILayout.Button("^"))
                {
                    if (preview != null)
                    {
                        preview.DoAvatarPreviewOrbit(Event.current, new Vector2(0, -90));
                    }
                }

                if (GUILayout.Button(">"))
                {
                    if (preview != null)
                    {
                        preview.DoAvatarPreviewOrbit(Event.current, new Vector2(90, 0));
                    }
                }
            }

            if (IsShowParticleSystemPreview())
            {
                preview.OnPreviewSettings();
            }
            else
            {
                baseEditor.OnPreviewSettings();
            }
        }

        public override string GetInfoString()
        {
            return IsShowParticleSystemPreview() ? preview.GetInfoString() : baseEditor.GetInfoString();
        }

        public override void ReloadPreviewInstances()
        {
            if (IsShowParticleSystemPreview())
            {
                preview.ReloadPreviewInstances();
            }
            else
            {
                baseEditor.ReloadPreviewInstances();
            }
        }

        /// <summary>
        /// 需要调用 GameObjectInspector 的场景拖曳，否则无法拖动物体到 Scene 视图
        /// </summary>
        /// <param name="sceneView"></param>
        /// <param name="index"></param>
        public void OnSceneDrag(SceneView sceneView, int index)
        {
            System.Type t = baseEditor.GetType();
            MethodInfo onSceneDragMi = t.GetMethod("OnSceneDrag",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (onSceneDragMi != null)
            {
                // Lấy danh sách tham số của phương thức được gọi bằng delegate
                ParameterInfo[] parameters = onSceneDragMi.GetParameters();

                // Kiểm tra số lượng tham số
                if (parameters.Length == 1)
                {
                    // Tạo một mảng object[] chứa tham số truyền vào Invoke
                    object[] invokeParams = new object[] { sceneView };

                    // Invoke phương thức với tham số tương ứng
                    onSceneDragMi.Invoke(baseEditor, invokeParams);
                }
            }
        }
    }
}