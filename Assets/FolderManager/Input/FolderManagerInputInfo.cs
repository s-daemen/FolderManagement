using UnityEngine;

namespace SD.FolderManagement {
    public class FolderManagerInputInfo {

        public FolderManagerInputInfo() {
            Message = string.Empty;
            InputEvent = Event.current;
            InputPos = InputEvent.mousePosition;
            FolderTreeState = null;
        }

        public FolderManagerInputInfo(string message) {
            Message = message;
            InputEvent = Event.current;
            InputPos = InputEvent.mousePosition;
            FolderTreeState = null;
        }

        public FolderManagerInputInfo(FolderTreeViewState state) {
            Message = string.Empty;
            InputEvent = Event.current;
            InputPos = InputEvent.mousePosition;
            FolderTreeState = state;
        }

        public FolderManagerInputInfo(string message, FolderTreeViewState state) {
            Message = message;
            InputEvent = Event.current;
            InputPos = InputEvent.mousePosition;
            FolderTreeState = state;
        }

        public string Message { get; private set; }
        public Event InputEvent { get; private set; }
        public Vector2 InputPos { get; private set; }
        public FolderTreeViewState FolderTreeState { get; private set; }
    }
}