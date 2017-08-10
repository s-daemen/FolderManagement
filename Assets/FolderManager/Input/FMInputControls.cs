using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SD.FolderManagement {
    public class FMInputControls {

        [ContextFillerAttribute(ContextType.FolderTree)]
        private static void FillContextAddFolders(FolderManagerInputInfo inputInfo, GenericMenu contextMenu) {
            FolderTreeViewState state = inputInfo.FolderTreeState;
            contextMenu.AddItem(new GUIContent("Add Folder"), false, CreateContextCallback, new FolderManagerInputInfo("Test Message", state));
        }

        private static void CreateContextCallback(object userdata) {
            FolderManagerInputInfo inputInfo = userdata as FolderManagerInputInfo;

            int lastID = inputInfo.FolderTreeState.Cache.FolderTree.TreeElements[inputInfo.FolderTreeState.Cache.FolderTree.TreeElements.Count - 1].Id + 1;

            FolderTreeElement element = new FolderTreeElement("Folder Name", 0, lastID);
            inputInfo.FolderTreeState.Cache.FolderTree.AddElement(element);

            FolderManagerCallbacks.IssueOnAddElement(element);
            FolderManager.RepaintClients();
        }

        [ContextFillerAttribute(ContextType.Folder)]
        private static void FillContextAddItems(FolderManagerInputInfo inputInfo, GenericMenu contextMenu) {
            if (inputInfo.FolderTreeState.SelectedItem != null) {
                FolderTreeViewState state = inputInfo.FolderTreeState;

                contextMenu.AddItem(new GUIContent("Add Child Folder"), false, CreateContextItemCallback, new FolderManagerInputInfo("", state));
            }
        }

        private static void CreateContextItemCallback(object userData) {
            FolderManagerInputInfo inputInfo = userData as FolderManagerInputInfo;
            int lastID = inputInfo.FolderTreeState.Cache.FolderTree.TreeElements[inputInfo.FolderTreeState.Cache.FolderTree.TreeElements.Count - 1].Id + 1;

            FolderTreeElement element = new FolderTreeElement("Folder Name", 0 , lastID);
            inputInfo.FolderTreeState.Cache.FolderTree.AddElement(element);

            FolderManagerCallbacks.IssueOnAddElement(element);
            FolderManager.RepaintClients();
        }

        [ContextEntry(ContextType.Folder, "Delete Folder")]
        private static void DeleteFolder(FolderManagerInputInfo inputInfo) {
            Debug.Log("Delete Folder");

            FolderTreeElement element =
                inputInfo.FolderTreeState.Cache.FolderTree.TreeElements.FirstOrDefault(s => s.Id == inputInfo.FolderTreeState.SelectedItem.id);

            inputInfo.FolderTreeState.Cache.FolderTree.RemoveElement(element);
            FolderManagerCallbacks.IssueOnDeleteElement(element);
            inputInfo.InputEvent.Use();
        }

        [ContextEntry(ContextType.Folder, "Duplicate Folder")]
        private static void DuplicateFolder(FolderManagerInputInfo inputInfo) {
            Debug.Log("Duplicate Folder");

            inputInfo.InputEvent.Use();
        }
    }
}
