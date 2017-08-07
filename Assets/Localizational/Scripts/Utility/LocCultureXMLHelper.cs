using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
namespace Localizational {

    public static class LocCultureXMLHelper {

        public static LocCultureInfoCollection Deserialize(string fullPath) {
            XmlSerializer serializer = new XmlSerializer(typeof(LocCultureInfoCollection));
            LocCultureInfoCollection deserializedCollection = null;
            using (FileStream stream = new FileStream(fullPath, FileMode.Open)) {
                deserializedCollection = serializer.Deserialize(stream) as LocCultureInfoCollection;
            }
            return deserializedCollection;
        }

        public static void Serialize(this LocCultureInfoCollection info, string fullPath) {

            XmlSerializer serializer = new XmlSerializer(typeof(LocCultureInfoCollection));

            using (XmlTextWriter writer = new XmlTextWriter(fullPath, Encoding.UTF8)) {
                writer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, info);
            }
        }
    }

}
