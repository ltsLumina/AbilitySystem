#region
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
#endregion

[ExecuteInEditMode]
public class AbilityValidator : MonoBehaviour
{
    void ValidateName()
    {
        InputActionAsset actions = FindAnyObjectByType<PlayerInput>().actions;
        List<InputAction> abilities = actions.Where(a => InputManager.AbilityKeys.Contains(a.name)).ToList();

        int childIndex = transform.GetSiblingIndex();
        if (childIndex >= abilities.Count) return;

        const string template = "Ability [KEY]";
        gameObject.name = template.Replace("[KEY]", $"[\" {abilities[childIndex].GetBindingDisplayString()} \"]");
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
