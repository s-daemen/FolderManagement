using UnityEngine;

namespace SD.FolderManagement.Editor {
    public class WindowUtility {
        public static bool ShouldShowWindow() {
            if (!FolderManager.Exists()) {
                FolderManagerGUI.Title("First Time Setup");
                GUILayout.Space(20);
                if (GUILayout.Button("Start Making My Life Easier!")) if (FolderManager.Create()) return true;
                return false;
            }
            return true;
        }
    }
}