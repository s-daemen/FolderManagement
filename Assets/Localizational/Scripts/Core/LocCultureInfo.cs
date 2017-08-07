using System;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

namespace Localizational {

    [XmlRoot("CultureCollections"), System.Serializable]
    public class LocCultureInfoCollection {

        public const int LATEST_VERSION = 1;

        [XmlElement(ElementName = "version")]
        public int Version = 0;

        [XmlArray("CultureInfos"), XmlArrayItem("CultureInfo")]
        public List<LocCultureInfo> CultureInfos = new List<LocCultureInfo>();

        public void RemoveCultureInfo(LocCultureInfo info) {

            if (info == null) {
                Debug.LogError("Cannot remove a CultureInfo that's null!");
                return;
            }

            if (!CultureInfos.Remove(info)) {
                Debug.LogError("Something went wrong trying to delete a CultureInfo!");
            }
        }

        public void AddCultureInfo(LocCultureInfo info) {
            if (info == null) {
                Debug.LogError("Cannot add a CultureInfo that's null!");
                return;
            }

            CultureInfos.Add(info);
        }



        public LocCultureInfo FindCulture(LocCultureInfo cultureInfo) {
            if (cultureInfo == null) {
                return null;
            }
            return CultureInfos.Find(c =>
                string.Equals(c.EnglishName, cultureInfo.EnglishName, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(c.LanguageCode, cultureInfo.LanguageCode, StringComparison.CurrentCultureIgnoreCase));
        }

        public LocCultureInfo FindCulture(string languageCode) {
            if (string.IsNullOrEmpty(languageCode)) {
                return null;
            }

            return CultureInfos.Find(c =>
                string.Equals(c.LanguageCode.ToLower(), languageCode));
        }

        public bool IsCultureInCollection(LocCultureInfo cultureInfo) {
            return FindCulture(cultureInfo) != null;
        }
    }

    [System.Serializable]
    public class LocCultureInfo {
        public string LanguageCode { get; private set; }
        public string EnglishName { get; private set; }
        public string NativeName { get; private set; }
        public bool IsRightToLeft { get; private set; }
        public string CalendarFormat { get; private set; }

        public LocCultureInfo() {}

        public LocCultureInfo(string languageCode, string englishName, string nativeName, bool isRightToLeft) {
            LanguageCode = languageCode;
            EnglishName = englishName;
            NativeName = nativeName;
            IsRightToLeft = isRightToLeft;
        }

        public LocCultureInfo(string languageCode, string englishName, string nativeName, bool isRightToLeft, string calendarFormat) {
            LanguageCode = languageCode;
            EnglishName = englishName;
            NativeName = nativeName;
            IsRightToLeft = isRightToLeft;
            CalendarFormat = calendarFormat;
        }

        public override string ToString() {
            return string.Format(@"[LocCultureInfo LanguageCode=""{0}"" EnglishName={1} NativeName={2} IsRightToLeft={3}]",
                LanguageCode, EnglishName, NativeName, IsRightToLeft.ToString());
        }
    }
}