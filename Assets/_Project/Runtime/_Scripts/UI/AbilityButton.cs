#region
using System.Collections;
using System.Globalization;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using Lumina.Essentials.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VInspector;
using Tween = DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>;
using ColorTween = DG.Tweening.Core.TweenerCore<UnityEngine.Color, UnityEngine.Color, DG.Tweening.Plugins.Options.ColorOptions>;
#endregion

public class AbilityButton : MonoBehaviour, IPausable
{
	[Tab("Ability")]
	[SerializeField] Ability ability;
	[SerializeField] [ReadOnly] Player owner;

	[Tab("UI")]
	[Header("References")]
	[SerializeField] Image circle;
	[Tooltip("The layer that covers the button when it's on cooldown. \nActive = on cooldown.")]
	[SerializeField] Image availabilityLayer;
	[SerializeField] TextMeshProUGUI duration;

	[Header("Settings")]
	[UsedImplicitly] [Tooltip("Whether the ability is currently on cooldown.")]
	[SerializeField] [ReadOnly] bool onCooldown;
	[UsedImplicitly] [Tooltip("The amount of time remaining on the cooldown. \nThis variable is only used for debugging.")]
	[SerializeField] [ReadOnly] float cooldownTime;

	//[Tooltip("The amount of time remaining on the cooldown that the ability can be pressed again to queue it.")]
	//[SerializeField] float coyoteTime = 0.2f;
	//[SerializeField, ReadOnly] bool queued;

	[Header("Animation")]
	[Tooltip("The duration of all fades applied to the cooldown animation. (e.g. fill amount, text)")]
	[SerializeField] float fadeDuration = 0.35f;

	[Tab("Settings")]
	[SerializeField] bool showCooldown;
	[EndTab]
	
	bool OnCooldown
	{
		get => cooldownTween.IsActive() && cooldownTween.IsPlaying();

		set
		{
			onCooldown = value;
			ToggleAvailability();
		}
	}

	// ReSharper disable once ParameterHidesMember
	float CooldownTime()
	{
		if (ability == null) return 0;

		float time = ability.Cooldown - cooldownTween.Elapsed();

		time = Mathf.RoundToInt(time);
		cooldownTime = time;

		return time;
	}

	public int abilityIndex => transform.GetSiblingIndex();

	Button button => GetComponent<Button>();

	void TriggerGCD(Player performer)
	{
		if (performer != owner) return; // Only trigger the GCD for the player that owns this ability button.

		if (ability.Cooldown > 5) return;
		if (ability.CDType is Ability.CooldownType.Instant) return; // Instant abilities are not affected by the global cooldown.

		if (OnCooldown) return;
		
		Cooldown();
	}

	IEnumerator Start()
	{
		if (!ability) Logger.LogError("Ability is null. \nPlease assign an ability to this button.");
		
		PlayerManager.Instance.OnPlayerJoined += player =>
		{
			// if the player and the ability button have the same tag, then we can set the owner
			if (gameObject.CompareTag(player.tag)) owner = player;
		};

		GameManager.Instance.OnEnterBattle += () =>
		{
			ability.ResetProperties();

			if (ability.StartsOnCooldown) Cooldown();
			else ResetCooldown();

			if (ability.Cooldown >= 100) Cooldown(true);
		};

		GameManager.Instance.OnVictory += () =>
		{
			ability.ResetProperties();

			ResetCooldown();
		};

		yield return new WaitUntil(() => owner != null);
		
		Ability.OnGlobalCooldown += TriggerGCD;

		Job job = owner.Job;
		ability = job.Abilities[abilityIndex];

		button.onClick.AddListener(Invoke);
		button.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = ability.Icon;

		circle.fillAmount = 0;
		duration.alpha = 0;

		switch (showCooldown, ability.Cooldown)
		{
			case (false, >= 10):
			case (true, _):
				duration.gameObject.SetActive(true);
				break;

			default:
				duration.gameObject.SetActive(false);
				break;

		}

		InitAvailabilityLayer();

		// #if UNITY_EDITOR
		//
		// 		// This is probably due to using InvariantCulture in the ToString method rather than F0 or F2 as used previously.
		// 		if (showDecimals) Logger.LogWarning("For whatever reason, using showDecimals breaks the cooldown timer on the UI. \nUse at your own risk.");
		//
		// 		EditorApplication.update += () => cooldownTime = CooldownTime(true);
		// #endif
	}

	void OnDestroy() => Ability.OnGlobalCooldown -= TriggerGCD;

	void Update()
	{
		if (owner)
		{
			if (AbilityIsPrimed(ability))
			{
				GetComponentInChildren<Outline>().effectColor = Color.yellow;
			}
			else if (!AbilityIsPrimed(ability))
			{
				GetComponentInChildren<Outline>().effectColor = Color.black;
			}
		}
		
		// in simple terms, this checks the following:
		// - if the ability is on cooldown
		// - if the ability has been cancelled
		// - if the cooldown has not yet exceeded the cast time, meaning it hasn't been cast yet and its cooldown can be reset.
		if (OnCooldown && ability.casting && ability.cancelled && cooldownTween.Elapsed() <= ability.CastTime)
		{
			if (!ability.cancellable) return;

			Logger.LogWarning($"{ability} has been cancelled.");
			ResetCooldown();
		}
	}

	void InitAvailabilityLayer()
	{
		availabilityLayer.name = "Available";
		availabilityLayer.gameObject.SetActive(true);
		availabilityLayer.DOFade(0, 0);
	}

	ColorTween fadeTween;

	void ToggleAvailability()
	{
		if (fadeTween != null && fadeTween.IsActive()) fadeTween.Kill();

		const float fadeTo = 0.8f;
		const float fadeFrom = 0;

		fadeTween = OnCooldown ? availabilityLayer.DOFade(fadeTo, fadeDuration).OnStart(() => availabilityLayer.gameObject.SetActive(true)) : availabilityLayer.DOFade(fadeFrom, fadeDuration).OnComplete(() => availabilityLayer.gameObject.SetActive(false));

		StartCoroutine(UpdateCooldownText());
	}

	IEnumerator UpdateCooldownText()
	{
		// set the cooldown text to the cooldown duration while the cooldown is counting down
		while (OnCooldown)
		{
			// Always show decimals in the hierarchy.
			// Mostly a debugging feature.
			availabilityLayer.gameObject.name = $"Unavailable ({CooldownTime()} seconds)";
			duration.text = CooldownTime().ToString(CultureInfo.InvariantCulture);
			yield return null;
		}

		availabilityLayer.gameObject.name = "Available";
	}

	Tween cooldownTween;
	ColorTween cooldownFadeTween;

	void Cooldown(bool hideUI = false)
	{
		ResetCooldown();

		// Only play the cooldown animation if it's not already playing
		if (TweenIsInvalid(cooldownTween) || TweenIsInvalid(cooldownFadeTween)) return;

		InitCooldown(); // Sets the fill amount to 1 and fades the duration text in

		cooldownTween = circle.DOFillAmount(0, ability.Cooldown).SetEase(Ease.InOutSine);
		OnCooldown = true;

		cooldownTween.OnComplete
		(() =>
		{
			cooldownTween = null;
			cooldownFadeTween = null;

			OnCooldown = false;

			Terminate(); // Fades the duration text out
		});

		if (hideUI)
		{
			circle.gameObject.SetActive(false);
			duration.gameObject.SetActive(false);

			cooldownTween.OnComplete
			(() =>
			{
				circle.gameObject.SetActive(true);
				duration.gameObject.SetActive(true);
			});
		}

		return;
		void InitCooldown()
		{
			const float fadeTo = 1;

			circle.fillAmount = 1;
			cooldownFadeTween = duration.DOFade(fadeTo, fadeDuration);
		}

		void Terminate() => duration.DOFade(0, 0.35f);
	}

	public void ResetCooldown()
	{
		// Reset everything and apply the fade out animation to the duration text

		cooldownTween.Kill();
		cooldownFadeTween.Kill();

		circle.fillAmount = 0;
		duration.DOFade(0, fadeDuration);
		OnCooldown = false;
	}

	public void Invoke()
	{
		if (!enabled) return; // if paused
		
		if (OnCooldown) return;

		if (!ability.Invoke(owner, out Ability.InvokeOutcome reason))
		{
			switch (reason)
			{
				case Ability.InvokeOutcome.NoTarget:
					Logger.LogWarning($"{ability} was not invoked. \nReason: No target found.");
					break;
				
				default:
					Logger.LogWarning($"{ability} was not invoked. \nReason: Unknown.");
					break;
			}
			return;
		}
		
		Cooldown();

		if (ability.TryRefund(owner, out Ability refundee))
		{
			AbilityButton abilityButton = FindObjectsByType<AbilityButton>(FindObjectsSortMode.None)
			    .FirstOrDefault(b => b.ability == refundee);
			
			abilityButton?.ResetCooldown();
		}
	}
	
	bool AbilityIsPrimed(Ability ability) => ability.IsPrimed(owner) > 1;

	bool TweenIsInvalid(Tween tween) => tween != null && tween.IsActive() && tween.IsPlaying();

	bool TweenIsInvalid(ColorTween tween) => tween != null && tween.IsActive() && tween.IsPlaying();

	#region Utility
	static float RoundToDecimal(float value, int decimals) => Mathf.Round(value * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals);
	#endregion

	public void Pause() => enabled = false;

	public void Resume() => enabled = true;
}