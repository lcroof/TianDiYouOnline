using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml;

namespace DataPublic
{
    public static class SerializePublic
    {
        #region BinaryStream
        public static void WriteObjectToBinaryStream(this object dataObject, Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, dataObject);
        }

        public static object ReadObjectFromBinaryStream(this Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }
        #endregion

        #region XMLStream
        public static void WriteObjectToXmlStream(this object dataObject, Stream stream)
        {
            XmlWriter xmlWriter = XmlWriter.Create(stream);
            xmlWriter.WriteStartElement("XmlObject");
            xmlWriter.WriteElementString("TypeName", dataObject.GetType().FullName);
            DataContractSerializer serializer = new DataContractSerializer(dataObject.GetType());
            serializer.WriteObject(xmlWriter, dataObject);
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
        }

        public static object ReadObjectFromXmlStream(this Stream stream)
        {
            XmlTextReader xmlReader = new XmlTextReader(stream);
            xmlReader.Normalization = false;
            xmlReader.Read();
            xmlReader.ReadStartElement("XmlObject");
            string typeName = xmlReader.ReadElementString("TypeName");
            Type objectType = Type.GetType(typeName);
            if (objectType == null)
                throw new Exception(String.Format("类型{0}导入失败", typeName));
            DataContractSerializer serializer = new DataContractSerializer(objectType);
            object returnObject = serializer.ReadObject(xmlReader);
            return returnObject;
        }

        public static Type ReadObjectTypeFromXmlStream(this Stream stream)
        {
            XmlReader xmlReader = XmlReader.Create(stream);
            xmlReader.Read();
            xmlReader.ReadStartElement("XmlObject");
            string typeName = xmlReader.ReadElementString("TypeName");
            return Type.GetType(typeName);
        }
        #endregion


    }
}
