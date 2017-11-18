using System.Linq;
using UnityEngine;

namespace SD.FolderManagement {
    public class FMInputControls {
        [ContextFiller(ContextType.FolderTree)]
        private static void FillContextAddFolders(FolderManagerInputInfo inputInfo, GenericMenu contextMenu) {
            var state = inputInfo.FolderTreeState;
            contextMenu.AddItem(new GUIContent("Add Folder"), false, CreateContextCallback,
                new FolderManagerInputInfo("Test Message", state));
        }

        private static void CreateContextCallback(object userData) {
            var inputInfo = userData as FolderManagerInputInfo;

            if (inputInfo != null) {
                var sortedList = inputInfo.FolderTreeState.Cache.FolderTree.TreeElements;
                sortedList.Sort((e1, e2) => e1.Id.CompareTo(e2.Id));
                var lastID = sortedList[sortedList.Count - 1].Id + 1;

                var element = new FolderTreeElement("Folder Name", 0, lastID);
                inputInfo.FolderTreeState.Cache.FolderTree.AddElement(element);

                FolderManagerCallbacks.IssueOnAddElement(element);
            }
            FolderManager.RepaintClients();
        }
        /*
        [ContextFillerAttribute(ContextType.Folder)]
        private static void FillContextAddItems(FolderManagerInputInfo inputInfo, GenericMenu contextMenu) {
            if (inputInfo.FolderTreeState.SelectedItem != null) {
                FolderTreeViewState state = inputInfo.FolderTreeState;

                contextMenu.AddItem(new GUIContent("Add Child Folder"), false, CreateContextItemCallback, new FolderManagerInputInfo("", state));
            }
        }

        private static void CreateContextItemCallback(object userData) {
            var inputInfo = userData as FolderManagerInputInfo;

            if (inputInfo != null) {
                var sortedList = inputInfo.FolderTreeState.Cache.FolderTree.TreeElements;
                sortedList.Sort((e1, e2) => e1.Id.CompareTo(e2.Id));
                var lastID = sortedList[sortedList.Count - 1].Id + 1;

                var element = new FolderTreeElement("Folder Name", 0, lastID) {
                    Parent = inputInfo.FolderTreeState.Cache.FolderTree.TreeElements.Find(
                        s => s.Id == inputInfo.FolderTreeState.lastClickedID)
                };
                element.Depth = element.Parent.Depth + 1;
                inputInfo.FolderTreeState.Cache.FolderTree.AddElement(element);
                FolderManagerCallbacks.IssueOnAddElement(element);
            }

            FolderManager.RepaintClients();
        }*/

        [ContextEntry(ContextType.Folder, "Delete Folder")]
        private static void DeleteFolder(FolderManagerInputInfo inputInfo) {
            foreach (var item in inputInfo.FolderTreeState.GetSelectedItems()) {
                var element =
                    inputInfo.FolderTreeState.Cache.FolderTree.TreeElements.FirstOrDefault(s => s.Id == item.id);

                DeleteChildren(inputInfo, element);
            }

            inputInfo.InputEvent.Use();
        }

        private static void DeleteChildren(FolderManagerInputInfo inputInfo, FolderTreeElement element) {
            if (element.HasChildren)
                foreach (var treeElement in element.Children) {
                    var el = treeElement as FolderTreeElement;
                    inputInfo.FolderTreeState.Cache.FolderTree.RemoveElement(el);
                    DeleteChildren(inputInfo, el);
                }
            else inputInfo.FolderTreeState.Cache.FolderTree.RemoveElement(element);
            FolderManagerCallbacks.IssueOnDeleteElement(element);
        }

        /*
        [ContextEntry(ContextType.Folder, "Duplicate Folder")]
        private static void DuplicateFolder(FolderManagerInputInfo inputInfo) {
            Debug.Log("Duplicate Folder");

            inputInfo.InputEvent.Use();
        }*/
    }
}