using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace SD.FolderManagement {
    public class FolderTreeViewState : TreeViewState {
        public TreeViewItem SelectedItem {
            get { return GetSelectedItem(); }
        }

        public FolderTreeView View { get; set; }
        public FolderTreeCache Cache { get; set; }

        public IList<TreeViewItem> GetSelectedItems() {
            return View.GetSelectedItemsAsItems();
        }

        private TreeViewItem GetSelectedItem() {
            return View.GetSelectedItem();
        }
    }
}