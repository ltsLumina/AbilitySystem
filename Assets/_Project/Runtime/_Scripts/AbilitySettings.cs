/// <summary>
///     Contains the default settings for abilities.
/// </summary>
public struct AbilitySettings
{
    /// <summary>
    ///     The default GCD cooldown.
    /// </summary>
    public static float GlobalCooldown => 1.5f;

    /// <summary>
    ///     <para> A DoT will deal damage every X tick cycles. </para>
    /// </summary>
    public static int DoT_Rate => 3;

    public struct ResourcePaths
    {
        public const string SCRIPTABLES = "Scriptables";
        public const string ABILITIES = "Scriptables/Abilities";
        public const string JOB = "Scriptables/Jobs";
    }
}
