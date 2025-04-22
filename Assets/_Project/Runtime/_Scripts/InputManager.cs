#region
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using VInspector;
#endregion

public class InputManager : MonoBehaviour
{
	[SerializeField] SerializedDictionary<string, GameObject> abilityButtons;

	public Vector2 MoveInput { get; private set; }

	public bool IsMoving { get; private set; }
	
	/// <summary>
	///     Overrides the move input with the given input. (Used for Mouse movement)
	/// </summary>
	public void OverrideMoveInput(Vector2 input) => MoveInput = input;

	/// <summary>
	///     Toggles an input layer on or off.
	///     This is used to enable or disable input layers for different actions.
	///     <example>
	///         Layer 1: Player (regular gameplay)
	///         <para>Layer 2: UI</para>
	///     </example>
	/// </summary>
	/// <param name="layerName"></param>
	public void ToggleInputLayer(string layerName)
	{
		switch (layerName)
		{
			case "Player": {
				var playerInput = FindAnyObjectByType<PlayerInput>();
				playerInput.SwitchCurrentActionMap("Player");
				return;
			}

			case "UI": {
				var playerInput = FindAnyObjectByType<PlayerInput>();
				playerInput.SwitchCurrentActionMap("UI");
				return;
			}

			default:
				Debug.LogWarning($"Input layer {layerName} does not exist.");
				break;
		}
	}

	/// <summary>
	///     Overload for <see cref="ToggleInputLayer(string)" /> that takes an index instead of a string.
	/// </summary>
	/// <param name="layerIndex">The index of the layer to toggle.</param>
	/// <remarks> 0 = Player,
	///     <para>1 = UI</para>
	/// </remarks>
	public void ToggleInputLayer(int layerIndex)
	{
		switch (layerIndex)
		{
			case 0:
				ToggleInputLayer("Player");
				break;

			case 1:
				ToggleInputLayer("UI");
				break;
		}
	}

	void Start()
	{
		var playerInput = GetComponent<PlayerInput>();
		playerInput.uiInputModule = FindAnyObjectByType<InputSystemUIInputModule>();
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		MoveInput = context.ReadValue<Vector2>();
		IsMoving = MoveInput != Vector2.zero;
	}

	public void Ability(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		SetDictionaryKeys();
		if (abilityButtons.TryGetValue(context.action.name, out GameObject button)) button.GetComponent<AbilityButton>().Invoke();
	}

	public void OnPause(InputAction.CallbackContext context)
	{
		if (!context.performed) return;

		GameManager.Instance.PauseManager.TogglePause();
	}

	#region Utility
	public static List<string> AbilityKeys => (from action in FindAnyObjectByType<PlayerInput>().actions where action.name.StartsWith("Ability") select action.name).ToList();

	int abilityIndex(AbilityButton button) => button.transform.GetSiblingIndex();

	[Button]
	void SetDictionaryKeys()
	{
		int playerIndex = GetComponent<PlayerInput>().playerIndex;
		GameObject[] objs = GameObject.FindGameObjectsWithTag($"Player {playerIndex + 1}");
		List<AbilityButton> buttons = objs.SelectMany(obj => obj.GetComponentsInChildren<AbilityButton>()).ToList();
		for (int index = 0; index < AbilityKeys.Count; index++) abilityButtons[AbilityKeys[index]] = buttons.FirstOrDefault(b => abilityIndex(b) == index)?.gameObject;
	}
	#endregion
}
