/// <summary>
///     Contains the default settings for abilities.
/// </summary>
public static class AbilitySettings
{
    /// <summary>
    ///     The default GCD cooldown.
    /// </summary>
    public static float GlobalCooldown => 2.5f;

    /// <summary>
    ///     <para> A DoT will deal damage every X tick cycles. </para>
    /// </summary>
    public static int DoT_Cycles => 3;

    public static string ResourcesPath => "Scriptables/Abilities";
}
