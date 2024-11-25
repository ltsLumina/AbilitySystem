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
    void ValidateButtonName()
    {
        InputActionAsset actions = FindAnyObjectByType<PlayerInput>()?.actions;

        if (actions != null)
        {
            List<InputAction> abilities = actions.Where(a => InputManager.AbilityKeys.Contains(a.name)).ToList();

            int childIndex = transform.GetSiblingIndex();
            if (childIndex >= abilities.Count) return;

            const string template = "Ability [KEY]";
            gameObject.name = template.Replace("[KEY]", $"[\" {abilities[childIndex].GetBindingDisplayString()} \"]");
        }
    }

    [Button, UsedImplicitly]
    public void FixName()
    {
        const string template = "Ability [KEY]";
        gameObject.name = template.Replace("[KEY]", "N/A");
    }

    void Start()
    {
        ValidateButtonName();
        
        ValidateAbilityAssetFields();
    }

    void OnValidate() => ValidateButtonName();

    void ValidateAbilityAssetFields()
    {
        var job = FindAnyObjectByType<Player>().Job;
        
        Debug.Assert(job.Abilities.Count != 0, $"No abilities found for {job}.", job);
        Debug.Assert(job.Abilities.Count == 4, $"Not enough abilities for {job}.", job);

        // TODO: Uncomment these lines after more work has been done on the Ability class
        var randomNames = NameGenerator.GenerateRandomNames(job.Abilities.Count);
        foreach (Ability a in job.Abilities)
        {
            if (string.IsNullOrEmpty(a.Name))
            {
                int randIndex = Random.Range(0, randomNames.Count);
                a.Name = randomNames[randIndex];
                randomNames.RemoveAt(randIndex);
            }
            //Debug.Assert(!string.IsNullOrEmpty(a.Description), $"Ability description is null for {a}.", a);
            Debug.Assert(a.Icon        != null, $"Ability icon is null for {a}.", a);
            Debug.Assert(a.Cooldown    > 0, $"Ability cooldown is less than 0 for {a}.", a);
            //Debug.Assert(a.Range       > 0, $"Ability range is less than 0 for {a}.", a);
            //Debug.Assert(a.Radius      > 0, $"Ability radius is less than 0 for {a}.", a);
            Debug.Assert(a.Damage      > 0, $"Ability damage is less than 0 for {a}.", a);
            if (a.DmgType == Ability.DamageType.DoT) 
                Debug.Assert(a.DamageTicks > 0, $"Ability damage ticks is less than 0 for {a}.", a);
        }
    }
}

public static class NameGenerator
{
    private static readonly List<string> Elements = new List<string>
    { "Fire", "Water", "Earth", "Air", "Lightning", "Ice" };
    private static readonly List<string> Nouns = new List<string>
    { "Slash", "Punch", "Strike", "Blast", "Wave", "Bolt" };

    public static List<string> GenerateRandomNames(int count)
    {
        List<string> names = new List<string>();

        for (int i = 0; i < count; i++)
        {
            string element = Elements[Random.Range(0, Elements.Count)];
            string noun = Nouns[Random.Range(0, Nouns.Count)];

            names.Add($"{element} {noun}");
        }

        return names;
    }
}
