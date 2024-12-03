using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDictionary), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    private SerializedProperty listProperty;

    private SerializedProperty getListProperty(SerializedProperty property)
    {
        if (listProperty == null)
            listProperty = property.FindPropertyRelative("list");

        return listProperty;

    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, getListProperty(property), label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(getListProperty(property), true);
    }
}
