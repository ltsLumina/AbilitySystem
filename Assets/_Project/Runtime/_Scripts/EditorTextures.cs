#region
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#endregion

public static class EditorTextures
{
    public static Dictionary<string, Texture2D> Textures => new ()
    { { "Refresh", Refresh },
      { "Warning", Warning },
      { "Error", Error },
      { "Success", Info },
      { "Help", Help },
      { "Play", Play },
      { "Pause", Pause },
      { "Stop", Stop },
      { "Forward", Forward },
      { "Backward", Back },
      { "Favorite", Favorite },
      { "FavoriteActive", FavoriteIcon },
      { "RGB", RGB },
      { "Settings", Settings } };

    public static Texture2D GetTexture(string name) => EditorGUIUtility.FindTexture(name);

    public static Texture2D Refresh => GetTexture("d_Refresh");
    public static Texture2D Info => GetTexture("d_console.infoicon");
    public static Texture2D Warning => GetTexture("d_console.warnicon");
    public static Texture2D Error => GetTexture("d_console.erroricon");
    public static Texture2D Help => GetTexture("d_UnityEditor.ConsoleWindow");
    public static Texture2D Play => GetTexture("d_PlayButton");
    public static Texture2D Pause => GetTexture("d_PauseButton");
    public static Texture2D Stop => GetTexture("d_StopButton");
    public static Texture2D Forward => GetTexture("d_Forward");
    public static Texture2D Back => GetTexture("d_Back");
    public static Texture2D Favorite => GetTexture("d_Favorite");
    public static Texture2D FavoriteIcon => GetTexture("d_Favorite Icon");
    public static Texture2D RGB => GetTexture("d_SceneViewRGB");
    public static Texture2D Settings => GetTexture("d_SettingsIcon");
}
