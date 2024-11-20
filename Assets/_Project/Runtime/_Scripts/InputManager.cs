using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Lumina.Essentials.Modules.Helpers;

public class InputManager : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    public bool IsMoving { get; set; }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        IsMoving = MoveInput != Vector2.zero;
    }

    void Ability(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"{context.action.name} activated.");
            var effect = Instantiate(Resources.Load<GameObject>("PREFABS/Effect"));
            effect.transform.position = transform.position + transform.right * 2.5f;
        }
    }
    
    public void OnAbility1(InputAction.CallbackContext context)
    {
        Ability(context);
    }
    
    public void OnAbility2(InputAction.CallbackContext context)
    {
        Ability(context);
    }
    
    public void OnAbility3(InputAction.CallbackContext context)
    {
        Ability(context);
    }
    
    public void OnAbility4(InputAction.CallbackContext context)
    {
        Ability(context);
    }
}
