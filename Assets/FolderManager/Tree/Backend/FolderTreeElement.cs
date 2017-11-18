using System;
using SD.FolderManagement.Model;
using UnityEngine;

namespace SD.FolderManagement {
    [Serializable]
    public class FolderTreeElement : TreeElement {
        public string Comment = "";
        public Texture2D Icon;
        public bool Selected;

        public FolderTreeElement(string name, int depth, int id) : base(name, depth, id) {
            Selected = true;
        }

        public string GetPath() {
            return GetParent(this, 0);
        }

        public string GetParent(TreeElement parent, int depth) {
            if (parent == null)
                return "";

            if (parent.Parent == null)
                return "";

            return GetParent(parent.Parent, 0) + "/" + parent.Name;
        }
    }
}