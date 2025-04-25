#region
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#endregion

public static class Logger
{
    #region Prefix
    // Change the prefix color here.
    // The color must be a key in the colorDictionary.
    // Enter the color name from the colorDictionary to change the color.
    // For example, to change the prefix color to red, enter "Red", or "Orange" for orange.
    const string prefixColor = "Orange";

    static string ErrorMessagePrefix(string color, string prefix = "Logger")
    {
        if (string.IsNullOrEmpty(prefix)) prefix = "Logger";
        return $"{color}[{prefix}] ►</color>";
    }

    const string DefaultErrorMessage = "No message was provided. \nWas this intentional?";

    readonly static Dictionary<string, string> colorDictionary = new ()
    { { "Red", "<color=red>" },
      { "Orange", "<color=orange>" },
      { "Yellow", "<color=yellow>" },
      { "Green", "<color=green>" },
      { "Blue", "<color=blue>" },
      { "Indigo", "<color=indigo>" },
      { "Violet", "<color=violet>" },
      { "Cyan", "<color=cyan>" },
      { "Magenta", "<color=magenta>" },
      { "Black", "<color=black>" },
      { "White", "<color=white>" },
      { "Gray", "<color=gray>" },
      { "Brown", "<color=brown>" },
      { "None", "<color=white>" } };
    #endregion

    #region Log Behaviour
    public static LogLevel LogBehaviour { get; set; } = LogLevel.Verbose;

    public enum LogLevel
    {
        [UsedImplicitly]
        Quiet,
        [UsedImplicitly]
        Verbose,
    }
    #endregion

    #region Logging Methods
    /// <summary>
    ///     Logs a message to the console.
    ///     <remarks>
    ///         If the message is NULL, the default error message is logged.
    ///         If the message is string.Empty (or simply: ""), an empty string is logged without the typical prefix.
    ///     </remarks>
    /// </summary>
    /// <param name="message"> The message to be logged. </param>
    /// <param name="context"> Object to which the message applies. </param>
    /// <param name="prefix"> An optional prefix for the message. By default uses the [Logger] prefix. </param>
    public static void Log(string message = default, Object context = default, string prefix = default) => LogMessage(Debug.Log, message, context, prefix);

    public static void Log(bool message = default, Object context = default) => LogMessage(Debug.Log, message.ToString(), context);

    /// <summary>
    ///     A variant of the Log method that logs a warning message to the console.
    ///     <remarks>
    ///         If the message is NULL, the default error message is logged.
    ///         If the message is string.Empty (or simply: ""), an empty string is logged without the typical prefix.
    ///     </remarks>
    /// </summary>
    /// <param name="message"> The message to be logged. </param>
    /// <param name="context"> Object to which the message applies. </param>
    /// <param name="prefix"> An optional prefix for the message. By default uses the [Logger] prefix. </param>
    public static void LogWarning(string message = default, Object context = default, string prefix = default) => LogMessage(Debug.LogWarning, message, context, prefix);

    public static void LogWarning(bool message = default, Object context = default) => LogMessage(Debug.LogWarning, message.ToString(), context);

    /// <summary>
    ///     A variant of the Log method that logs an error message to the console.
    ///     <remarks>
    ///         If the message is NULL, the default error message is logged.
    ///         If the message is string.Empty (or simply: ""), an empty string is logged without the typical prefix.
    ///     </remarks>
    /// </summary>
    /// <param name="message"> The message to be logged. </param>
    /// <param name="context"> Object to which the message applies. </param>
    public static void LogError(string message = default, Object context = default) => LogMessage(Debug.LogError, message, context);

    public static void LogError(bool message = default, Object context = default) => LogMessage(Debug.LogError, message.ToString(), context);

    /// <summary>
    ///     A variant of the Log method that logs an exception message to the console.
    ///     <remarks>
    ///         If the message is NULL, the default error message is logged.
    ///         If the message is string.Empty (or simply: ""), an empty string is logged without the typical prefix.
    ///     </remarks>
    /// </summary>
    /// <param name="exception"> The exception to be logged. </param>
    /// <param name="context"> Object to which the message applies. </param>
    public static void LogException(Exception exception, Object context = default) => LogMessage(Debug.LogError, exception.Message, context);

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    ///     A variant of the Log method that logs a warning message to the console regardless of the log behaviour.
    ///     This is meant to be used for warnings that should always be logged.
    /// </summary>
    /// <param name="message"> The message to be logged. </param>
    /// <param name="context"> Object to which the message applies. </param>
    public static void LogExplicit(string message, Object context = default)
    {
        message ??= DefaultErrorMessage;
        if (message == string.Empty) message = "";

        string formattedMessage = $"{ErrorMessagePrefix(colorDictionary[prefixColor])} {message}" + "\n";
        Debug.LogWarning(formattedMessage, context);
    }

    public static void LogExplicit(bool message, Object context = default) => Debug.LogWarning("<color=red>[Logger] ►</color> " + message, context);

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    ///     A variant of the Log method that logs a error message to the console regardless of the log behaviour.
    ///     This is meant to be used for warnings that should always be logged.
    /// </summary>
    /// <param name="message"> The message to be logged. </param>
    /// <param name="context"> Object to which the message applies. </param>
    /// <param name="pauseEditor"> Whether or not to pause the editor when the error is logged. </param>
    public static void LogExplicitError(string message, Object context = default, bool pauseEditor = false)
    {
        Debug.LogError("<color=red>[Logger] ►</color> " + message, context);
#if UNITY_EDITOR
        if (pauseEditor) EditorApplication.isPaused = true;
#endif
    }

    public static void LogExplicitError(bool message, Object context = default, bool pauseEditor = false)
    {
        Debug.LogError("<color=red>[Logger] ►</color> " + message, context);
#if UNITY_EDITOR
        if (pauseEditor) EditorApplication.isPaused = true;
#endif
    }

    // -- The following methods are used internally by the Logger class. --

    static void LogMessage(Action<string, Object> logAction, string message, Object context = default, string prefix = null)
    {
        if (LogBehaviour != LogLevel.Verbose) return;
        
        // if message is string.Empty, log an empty string
        if (message == string.Empty) message = "";

        string formattedMessage = $"{ErrorMessagePrefix(colorDictionary[prefixColor], prefix)} {message}" + "\n";
        logAction(formattedMessage, context);
    }
    #endregion
}
