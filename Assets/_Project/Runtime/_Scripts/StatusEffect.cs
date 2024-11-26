#region
using System;
#endregion

[Serializable]
public struct StatusEffect
{
    public string Name;
    public float Duration;
    public string Description;

    public StatusEffect(string name, float duration, string description = "")
    {
        Name = name;
        Duration = duration;
        Description = description;
    }
}
