#region
using System;
using UnityEditor;
using UnityEngine;
#endregion

[CustomEditor(typeof(StatusEffect), true)] [CanEditMultipleObjects]
public class StatusEffectEditor : Editor
{
	SerializedProperty statusName;
	SerializedProperty description;
	SerializedProperty duration;
	new SerializedProperty target;
	SerializedProperty timing;
	SerializedProperty time;
	SerializedProperty caster;

	void OnEnable()
	{
		statusName = serializedObject.FindProperty("statusName");
		description = serializedObject.FindProperty("description");
		duration = serializedObject.FindProperty("duration");
		target = serializedObject.FindProperty("target");
		timing = serializedObject.FindProperty("timing");
		time = serializedObject.FindProperty("time");
		caster = serializedObject.FindProperty("caster");
	}

	bool showInfo = true;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		var headerButtonStyle = new GUIStyle(GUI.skin.label)
		{ alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };

		using (new GUILayout.HorizontalScope("textField")) { showInfo = EditorGUILayout.BeginFoldoutHeaderGroup(showInfo, "Ability Info", headerButtonStyle); }

		{
			if (showInfo)
			{
				using (new GUILayout.VerticalScope("box"))
				{
					EditorGUILayout.PropertyField(statusName);
					EditorGUILayout.PropertyField(description);

					if (Application.isPlaying) EditorGUILayout.LabelField("Time Remaining", time.floatValue.ToString("F1"), GUI.skin.textField);
					else EditorGUILayout.PropertyField(duration);

					EditorGUILayout.PropertyField(target);

					switch (timing.enumValueIndex)
					{
						case 0: {
							var earlyContent = new GUIContent("Prefix", "Applies before damage is applied.");
							timing.enumValueIndex = EditorGUILayout.Popup(earlyContent, timing.enumValueIndex, Enum.GetNames(typeof(StatusEffect.Timing)));
							break;
						}

						case 1: {
							var lateContent = new GUIContent("Postfix", "Applies after damage is applied.");
							timing.enumValueIndex = EditorGUILayout.Popup(lateContent, timing.enumValueIndex, Enum.GetNames(typeof(StatusEffect.Timing)));
							break;
						}
					}
				}
			}

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		serializedObject.ApplyModifiedProperties();
	}
}
