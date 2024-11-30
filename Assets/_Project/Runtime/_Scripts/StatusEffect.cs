#region
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using DG.Tweening;
using Lumina.Essentials.Attributes;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#endregion

public abstract class StatusEffect : ScriptableObject
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	protected enum Target
	{
		Self,
		Enemy,
	}

	public enum Timing
	{
		Prefix,
		Postfix,
	}

	[SerializeField] protected string statusName;
	[SerializeField] protected string description;
	[SerializeField] protected int duration;
	[SerializeField] protected Target target;
	[SerializeField] public Timing timing;
	[Tooltip("The time remaining for the status effect.")]
	[SerializeField] [ReadOnly] float time;
	[Tooltip("The entity that applied the status effect. (Not the entity that the status effect is applied to.)")]
	[SerializeField] [ReadOnly] protected Entity caster;

	public string StatusName => statusName;
	public int Duration => duration;
	public float Time
	{
		get => time;
		set => time = value;
	}
	public Entity Caster
	{
		get => caster;
		set => caster = value;
	}

	protected new string name => base.name = string.IsNullOrEmpty
			(Path.GetFileName(AssetDatabase.GetAssetPath(this)).Replace(".asset", string.Empty))
			? statusName
			: Path.GetFileName(AssetDatabase.GetAssetPath(this)).Replace(".asset", string.Empty);

	public override string ToString() => $"{statusName} ({duration} seconds)";

	protected Player player => FindAnyObjectByType<Player>();

	#region Base
	public virtual void Invoke(Entity entityTarget)
	{
		// set the target to the player if the target is self
		if (target == Target.Self) entityTarget = player;

		VisualEffect(entityTarget);
		entityTarget.ApplyStatusEffects(this);

		Debug.Log($"Applied {this} to {entityTarget}!");
	}

	protected Coroutine decayCoroutine;

	protected virtual void Awake() => OnInstantiated += effect =>
	{
		effect.Time = effect.Duration;
		effect.decayCoroutine ??= CoroutineHelper.StartCoroutine(effect.Decay());
	};

	protected virtual IEnumerator Decay()
	{
		while (Time > 0)
		{
			Time -= UnityEngine.Time.deltaTime;
			yield return null;
		}

		Destroy(this);
		decayCoroutine = null;
	}

	protected virtual void Reset()
	{
		statusName = "$NAME";
		description = "$DESCRIPTION";
		duration = 24;
		target = Target.Self;
		timing = Timing.Postfix;
	}
	#endregion

	#region Effects
	public abstract class Buff : StatusEffect // Mostly a marker class
	{
	}

	public abstract class Debuff : StatusEffect // Mostly a marker class
	{
	}
	#endregion

	#region Utility
	static GameObject GetPooledObject(GameObject prefab) => ObjectPoolManager.FindObjectPool(prefab, 5).GetPooledObject(true);

	protected static void VisualEffect(Entity target, bool isDoT = false)
	{
		var prefab = Resources.Load<GameObject>("PREFABS/Effect");
		GameObject pooled = GetPooledObject(prefab);
		pooled.transform.position = target.transform.position;
		pooled.transform.localScale = isDoT ? new (0.5f, 0.5f) : new (1, 1);
		var sprite = pooled.GetComponent<SpriteRenderer>();

		sprite.DOFade(0, 1).OnComplete
		(() =>
		{
			sprite.DOFade(1, 0);
			pooled.SetActive(false);
		});
	}

	/// <summary>
	///     Callback for when a status effect is instantiated.
	///     The parameters are the status effect that was instantiated and the entity that the status effect was applied to.
	/// </summary>
	protected static event Action<StatusEffect> OnInstantiated;

	public static StatusEffect Instantiate(StatusEffect original)
	{
		StatusEffect effect = Object.Instantiate(original);

		OnInstantiated?.Invoke(effect);
		return effect;
	}
	#endregion
}

[CustomEditor(typeof(StatusEffect), true)]
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

			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		serializedObject.ApplyModifiedProperties();
	}
}

public static class StatusEffectExtensions
{
	/// <summary>
	/// </summary>
	/// <param name="original"> The status effect from the list of effects to load an instance of. </param>
	/// <returns> A runtime <see cref="ScriptableObject" /> instance of the Status Effect. </returns>
	static StatusEffect Load(StatusEffect original)
	{
		try
		{
			string path = $"Scriptables/Status Effects/{original.name}";
			original = Resources.Load<StatusEffect>(path);
		} catch (InvalidPathException pathException)
		{
			Logger.LogException(pathException);
			throw;
		}

		StatusEffect effect = StatusEffect.Instantiate(original);

		return effect;
	}

	/// <summary>
	/// </summary>
	/// <param name="effects"></param>
	/// <returns>
	///     Returns a tuple containing two lists of status effects. The first list contains status effects that apply
	///     early (before damage is applied), and the second list contains status effects that apply late (after damage is
	///     applied).
	/// </returns>
	public static (List<StatusEffect> appliesEarly, List<StatusEffect> appliesLate) Load(this List<StatusEffect> effects)
	{
		List<StatusEffect> prefix = new ();
		List<StatusEffect> postfix = new ();

		foreach (StatusEffect effect in effects)
		{
			switch (effect.timing)
			{
				case StatusEffect.Timing.Prefix: {
					StatusEffect instantiatedEffect = Load(effect);
					prefix.Add(instantiatedEffect);
					break;
				}

				case StatusEffect.Timing.Postfix: {
					StatusEffect instantiatedEffect = Load(effect);
					postfix.Add(instantiatedEffect);
					break;
				}
			}
		}

		return (prefix, postfix);
	}

	public static void Apply(this List<StatusEffect> effects, (Entity target, Entity caster) target)
	{
		foreach (StatusEffect effect in effects)
		{
			effect.Caster = target.caster;
			effect.Invoke(target.target);
		}
	}
}
