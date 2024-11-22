using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;

[ExecuteInEditMode]
public class AbilityValidator : MonoBehaviour
{
    [SerializeField] List<InputAction> abilities = new ();

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

            // this will be the proper way eventually:
            // icon.GetComponent<Image>().sprite = iconSprite;
        }
    }

    [Button, UsedImplicitly]
    public void FixName()
    {
        const string template = "Ability [KEY]";
        gameObject.name = template.Replace("[KEY]", "N/A");
    }

    void Start() => ValidateName();

    void OnValidate() => ValidateName();
}
