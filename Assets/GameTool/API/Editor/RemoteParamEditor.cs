#if USE_FIREBASE
using UnityEngine;
using UnityEditor;
using GameToolSample.Scripts.FirebaseServices;

[CustomPropertyDrawer(typeof(RemoteParam))]
public class RemoteParamEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        var paramType = new Rect(position.x, position.y, position.width / 5, 20);
        var paramText = new Rect(paramType.x + paramType.width + 10, paramType.y - 2, 100, 20);
        var paramName = new Rect(paramText.x + 100, position.y, position.width / 2, 20);

        EditorGUI.PropertyField(paramType, property.FindPropertyRelative("variableType"), GUIContent.none);
        EditorGUI.LabelField(paramText, "Variable Name");
        EditorGUI.PropertyField(paramName, property.FindPropertyRelative("paramName"), GUIContent.none);
        //    EditorGUI.LabelField(new Rect(paramType.x + paramType.width + 10, paramType.y - 2, 50, 20), "DefaultValue");

        //if (GUI.Button(buttonCopy, "Copy Key"))
        //    CopyKey();

        //if (GUI.Button(buttonCopy, "Copy Data"))
        //    CopyData();

        EditorGUI.EndProperty();
    }
}
#endif
