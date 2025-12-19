using UnityEditor;

namespace DatdevUlts.DateTimeScripts.Editor
{
    [CustomEditor(typeof(DateTimeManager))]
    public class DateTimeManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var dateTimeManager = (DateTimeManager)target;
            Undo.RecordObject(dateTimeManager, "DateTimeManager");
            var typeRequestValue = dateTimeManager.TypeRequestValue;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_typeRequestValue"));
            
            if (typeRequestValue != DateTimeManager.TypeRequest.Local)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_timeOutPerRequest"));
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_useLocalDateTimeOnNetError"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxRequestTimes"));
            }

            if (typeRequestValue == DateTimeManager.TypeRequest.Https || typeRequestValue == DateTimeManager.TypeRequest.NetworkMixed)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("https"));
            }

            if (typeRequestValue == DateTimeManager.TypeRequest.NTP || typeRequestValue == DateTimeManager.TypeRequest.NetworkMixed)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ntpLinks"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}