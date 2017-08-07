using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Localizational.Editor {

    public class ImportHelper : AssetPostprocessor {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

            if (!LocalizationalWorkSpace.Exists()) return;

            foreach (string str in importedAssets) {
                if (str.EndsWith(LocalizationalWorkSpace.resXFileEnding)) {
                    //Checks if there was a new resx file imported
                    string newFile = LocalizationalWorkSpace.ResourcesFolderFilePath() + "/" + Path.GetFileNameWithoutExtension(str) + LocalizationalWorkSpace.txtFileEnding;

                    if (!DirectoryUtility.CheckAndCreate(LocalizationalWorkSpace.ResourcesFolderFilePath())) return;

                    //Checks if the file already exists and deletes it if so.
                    if (FileUtility.Exists(newFile)) {
                        FileUtility.Delete(newFile);
                    }

                    string data = string.Empty;

                    using (StreamReader reader = new StreamReader(str)) {
                        data = reader.ReadToEnd();
                    }

                    FileUtility.WriteToFile(newFile, data);

                    LocCultureInfoCollection allCultures = LocCultureXMLHelper.Deserialize(LocalizationalWorkSpace.CultureInfoCollectionFilePath());
                    LanguageHelper.CheckAndSaveAvailableLanguages(allCultures);

                    AssetDatabase.Refresh(ImportAssetOptions.Default);
                }
            }
        }
    }
}
