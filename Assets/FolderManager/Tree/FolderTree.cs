using System.Collections.Generic;
using UnityEngine;

namespace SD.FolderManagement {
    public class FolderTree : ScriptableObject {
        [SerializeField] private List<FolderTreeElement> _treeElements = new List<FolderTreeElement>();

        [SerializeField] public string TreeName = "FolderTree";

        public List<FolderTreeElement> TreeElements {
            get { return _treeElements; }
            set { _treeElements = value; }
        }

        public void AddElement(FolderTreeElement element) {
            TreeElements.Add(element);
        }

        public void AddElement(string name, int depth, int id) {
            var element = new FolderTreeElement(name, depth, id);
            TreeElements.Add(element);
        }

        public void RemoveElement(int index) {
            TreeElements.RemoveAt(index);
        }

        public void RemoveElement(FolderTreeElement element) {
            TreeElements.Remove(element);
        }

        public void RemoveElements(IList<FolderTreeElement> elements) {
            foreach (var element in elements) TreeElements.Remove(element);
        }
    }
}