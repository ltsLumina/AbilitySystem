using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
using static Lumina.Essentials.Modules.Helpers;

public class InputManager : MonoBehaviour
{
    [SerializeField] SerializedDictionary<string, GameObject> abilityButtons;

    public Vector2 MoveInput { get; private set; }

    public bool IsMoving { get; set; }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        IsMoving = MoveInput != Vector2.zero;
    }

    public void OnDash(InputAction.CallbackContext context) { Logger.LogWarning("Dash is not implemented."); }

    void Ability(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (abilityButtons.TryGetValue(context.action.name, out GameObject button)) { button.GetComponent<AbilityButton>().Invoke(); }
    }

    public void OnAbility1(InputAction.CallbackContext context) { Ability(context); }

    public void OnAbility2(InputAction.CallbackContext context) { Ability(context); }

    public void OnAbility3(InputAction.CallbackContext context) { Ability(context); }

    public void OnAbility4(InputAction.CallbackContext context) { Ability(context); }

    [Button, UsedImplicitly]
    public void SetDictionaryKeys()
    {
        InputActionAsset actions = GetComponentInParent<PlayerInput>().actions;
        List<string> abilityKeys = new ();

        foreach (var action in actions)
        {
            // add all actions that start with "Ability_" to the list
            if (action.name.StartsWith("Ability")) { abilityKeys.Add(action.name); }
        }

        abilityButtons = new ()
        { { abilityKeys[0], FindMultiple<AbilityButton>().FirstOrDefault(b => b.ability == 1)?.gameObject },
          { abilityKeys[1], FindMultiple<AbilityButton>().FirstOrDefault(b => b.ability == 2)?.gameObject },
          { abilityKeys[2], FindMultiple<AbilityButton>().FirstOrDefault(b => b.ability == 3)?.gameObject },
          { abilityKeys[3], FindMultiple<AbilityButton>().FirstOrDefault(b => b.ability == 4)?.gameObject } };
    }
}
