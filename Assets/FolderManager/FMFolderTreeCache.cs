using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using SD.FolderManagement.Utility;
using UnityEngine;

namespace SD.FolderManagement {

    public class FolderTreeCache {
        public FolderTree FolderTree;

        public string OpenedCanvasPath = "";
        private string _cachePath;
        public string LastSessionPath { get { return _cachePath + "/LastSession.asset"; } }

        public FolderTreeCache(string cachePath) {

            _cachePath = cachePath;
        }

        public void AssureFolderTree() {
            if (FolderTree == null)
            {
                NewFolderTree();
            }
        }


        #region Cache

        public void SetupCacheEvents() {
            LoadCache();
        }

        public void ClearCacheEvents() {

        }

        private void SaveCache() {
            FolderManagerSaveManager.SaveFolderTree(LastSessionPath, FolderTree, false);
        }


        private void LoadCache() {

            if (!FileUtility.Exists(LastSessionPath) || (FolderTree =
                    FolderManagerSaveManager.LoadFolderTree(LastSessionPath, false)) == null) {
                NewFolderTree();
                return;
            }
            FolderManager.RepaintClients();
        }

        #endregion

        public void SaveFolderTree(string path)
        {
            FolderManagerSaveManager.SaveFolderTree(path, FolderTree, true);
            FolderManager.RepaintClients();
        }

        public void LoadFolderTree(string path) {
            if (!FileUtility.Exists(path) || (FolderTree = FolderManagerSaveManager.LoadFolderTree(path, true)) == null) {
                NewFolderTree();
                return;
            }
            OpenedCanvasPath = path;

            SaveCache();
            FolderManager.RepaintClients();
        }

        private void NewFolderTree() {
            FolderTree = ScriptableObject.CreateInstance<FolderTree>();
            FolderTree.name = "New " + FolderTree.TreeName;
            FolderTree.TreeElements = new List<FolderTreeElement>()
            {
                new FolderTreeElement("root", -1, -1),
                new FolderTreeElement("Root", 0, 0)
            };
            OpenedCanvasPath = "";

            SaveCache();
        }
    }
}
