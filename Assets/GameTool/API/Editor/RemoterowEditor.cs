using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomPropertyDrawer(typeof(RemoteRow))]
//public class RemoterowEditor : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        EditorGUI.BeginProperty(position, label, property);
//        var remoteKey = new Rect(position.x, position.y, position.width / 5, 20);
//        var param = new Rect(position.x, position.y, position.width , 20);

 
//        EditorGUI.PropertyField(remoteKey, property.FindPropertyRelative("remoteKey"), GUIContent.none);
//        EditorGUI.PropertyField(param, property.FindPropertyRelative("param"), GUIContent.none);
//        //    EditorGUI.LabelField(new Rect(paramType.x + paramType.width + 10, paramType.y - 2, 50, 20), "DefaultValue");

//        //if (GUI.Button(buttonCopy, "Copy Key"))
//        //    CopyKey();

//        //if (GUI.Button(buttonCopy, "Copy Data"))
//        //    CopyData();

//        EditorGUI.EndProperty();
//    }
//}
