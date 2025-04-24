#if UNITY_EDITOR
#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static VInspector.Libs.VUtils;
using static VInspector.Libs.VGUI;
using Object = UnityEngine.Object;
#endregion

namespace VInspector
{
#if !DISABLED
[CustomPropertyDrawer(typeof(Object), true)]
[CustomPropertyDrawer(typeof(int), true)]
[CustomPropertyDrawer(typeof(float), true)]
[CustomPropertyDrawer(typeof(string), true)]
[CustomPropertyDrawer(typeof(bool), true)]
[CustomPropertyDrawer(typeof(RangeResettable))]
#endif
class VIResettablePropDrawer : PropertyDrawer
{
	public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
	{
		bool isResetted = true;
		bool actAsPrefabModifiation;
		bool isObjectField = typeof(Object).IsAssignableFrom(fieldInfo.FieldType);
		bool isArrayElement = prop.propertyPath.Split('.').Last().StartsWith("data[");

		void buttonInteraction()
		{
			if (!GUI.enabled) return;
			if (isArrayElement) return;

			object parent = GetParent(prop);

			if (!scriptTypesWithVInspector.Contains(parent.GetType())) return;

			if (parent.GetType() != fieldInfo.DeclaringType) return;

			if (!parentsWithDefaultValues.ContainsKey(parent.GetType()))

				if (parent is MonoBehaviour)
#if UNITY_2023_2_OR_NEWER
				{
					GameObject go = EditorUtility.CreateGameObjectWithHideFlags("Dummy object for fetching default variable values for vInspector's resettable variables feature", HideFlags.HideAndDontSave, parent.GetType());

					try { parentsWithDefaultValues[parent.GetType()] = go.GetComponent(parent.GetType()); }
					finally { Object.DestroyImmediate(go); }
				}
#else
                    parentsWithDefaultValues[parent.GetType()] = ScriptableObject.CreateInstance(parent.GetType());
#endif
				else if (parent is ScriptableObject) parentsWithDefaultValues[parent.GetType()] = ScriptableObject.CreateInstance(parent.GetType());

				else if (parent.GetType().GetConstructor(Type.EmptyTypes) != null) parentsWithDefaultValues[parent.GetType()] = Activator.CreateInstance(parent.GetType());

				else return;

			if (!parentsWithDefaultValues.TryGetValue(parent.GetType(), out object parentWithDefaultValues) || parentWithDefaultValues == null) return;

			object valCur = fieldInfo.GetValue(parent);
			object valDefault = fieldInfo.GetValue(parentWithDefaultValues);

			actAsPrefabModifiation = prop.isInstantiatedPrefab && !holdingAlt;

			if (actAsPrefabModifiation) isResetted = !prop.prefabOverride;
			else
			{
				isResetted = Equals(valCur, valDefault);

				if (typeof(Object).IsAssignableFrom(fieldInfo.FieldType)) isResetted |= valDefault == null && !(bool) (Object) valCur;

				if (fieldInfo.FieldType == typeof(string)) isResetted |= fieldInfo.FieldType == typeof(string) && Equals(valCur, "") && Equals(valDefault, null);
			}

			if (_ResetFieldButton(rect, isObjectField))
			{
				prop.serializedObject.targetObject.RecordUndo();

				PrefabUtility.IsPartOfPrefabInstance(prop.serializedObject.targetObject);

				if (actAsPrefabModifiation) PrefabUtility.RevertPropertyOverride(prop, InteractionMode.AutomatedAction);
				else
				{
					if (parent.GetType().IsValueType) fieldInfo.SetValueDirect(__makeref(parent), valDefault);
					else fieldInfo.SetValue(parent, valDefault);
				}

				GUI.FocusControl(null);

				prop.serializedObject.targetObject.Dirty();
				prop.serializedObject.ApplyModifiedProperties();
			}

			if ((e.keyDown() || e.keyUp()) && holdingAlt) prop.serializedObject.targetObject.Dirty();
		}

		void defaultPropDrawer()
		{
			EditorGUI.BeginProperty(rect, label, prop);

			if (attribute is RangeResettable)
			{
				var range = attribute as RangeResettable;

				if (prop.propertyType == SerializedPropertyType.Float) EditorGUI.Slider(rect, prop, range.min, range.max, label);
				else if (prop.propertyType == SerializedPropertyType.Integer) EditorGUI.IntSlider(rect, prop, (int) range.min, (int) range.max, label);
			}
			else { EditorGUI.PropertyField(rect, prop, label); }

			EditorGUI.EndProperty();
		}

		void buttonIcon()
		{
			if (!GUI.enabled) return;
			if (isArrayElement) return;

			if (isResetted) return;

			_DrawResettableFieldCrossIcon(rect, isObjectField);
		}

		buttonInteraction();
		defaultPropDrawer();
		buttonIcon();
	}
	public static Dictionary<Type, object> parentsWithDefaultValues = new ();
	public static List<Type> scriptTypesWithVInspector = new ();

	public object GetParent(SerializedProperty prop)
	{
		if (prop.serializedObject.targetObject.GetType() == fieldInfo.ReflectedType) return prop.serializedObject.targetObject;

		string path = prop.propertyPath;
		object cur = prop.serializedObject.targetObject;
		object prev = null;
		int i = 0;

		while (cur != null && HasInChildren(prop.propertyPath, ref i, out PropertyPathComponent token))
		{
			prev = cur;
			cur = MoveToNext(cur, token);
		}

		return prev;
	}
	bool HasInChildren(string propertyPath, ref int index, out PropertyPathComponent component)
	{
		component = new ();

		if (index >= propertyPath.Length) return false;

		Match arrayElementMatch = arrayElementRegex.Match(propertyPath, index);

		if (arrayElementMatch.Success)
		{
			index += arrayElementMatch.Length + 1;
			component.elementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
			return true;
		}

		int dot = propertyPath.IndexOf('.', index);

		if (dot == -1)
		{
			component.propertyName = propertyPath.Substring(index);
			index = propertyPath.Length;
		}
		else
		{
			component.propertyName = propertyPath.Substring(index, dot - index);
			index = dot + 1;
		}

		return true;
	}
	object MoveToNext(object container, PropertyPathComponent component)
	{
		if (component.propertyName == null) return container is IList list && component.elementIndex.IsInRangeOf(list) ? list[component.elementIndex] : null;
		return GetMemberValue(container, component.propertyName);
	}
	static object GetMemberValue(object container, string name)
	{
		if (container == null) return null;
		Type type = container.GetType();
		MemberInfo[] members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

		for (int i = 0; i < members.Length; ++i)
		{
			if (members[i] is FieldInfo field) return field.GetValue(container);
			else if (members[i] is PropertyInfo property)
				try { return property.GetValue(container); } catch { return null; }
		}

		return null;
	}
	static Regex arrayElementRegex = new (@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);

	struct PropertyPathComponent
	{
		public string propertyName;
		public int elementIndex;
	}
}
}
#endif
