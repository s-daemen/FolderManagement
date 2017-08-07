using UnityEngine;
using System.Collections;
using Localizational.ReorderableList;
using UnityEditor;

namespace Localizational.Editor {

    internal class SettingsMenuControl : ReorderableListControl {
        public SettingsMenuControl() : base(ReorderableListFlags.HideAddButton | ReorderableListFlags.DisableContextMenu) { }
    }
}