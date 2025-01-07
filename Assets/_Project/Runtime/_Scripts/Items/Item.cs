using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TMPro;
using UnityEngine;
using VInspector;

public abstract class Item : MonoBehaviour
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Rarity
    {
        [InspectorName("Grey")] Common,    // Grey
        [InspectorName("Blue")] Rare,      // Blue
        [InspectorName("Purple")] Epic,    // Purple
        [InspectorName("Gold")] Legendary, // Gold
        [InspectorName("Red")] Mythic,     // Red
    }
    
    [Serializable]
    public enum Attributes
    {
        Damage,
        Duration,
        Cooldown,
        Buff,
    }

    #region
    public (Rarity rarity, Color color) RarityColor => 
        rarity switch
        { Rarity.Common    => (Rarity.Common, Color.grey),
          Rarity.Rare      => (Rarity.Rare, Color.blue),
          Rarity.Epic      => (Rarity.Epic, Color.magenta),
          Rarity.Legendary => (Rarity.Legendary, Color.yellow),
          Rarity.Mythic    => (Rarity.Mythic, Color.red),
          _                => (Rarity.Common, Color.grey) };
    #endregion
    
    [Header("Item Info")]
    [SerializeField] new string name;
    [TextArea(3, 5)]
    [SerializeField] string description;
    [Space(10)]

    [Header("Attributes")]
    [SerializeField] int damage;
    [SerializeField] float duration;
    [SerializeField] float cooldown;
    [SerializeField] StatusEffect buff;
    
    [Space(10)]
    [SerializeField] Rarity rarity;
    [Space(10)]
    
    [Header("UI")]
    [SerializeField] TextMeshProUGUI descriptionText;

    public virtual void Action() { }
    
    public void OnValidate()
    {
        if (!string.IsNullOrEmpty(name)) gameObject.name = name;
        else name = gameObject.name;
        
        FormatDescription();
    }

    void FormatDescription()
    {
        string[] keywords = { "[damage]", "[duration]", "[cooldown]", "[buff]" };
        string format = description;

        for (int i = 0; i < keywords.Length; i++)
        {
            if (format.Contains(keywords[i])) format = format.Replace(keywords[i], $"{{{i}}}");
        }

        descriptionText.text = string.Format(format, damage, duration, cooldown, buff.StatusName);
    }
}
