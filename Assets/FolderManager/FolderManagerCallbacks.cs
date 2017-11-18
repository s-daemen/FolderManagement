using System;

namespace SD.FolderManagement {
    public static class FolderManagerCallbacks {
        public static Action<FolderTreeElement> OnAddElement;

        public static Action<FolderTreeElement> OnDeleteElement;

        public static void IssueOnAddElement(FolderTreeElement element) {
            if (OnAddElement != null)
                OnAddElement.Invoke(element);
        }

        public static void IssueOnDeleteElement(FolderTreeElement element) {
            if (OnDeleteElement != null)
                OnDeleteElement.Invoke(element);
        }
    }
}