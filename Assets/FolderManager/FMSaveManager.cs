using System.Collections;
using System.Collections.Generic;
using SD.FolderManagement.Utility;
using UnityEngine;

namespace SD.FolderManagement {
    public static class FolderManagerSaveManager {

        public static void SaveFolderTree(string path, FolderTree folderTree, bool createWorkingCopy) {

            if (string.IsNullOrEmpty(path)) throw new UnityException("Cannot save Folder Tree: No spath specified to save the Folder Tree" + (folderTree != null ? folderTree.name : "") + " to!");
            if (folderTree == null) throw new UnityException("Cannot save NodeCanvas: The specified NodeCanvas that should be saved to path " + path + " is null!");
            if (!createWorkingCopy && UnityEditor.AssetDatabase.Contains(folderTree) &&
                UnityEditor.AssetDatabase.GetAssetPath(folderTree) != path) {
                Debug.LogError("Trying to create a duplicate save file for '" + folderTree.name + "'!");
                return;
            }

            ProcessFolderTree(ref folderTree, createWorkingCopy);
            UnityEditor.AssetDatabase.CreateAsset(folderTree, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }

        private static void ProcessFolderTree(ref FolderTree folderTree, bool createWorkingCopy) {
                folderTree = CreateWorkingCopy(folderTree);
        }

        public static FolderTree LoadFolderTree(string path, bool createWorkingCopy) {
            if (!FileUtility.Exists(path)) throw new UnityException("Cannot load the Folder Tree asset: File + '" + path + "' does not exist!");

            FolderTree folderTree = LoadResource<FolderTree>(path);
            if (folderTree == null) throw new UnityException("Cannot Load Folder Tree: The file at the specified path '" + path + "' is no valid save file as it does not contain a Folder Tree!");

            ProcessFolderTree(ref folderTree, createWorkingCopy);

            UnityEditor.AssetDatabase.Refresh();
            return folderTree;
        }

        #region Utility

        private static FolderTree CreateWorkingCopy(FolderTree folderTree) {
            folderTree = Clone(folderTree);
            return folderTree;
        }

        private static T Clone<T>(T SO) where T : ScriptableObject {
            string soName = SO.name;
            SO = Object.Instantiate<T>(SO);
            SO.name = soName;
            return SO;
        }


        public static T LoadResource<T>(string path) where T : Object {
            path = PreparePath(path);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);

        }

        public static string PreparePath(string path) {
            path = path.Replace(Application.dataPath, "Assets");
            if (!path.StartsWith("Assets/"))
                path = "" + path;

            return path;

        }

        #endregion

    }
}