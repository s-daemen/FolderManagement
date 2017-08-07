using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using UnityEditor;
using System.Text;

namespace Localizational.Editor {


    /// <summary>
    /// Utility class for handling language files in the editor
    /// </summary>
    public static class LanguageHelper {

        public static LocCultureInfoCollection GetNonAvailableLanguages(LocCultureInfoCollection allCultures) {
            LocCultureInfoCollection nonCreatedLanguages = new LocCultureInfoCollection();

            foreach (LocCultureInfo cultureInfo in allCultures.CultureInfos) {
                if (!FileUtility.Exists(LocalizationalWorkSpace.LanguageFilePath(cultureInfo.LanguageCode))) {
                    nonCreatedLanguages.AddCultureInfo(cultureInfo);
                }
            }

            return nonCreatedLanguages;
        }

        public static LocCultureInfoCollection CheckAndSaveAvailableLanguages(LocCultureInfoCollection allCultures) {
            LocCultureInfoCollection createdCultures = new LocCultureInfoCollection();

            foreach (LocCultureInfo cultureInfo in allCultures.CultureInfos) {
                if (FileUtility.Exists(LocalizationalWorkSpace.LanguageFilePath(cultureInfo.LanguageCode))) {
                    createdCultures.AddCultureInfo(cultureInfo);
                }
            }

            createdCultures.Serialize(LocalizationalWorkSpace.AvailableCulturesFilePath());
            return createdCultures;
        }

        #region Language Creation

        public static void CreateRootResourceFile() {
            var baseDictionary = new Dictionary<string, string> { { "First Key", string.Empty } };
            SaveLanguageFile(baseDictionary, LocalizationalWorkSpace.RootLanguageFilePath());
        }

        public static void CreateNewLanguage(string languageName) {
          Dictionary<string, string> rootValues = LoadLanguageFile(null, true);
        }


        #endregion

        #region Language Saving
        /// <summary>
        /// This saves the given language as a .resx file. A DotNet Resource file.
        /// A ResX file is typically used for localization purposes. 
        /// So I wanted to use these.
        /// </summary>
        /// <param name="languageValueDictionary"></param>
        /// <param name="filePath"></param>
        public static void SaveLanguageFile(Dictionary<string, string> languageValueDictionary, string filePath) {
            TextAsset resxHeader = Resources.Load("resxheader") as TextAsset;

            if (resxHeader == null) {
                Debug.LogError("The ResX Header file was not found in the plugin's resources folder!");
                return;
            }
            string headerText = resxHeader.text;

            using (XmlTextWriter writer = new XmlTextWriter(filePath, Encoding.UTF8)) {
                writer.Formatting = Formatting.Indented;
                writer.Settings.Encoding = Encoding.UTF8;
                writer.Settings.Indent = true;

                //Write the actual document.
                writer.WriteStartDocument();
                writer.WriteStartElement("root");
                writer.WriteString("\n");
                writer.WriteRaw(headerText);
                writer.WriteString("\n");

                //Actual content values
                foreach (KeyValuePair<string, string> pair in languageValueDictionary) {
                    writer.WriteString("\t");
                    writer.WriteStartElement("data");
                    writer.WriteAttributeString("name", pair.Key);
                    writer.WriteAttributeString("xml:space", "preserve");
                    writer.WriteString("\n\t\t"); //Proper identation

                    writer.WriteStartElement("value");
                    writer.WriteString(pair.Value);
                    writer.WriteEndElement(); //Close the value element
                    writer.WriteString("\n\t");
                    writer.WriteEndElement(); //Close the data element
                    writer.WriteString("\n");
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }

        #endregion

        #region Language Loading
        private static Dictionary<string, string> LoadLanguageFile(string languageCode, bool isRootFile) {
            Dictionary<string, string> loadedLanguage = LoadParsedLanguageFile(languageCode, isRootFile);

            return loadedLanguage;
        }

        private static Dictionary<string, string> LoadParsedLanguageFile(string languageCode, bool isRootFile) {
            string fileContents = string.Empty;
            string filePath = null;

            if (isRootFile) {
                filePath = LocalizationalWorkSpace.ResourcesFolderFilePath() + "/" +
                           LocalizationalWorkSpace.rootLanguageName + LocalizationalWorkSpace.txtFileEnding;
            }
            else {
                filePath = LocalizationalWorkSpace.ResourcesFolderFilePath() + "/" +
                           LocalizationalWorkSpace.rootLanguageName+ "." + languageCode + LocalizationalWorkSpace.txtFileEnding;
            }

            if (!FileUtility.ReadFromFile(filePath, out fileContents)) {
                Debug.LogError("Failed to read language from file - " + filePath);
            }
            var loadedLanguage = LanguageParser.LoadLanguage(fileContents);
            return new Dictionary<string, string>(loadedLanguage);
        }

        #endregion

    }
}