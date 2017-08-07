using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SD.FolderManagement {

    public class FolderTreeAsset : ScriptableObject {

        [SerializeField] private List<FolderTreeElement> _treeElements = new List<FolderTreeElement>();

        internal List<FolderTreeElement> TreeElements {
            get { return _treeElements; }
            set { _treeElements = value; }
        }

        private void Awake() {
            if (_treeElements.Count == 0) {
                //TODO
            }
        }
    }
}
