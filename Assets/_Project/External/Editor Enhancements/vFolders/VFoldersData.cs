#if UNITY_EDITOR
using System;
using UnityEngine;
using static VFolders.Libs.VUtils;

namespace VFolders
{
public class VFoldersData : ScriptableObject
{
    public SerializeableDicitonary<string, FolderData> folderDatasByGuid = new SerializeableDicitonary<string, FolderData>();

    [Serializable]
    public class FolderData
    {
        public string guid;

        public Color color => VFoldersIconEditor.GetColor(iColor);
        public int iColor;

        public string icon
        {
            get
            {
                var icon = VFoldersMenuItems.customIconsEnabled ? customIcon : "";

                if (icon == "") icon = VFoldersMenuItems.autoIconsEnabled ? autoIcon : "";

                if (icon == "none") icon = "";

                return icon;
            }
            set
            {
                if (value == autoIcon && VFoldersMenuItems.autoIconsEnabled) customIcon = "";
                else if (value == "") customIcon                                        = "none";
                else customIcon                                                         = value;
            }
        }
        public string autoIcon = "";
        public string customIcon = "";
        public bool autoIconDirty = true;

        public FolderData(string guid) => this.guid = guid;
    }
}
}
#endif
