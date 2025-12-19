using UnityEditor;
using UnityEngine;

namespace DatdevUlts.Editor
{
    public static class CopyReferenceMenu
    {
        [MenuItem("CONTEXT/Component/Copy Reference")]
        private static void CopyComponentReference(MenuCommand command)
        {
            Object obj = command.context;
            CopyObjectReference(obj);
        }

        [MenuItem("GameObject/Copy Reference", false, 49)]
        private static void CopyGameObjectReference(MenuCommand command)
        {
            Object obj = Selection.activeObject;
            if (obj != null)
                CopyObjectReference(obj);
        }

        private static void CopyObjectReference(Object obj)
        {
            if (obj == null)
                return;

            int instanceId = obj.GetInstanceID();

            // Lấy GUID + Local ID của object (nếu có asset)
            string guid = "";
            long localId = 0;

            string path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out localId);
            }

            int type = (int)PrefabUtility.GetPrefabInstanceStatus(obj);

            string json =
                $"UnityEditor.ObjectWrapperJSON:{{\"guid\":\"{guid}\",\"localId\":{localId},\"type\":{type},\"instanceID\":{instanceId}}}";

            EditorGUIUtility.systemCopyBuffer = json;

            Debug.Log($"Copied reference: {json}");
        }
    }
}