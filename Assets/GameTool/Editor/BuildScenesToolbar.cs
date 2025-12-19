using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// https://github.com/marijnz/unity-toolbar-extender.git
using UnityToolbarExtender;

namespace GameTool.Editor
{
    [InitializeOnLoad]
    public static class BuildScenesToolbar
    {
        // số scene tối đa sẽ hiển thị
        private const int MaxScenes = 5;

        static BuildScenesToolbar()
        {
            // Thêm GUI vào bên trái toolbar
            // https://github.com/marijnz/unity-toolbar-extender.git
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

            // Nếu thích bên phải thì dùng:
            // ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            // Lấy các scene active trong Build Settings
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Take(MaxScenes)
                .ToArray();

            if (scenes.Length == 0)
            {
                GUILayout.Label("No active build scenes");
                return;
            }

            GUILayout.Space(10);
            GUILayout.Label("Scenes:");

            foreach (var scene in scenes)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scene.path);

                if (GUILayout.Button(sceneName, GUILayout.Width(90)))
                {
                    OpenSceneWithSaveCheck(scene.path);
                }
            }
        }

        private static void OpenSceneWithSaveCheck(string scenePath)
        {
            // Nếu đang ở đúng scene rồi thì thôi
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path == scenePath)
                return;

            // Hỏi user có muốn save các scene đang mở không
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return; // user bấm Cancel

            // Mở scene được chọn
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}