using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SD.FolderManagement {

    public class FolderTree : ScriptableObject {

        public virtual string TreeName {
            get { return "Folder Tree"; }
            set { TreeName = value; }
        }

        [SerializeField]
        private List<FolderTreeElement> _treeElements = new List<FolderTreeElement>();

        public List<FolderTreeElement> TreeElements {
            get { return _treeElements; }
            set { _treeElements = value; }
        }

        public void AddElement(FolderTreeElement element)
        {
            TreeElements.Add(element);
        }

        public void AddElement(string name, int depth, int id) {
            FolderTreeElement element = new FolderTreeElement(name, depth, id);
            TreeElements.Add(element);
        }

        public void RemoveElement(int index) {
            if (index == 0) {
                return;
            }
            TreeElements.RemoveAt(index);
        }

        public void RemoveElement(FolderTreeElement element) {
            if (element.Id == 0) {
                return;
            }
            TreeElements.Remove(element);
        }

        public void RemoveElements(IList<FolderTreeElement> elements) {
            foreach (FolderTreeElement element in elements) {
                TreeElements.Remove(element);
            }
        }
    }
}
