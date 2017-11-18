using System;
using System.IO;
using UnityEngine;

namespace SD.FolderManagement.Utility {
    /// <summary>
    ///     This is a utility class for handling folder/directory operations
    /// </summary>
    public class DirectoryUtility {
      
        #region Folder Deletion

        public static void DeleteAllFilesAndFolders(string path, bool useRecursion = true, bool isTop = true) {
            if (!Exists(path)) return;

            foreach (var file in GetFiles(path)) File.Delete(file);
            if (useRecursion)
                foreach (var directory in Directory.GetDirectories(path))
                    DeleteAllFilesAndFolders(directory, useRecursion, false);
            if (!isTop) Directory.Delete(path);
        }

        #endregion

        #region Lookups

        public static bool Exists(string path) {
            return Directory.Exists(path);
        }

        public static bool ExistsRelative(string path) {
            return Exists(GetAppDataPath() + path);
        }

        public static string[] GetFiles(string path) {
            return Directory.GetFiles(path);
        }

        public static string[] GetFilesRelative(string path) {
            return GetFiles(GetAppDataPath() + path);
        }

        public static string GetAppDataPath() {
            return Application.dataPath;
        }

        public static string GetDirectoryName(string path) {
            return Path.GetDirectoryName(path);
        }

        #endregion

        #region Folder Creation

        public static bool Create(string path) {
            try {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception exception) {
                Debug.LogError("Failed to create directory at path: " + path + " Error: " + exception.Message);
                return false;
            }
        }

        public static bool CheckAndCreate(string path) {
            return Exists(path) || Create(path);
        }

        public static bool CreateRelative(string path) {
            return Create(GetAppDataPath() + path);
        }

        public static bool CheckAndCreateRelative(string path) {
            return Exists(path) || CreateRelative(path);
        }

        #endregion
    }
}