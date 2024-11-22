#region
using System.Collections;
using System.Globalization;
using DG.Tweening;
using JetBrains.Annotations;
using Lumina.Essentials.Attributes;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VInspector;
using Tween = DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>;
using ColorTween = DG.Tweening.Core.TweenerCore<UnityEngine.Color, UnityEngine.Color, DG.Tweening.Plugins.Options.ColorOptions>;
#endregion

public class AbilityButton : MonoBehaviour
{
    [Tab("Ability"), EnableIf(nameof(ability), null), SerializeField]
     Ability ability;
    [EndIf]
    [Tab("UI")]
    [Header("References")]
    [SerializeField] Image circle;
    [Tooltip("The layer that covers the button when it's on cooldown. \nActive = on cooldown.")]
    [SerializeField] Image availabilityLayer;
    [SerializeField] TextMeshProUGUI duration;

    [Header("Settings")]
    [SerializeField] float cooldown = 2.5f;
    [UsedImplicitly, Tooltip("Whether the ability is currently on cooldown.")]
    [SerializeField, ReadOnly] bool onCooldown;
    [UsedImplicitly, Tooltip("The amount of time remaining on the cooldown. \nThis variable is only used for debugging.")]
    [SerializeField, ReadOnly] float cooldownTime;
    [Tooltip("The amount of time remaining on the cooldown that the ability can be pressed again to queue it.")]
    [SerializeField] float coyoteTime = 0.2f;
    [SerializeField, ReadOnly] bool queued;

    [Header("Animation"), Tooltip("The duration of all fades applied to the cooldown animation. (e.g. fill amount, text)")]
    [SerializeField] float fadeDuration = 0.35f;

    [Tab("Settings")]
    [SerializeField] bool showDecimals;
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

    float CooldownTime(bool showDecimals = false)
    {
        if (showDecimals) return RoundToDecimal(circle.fillAmount * cooldown, 2);
        return Mathf.RoundToInt(circle.fillAmount * cooldown);
    }

    //float cooldownTime => RoundToDecimal(cooldownTween.Elapsed(), 2);

    public int abilityIndex => transform.GetSiblingIndex() + 1;

    Button button => GetComponent<Button>();

    protected void Start()
    {
        button.onClick.AddListener(Invoke);
        button.image.sprite = ability.abilityIcon;

        circle.fillAmount = 0;
        duration.alpha = 0;

        InitAvailabilityLayer();

#if UNITY_EDITOR

        // This is probably due to using InvariantCulture in the ToString method rather than F0 or F2 as used previously.
        if (showDecimals) Logger.LogWarning("For whatever reason, using showDecimals breaks the cooldown timer on the UI. \nUse at your own risk.");

        EditorApplication.update += () => cooldownTime = CooldownTime(true);
#endif
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

        fadeTween = OnCooldown
            ? availabilityLayer.DOFade(fadeTo, fadeDuration).OnStart(() => availabilityLayer.gameObject.SetActive(true))
            : availabilityLayer.DOFade(fadeFrom, fadeDuration).OnComplete(() => availabilityLayer.gameObject.SetActive(false));

        StartCoroutine(UpdateCooldownText());
    }

    IEnumerator UpdateCooldownText()
    {
        // set the cooldown text to the cooldown duration while the cooldown is counting down
        while (OnCooldown)
        {
            // Always show decimals in the hierarchy.
            // Mostly a debugging feature.
            availabilityLayer.gameObject.name = $"Unavailable ({CooldownTime(showDecimals: true)} seconds)";
            duration.text = CooldownTime(showDecimals).ToString(CultureInfo.InvariantCulture);
            yield return null;
        }

        availabilityLayer.gameObject.name = "Available";
    }

    Tween cooldownTween;
    ColorTween cooldownFadeTween;

    void Cooldown()
    {
        // Only play the cooldown animation if it's not already playing
        if (TweenIsInvalid(cooldownTween) || TweenIsInvalid(cooldownFadeTween)) return;

        InitCooldown(); // Sets the fill amount to 1 and fades the duration text in

        cooldownTween = circle.DOFillAmount(0, cooldown).SetEase(Ease.InOutSine);
        OnCooldown = true;

        cooldownTween.OnComplete
        (() =>
        {
            cooldownTween = null;
            cooldownFadeTween = null;

            OnCooldown = false;

            Terminate(); // Fades the duration text out

            if (queued)
            {
                queued = false;
                Cooldown();
            }
        });

        return;

        void InitCooldown()
        {
            const float fadeTo = 1;
            const float fadeDuration = 0.35f;

            circle.fillAmount = 1;
            cooldownFadeTween = duration.DOFade(fadeTo, fadeDuration);
        }

        void Terminate()
        {
            if (queued) return;

            duration.DOFade(0, 0.35f);
        }
    }

    public void Invoke()
    {
        if (OnCooldown && CooldownTime() < coyoteTime)
        {
            queued = true;
            return;
        }

        // <abilityLogic()>
        ability?.Invoke();
        Cooldown();
        Logger.Log($"Ability {abilityIndex} has been invoked.");
    }

    bool TweenIsInvalid(Tween tween) => tween != null && tween.IsActive() && tween.IsPlaying();

    bool TweenIsInvalid(ColorTween tween) => tween != null && tween.IsActive() && tween.IsPlaying();

    #region Utility
    static float RoundToDecimal(float value, int decimals) => Mathf.Round(value * Mathf.Pow(10, decimals)) / Mathf.Pow(10, decimals);
    #endregion
}
