using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SD.FolderManagement {

    public class FolderTreeViewState : TreeViewState {

        public TreeViewItem SelectedItem { get { return GetSelectedItem(); } }

        public TreeView View { get; set; }
        public FolderTreeCache Cache { get; set; }

        public IList<TreeViewItem> GetSelectedItems() {
            IList<TreeViewItem> selectedItems = new List<TreeViewItem>();
            foreach (int selectedID in selectedIDs) {
                if (selectedID >= 0)
                    selectedItems.Add(View.GetRows().FirstOrDefault(t => t.id == selectedID));
            }
            return selectedItems;
        }

        private TreeViewItem GetSelectedItem() {
            IList<TreeViewItem> items = GetSelectedItems();
            if (items != null && items.Count > 0) {

                var selectedItem = items.FirstOrDefault(s => s.id == lastClickedID);
                return selectedItem;
            }
            return null;
        }
    }
}
