using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SD.FolderManagement {
    public static class FolderManagerCallbacks {

        public static Action<FolderTreeElement> OnAddElement;
        public static void IssueOnAddElement(FolderTreeElement element) {
            if(OnAddElement != null)
                OnAddElement.Invoke(element);
        }

        public static Action<FolderTreeElement> OnDeleteElement;
        public static void IssueOnDeleteElement(FolderTreeElement element) {
            if (OnDeleteElement != null)
                OnDeleteElement.Invoke(element);
        }

    }
}
