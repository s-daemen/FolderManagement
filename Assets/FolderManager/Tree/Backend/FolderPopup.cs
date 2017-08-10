using UnityEngine;
using UnityEditor;

namespace SD.FolderManagement.Editor {


    public class FolderPopup : PopupWindowContent {
        private string newTreeName = "";

        public FolderPopup() { }

        public FolderPopup(string msg) {
            
        }

        public override Vector2 GetWindowSize() {
            return new Vector2(350, 150);
        }

        public override void OnGUI(Rect rect) {
            GUILayout.Label("Create a new folder tree", EditorStyles.boldLabel);
            newTreeName = EditorGUILayout.TextField("Folder name: ", newTreeName);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel")) {
                
            }

            if (GUILayout.Button("Accept")) {
                
            }
            EditorGUILayout.EndHorizontal();
        }

        public override void OnOpen() {
            Debug.Log("Popup opened: " + this);
        }

        public override void OnClose() {
            Debug.Log("Popup closed: " + this);
        }
    }
}
