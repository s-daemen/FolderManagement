using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SD.FolderManagement.Utility {

    public class FileUtility {

        #region Lookups

        public static bool Exists(string path) {
            return File.Exists(path);
        }

        public static bool ExistsRelative(string path) {
            return Exists(DirectoryUtility.GetAppDataPath() + path);
        }

        public static string GetFileExtension(string fileName, string relativePath) {
            string fullPath = DirectoryUtility.GetAppDataPath() + relativePath;

            if (!DirectoryUtility.Exists(fullPath)) {
                Debug.LogError("The file at the given folder path was not found!");
                return string.Empty;
            }

            string[] filesInFolder = DirectoryUtility.GetFiles(fullPath);
            foreach (string file in filesInFolder) {
                if (!file.EndsWith(".meta")) {
                    string currentfileName = RemoveExtension(file);
                    if (fileName == currentfileName) {
                        return GetFileExtension(file);
                    }
                }
            }
            return string.Empty;
        }

        private static string GetFileExtension(string path) {
            return Path.GetExtension(path);
        }

        private static string RemoveExtension(string file) {
            return Path.GetFileNameWithoutExtension(file);
        }

        #endregion

        #region File Handling

        public static bool Create(string path, string data) {
            try {
                File.WriteAllText(path, data);
                return true;
            } catch (System.Exception exception) {
                Debug.LogError("Could not Save/Create file! Error: " + exception.Message);
                return false;
            }
        }

        public static bool ReadFile(string path, out string data) {
            if (!Exists(path)) {
                data = string.Empty;
                Debug.LogError("File does not exist!");
                return false;
            }

            try {
                data = File.ReadAllText(path);
                return true;
            } catch (Exception exception) {
                data = string.Empty;
                Debug.LogError("Could not load the content of the file: " + path + " Error: " + exception.Message);
                return false;
            }
        }

        public static bool Delete(string path) {
            try {
                File.Delete(path);
                return true;
            } catch (Exception exception) {
                Debug.LogError("Failed to delete file. Error: " + exception.Message);
                return false;
            }
        }

        #endregion
    }
}