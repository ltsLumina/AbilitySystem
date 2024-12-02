using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public abstract class ItemAttributes : ScriptableObject
{
    [Header("Active Item Properties")]
    [SerializeField] Buff buff;
    [SerializeField] int damage;
    [SerializeField] float duration; // The duration of the potential effect the item has.
    [SerializeField] float cooldown;

    public TMP_Text damageText { get; set; }
    TMP_Text damageValueText { get; set; }
    public TMP_Text durationText { get; set; }
    TMP_Text durationValueText { get; set; }
    public TMP_Text cooldownText { get; set; }
    TMP_Text cooldownValueText { get; set; }

    public Buff Buff
    {
        get => buff;
        set => buff = value;
    }
    public int Damage
    {
        get => damage;
        set => damage = value;
    }
    
    public float Duration
    {
        get => duration;
        set => duration = value;
    }
    
    public float Cooldown
    {
        get => cooldown;
        set => cooldown = value;
    }
    public Item Item { get; set; }

    void OnValidate()
    {
        damageValueText   = damageText.transform.GetChild(0).GetComponent<TMP_Text>();
        durationValueText = durationText.transform.GetChild(0).GetComponent<TMP_Text>();
        cooldownValueText = cooldownText.transform.GetChild(0).GetComponent<TMP_Text>();
        
        damageValueText.text = Damage.ToString();
        durationValueText.text = $"({Duration}s)";
        cooldownValueText.text = $"{Cooldown}s";

        Item.RefreshText();
    }

    public void Invoke()
    {
        Item.StartCoroutine(Wait());
        
        return;
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(Cooldown);
            OnInvoke();
        }
    }
    
    protected virtual void OnInvoke() { }
}