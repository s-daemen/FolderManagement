using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SD.FolderManagement.Editor;
using SD;
using UnityEditor;
namespace SD.FolderManagement.Editor {

    public class WindowUtility {

        public static bool ShouldShowWindow() {
            if (!FolderManager.Exists()) {
                
                FolderManagerGUI.Title("First Time Setup");

                if (GUILayout.Button("Start Making My Life Easier!")) {
                    if (FolderManager.Create()) {
                        return true;
                    }
                }
                return false;
            } else {
                return true;
            }
        }
        public void Test() {
            
        }
    }

}
