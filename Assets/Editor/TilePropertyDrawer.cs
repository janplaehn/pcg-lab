using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// IngredientDrawer
[CustomPropertyDrawer(typeof(CAMapGenerator.TileNeighbours))]
public class IngredientDrawer : PropertyDrawer {
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var leftRect = new Rect(position.x, position.y, 10f, position.height);
        var rightRect = new Rect(position.x + 40, position.y, 10, position.height);
        var topRect = new Rect(position.x + 80, position.y, 10, position.height);
        var bottomRect = new Rect(position.x + 120, position.y, 10, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(leftRect, property.FindPropertyRelative("left"), GUIContent.none);
        EditorGUI.PropertyField(rightRect, property.FindPropertyRelative("right"), GUIContent.none);
        EditorGUI.PropertyField(topRect, property.FindPropertyRelative("top"), GUIContent.none);
        EditorGUI.PropertyField(bottomRect, property.FindPropertyRelative("bottom"), GUIContent.none);
        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}