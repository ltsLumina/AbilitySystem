using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Lumina.Essentials.Modules.Helpers;

public class AbilityButton : Button
{
    [SerializeField] Image cooldown;
    
    public int ability => transform.GetSiblingIndex() + 1;
    
    protected override void Start()
    {
        base.Start();
        targetGraphic = transform.GetChild(0).GetComponent<Image>();
    }

    public void Cooldown()
    {
        cooldown.fillAmount -= 0.1f;
    }
}
