using UnityEngine;
using UnityEditor;
using Config;

[CustomEditor(typeof(MoveBody))]
public class EditorAttributes : Editor
{
    SerializedProperty isClockwise;

    void OnEnable()
    {
        isClockwise = serializedObject.FindProperty("IsClockwise");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(isClockwise);
        serializedObject.ApplyModifiedProperties();
        isClockwise.arraySize = 3;
    }

    public static void Show(SerializedProperty list)
    {
        EditorGUILayout.PropertyField(list);
        for (int i = 0; i < list.arraySize; i++)
        {
            EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
        }
    }
}