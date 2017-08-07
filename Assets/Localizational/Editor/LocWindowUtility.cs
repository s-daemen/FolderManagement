using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Localizational.Editor {



    public static class LocWindowUtility {

        public static bool ShouldShowWindow() {
            if (!LocalizationalWorkSpace.Exists()) {
                GUILayout.Label("First Time Setup");

                if (GUILayout.Button("Start Translating!")) {
                    if (LocalizationalWorkSpace.Create()) {
                        return true;
                    }
                }
                return false;
            }
            else {
                return true;
            }
            
        }
    }
}
