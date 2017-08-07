using System;
using SD.FolderManagement.Model;
using UnityEngine;

namespace SD.FolderManagement {

    [Serializable]
    internal class FolderTreeElement : TreeElement {
        public Texture2D Icon;
        public string FolderName;
        public bool Selected;
        public string Comment = "";
        public FolderTreeElement(string name, int depth, int id) : base(name, depth, id) {
            Selected = true;
        }
    }
}
