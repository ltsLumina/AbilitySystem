using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Essentials.Modules;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

[ExecuteInEditMode]
public class AbilityValidator : MonoBehaviour
{
    [SerializeField] List<InputAction> abilities = new();
    
    InputAction ability;

    void ValidateName()
    {
        var actions = FindAnyObjectByType<PlayerInput>().actions;

        foreach (var action in actions)
        {
            if (abilities.Contains(action)) continue;

            abilities = actions.Where(a => a.name.Contains("Ability")).ToList();
            
            int childIndex = transform.GetSiblingIndex();

            ability = abilities[childIndex];
            const string template = "Ability [KEY]";
            gameObject.name = template.Replace("[KEY]", $"[\" {ability.GetBindingDisplayString()} \"]");
            
            GameObject icon = transform.GetLastChild().gameObject;
            // temporarily changing the text instead of an icon
            icon.GetComponent<TMPro.TextMeshProUGUI>().text = ability.GetBindingDisplayString();
            // this will be the proper way eventually:
            // icon.GetComponent<Image>().sprite = iconSprite;
        }
    }

    [Button]
    public void FixName()
    {
        const string template = "Ability [KEY]";
        gameObject.name = template.Replace("[KEY]", "N/A");
    }

    void Start() => ValidateName();

    void OnValidate() => ValidateName();
}
