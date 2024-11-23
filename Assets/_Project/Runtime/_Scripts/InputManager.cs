#region
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using VInspector;
using static Lumina.Essentials.Modules.Helpers;
#endregion

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

    #region Utility
    public static List<string> AbilityKeys => (from action in FindAnyObjectByType<PlayerInput>().actions where action.name.StartsWith("Ability") select action.name).ToList();

    [Button, UsedImplicitly]
    public void SetDictionaryKeys() => abilityButtons = new ()
    { { AbilityKeys[0], FindMultiple<AbilityButton>().FirstOrDefault(b => b.abilityIndex == 0)?.gameObject },
      { AbilityKeys[1], FindMultiple<AbilityButton>().FirstOrDefault(b => b.abilityIndex == 1)?.gameObject },
      { AbilityKeys[2], FindMultiple<AbilityButton>().FirstOrDefault(b => b.abilityIndex == 2)?.gameObject },
      { AbilityKeys[3], FindMultiple<AbilityButton>().FirstOrDefault(b => b.abilityIndex == 3)?.gameObject } };
    #endregion
}
