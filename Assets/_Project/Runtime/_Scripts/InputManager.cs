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

	public void OnDash(InputAction.CallbackContext context) => Logger.LogWarning("Dash is not implemented.");

	public void Ability(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		if (abilityButtons.TryGetValue(context.action.name, out GameObject button)) button.GetComponent<AbilityButton>().Invoke();
	}

	#region Utility
	public static List<string> AbilityKeys => (from action in FindAnyObjectByType<PlayerInput>().actions where action.name.StartsWith("Ability") select action.name).ToList();

	int abilityIndex(AbilityButton button) => button.transform.GetSiblingIndex();

	[Button] [UsedImplicitly]
	public void SetDictionaryKeys()
	{
		for (int index = 0; index < AbilityKeys.Count; index++) abilityButtons[AbilityKeys[index]] = FindMultiple<AbilityButton>().FirstOrDefault(b => abilityIndex(b) == index)?.gameObject;
	}
	#endregion
}
