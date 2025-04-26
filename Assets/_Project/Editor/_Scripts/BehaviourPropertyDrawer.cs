using System;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Behaviour))]
public class BehaviourPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Calculate rects for each property field
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        Rect currentRect = position;
        currentRect.height = lineHeight;

        // Get isCurrentBehaviour property
        var isCurrentProp = property.FindPropertyRelative("isCurrentBehaviour");
        Color defaultColor = GUI.backgroundColor;

        // Change background color if this is the current behaviour
        if (isCurrentProp.boolValue)
        {
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // Light green with transparency
        }

        // Initialize properties
        var descriptionProp = property.FindPropertyRelative("description");
        var typeProp = property.FindPropertyRelative("type");
        var durationProp = property.FindPropertyRelative("duration");
        Behaviour.Type behaviourType = (Behaviour.Type) typeProp.enumValueIndex;

        // Instead of using StringBuilder with label.text, create a new GUIContent
        GUIContent customLabel = new GUIContent(string.Empty); // Start with empty string
        StringBuilder labelBuilder = new StringBuilder();

        switch (behaviourType)
        {
            case Behaviour.Type.Move:
                labelBuilder.Append($"{behaviourType.ToString()}s ");
                labelBuilder.Append("to ");
                var positionProp = property.FindPropertyRelative("position");
                labelBuilder.Append(positionProp.vector2Value.ToString());
                break;

            case Behaviour.Type.Attack:
                var attackProp = property.FindPropertyRelative("attack");
                labelBuilder.Append("\"").Append(attackProp.enumDisplayNames[attackProp.enumValueIndex]).Append("\"");
                labelBuilder.Append(" at ");
                positionProp = property.FindPropertyRelative("position");
                labelBuilder.Append(positionProp.vector2Value.ToString());
                break;

            case Behaviour.Type.Dialogue:
                var dialogueProp = property.FindPropertyRelative("dialogue");
                labelBuilder.Append("\"").Append(dialogueProp.stringValue).Append("\"");
                break;

            case Behaviour.Type.Wait:
                labelBuilder.Append("Waiting for ");
                labelBuilder.Append(durationProp.floatValue);
                labelBuilder.Append(" seconds");
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Create the custom label
        customLabel.text = labelBuilder.ToString();

        // Use the custom label instead of the original one
        EditorGUI.LabelField(currentRect, customLabel, EditorStyles.boldLabel);

        currentRect.y += lineHeight + spacing;

        // Always show description and type
        // EditorGUI.PropertyField(currentRect, descriptionProp);
        // currentRect.y += lineHeight + spacing;
        
        EditorGUI.PropertyField(currentRect, typeProp);
        currentRect.y += lineHeight + spacing;

        // Show different fields based on the behaviour type

        switch (behaviourType)
        {
            case Behaviour.Type.Move:
                var positionProp = property.FindPropertyRelative("position");
                
                EditorGUI.PropertyField(currentRect, positionProp);
                currentRect.y += lineHeight + spacing;
                currentRect.height = lineHeight * 1.35f;
                EditorGUI.PropertyField(currentRect, durationProp);
                break;

            case Behaviour.Type.Attack:
                var attackProp = property.FindPropertyRelative("attack");
                var showWarningProp = property.FindPropertyRelative("showWarning");
                positionProp = property.FindPropertyRelative("position");
                var delayProp = property.FindPropertyRelative("delay");

                EditorGUI.PropertyField(currentRect, attackProp);
                currentRect.y += lineHeight + spacing;
                EditorGUI.PropertyField(currentRect, showWarningProp);
                currentRect.y += lineHeight + spacing;
                EditorGUI.PropertyField(currentRect, positionProp);
                currentRect.y += lineHeight + spacing;
                currentRect.height = lineHeight * 1.35f;
                EditorGUI.PropertyField(currentRect, durationProp);
                currentRect.y += lineHeight + spacing * 3.5f;
                currentRect.height = lineHeight * 1f;
                EditorGUI.PropertyField(currentRect, delayProp);
                break;

            case Behaviour.Type.Dialogue:
                var dialogueProp = property.FindPropertyRelative("dialogue");

                EditorGUI.PropertyField(currentRect, dialogueProp);
                currentRect.y += lineHeight + spacing;
                currentRect.height = lineHeight * 1.35f;
                EditorGUI.PropertyField(currentRect, durationProp);
                break;

            case Behaviour.Type.Wait:
                // Make the duration field taller
                currentRect.height = lineHeight * 1.35f;
                EditorGUI.PropertyField(currentRect, durationProp);
                break;

        }

        // Restore the original background color
        GUI.backgroundColor = defaultColor;


        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        
        // Base height for label, description and type
        float totalHeight = (lineHeight + spacing) * 3;

        // Add height based on type
        var typeProp = property.FindPropertyRelative("type");
        Behaviour.Type behaviourType = (Behaviour.Type)typeProp.enumValueIndex;
        
        switch (behaviourType)
        {
            case Behaviour.Type.Move:
                totalHeight += (lineHeight + spacing) * 2; // position, duration
                break;
            case Behaviour.Type.Attack:
                totalHeight += (lineHeight + spacing) * 5; // position, attack, showWarning, duration, delay
                break;
            case Behaviour.Type.Dialogue:
                totalHeight += (lineHeight + spacing) * 2; // dialogue, duration
                break;
            case Behaviour.Type.Wait:
                totalHeight += (lineHeight + spacing) * 1; // duration
                break;
        }

        // Add extra padding at the bottom
        totalHeight += spacing;
        
        return totalHeight;
    }
}