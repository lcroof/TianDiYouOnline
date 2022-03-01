using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DataPublic
{
    public static class ConvertPublic
    {
        #region Cast
        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public static T Cast<T>(this object sourceObject, T nullValue, string castErrorMessage, params object[] extendParameters)
        {
            if (sourceObject == null || sourceObject == Convert.DBNull)
            {
                return nullValue;
            }

            sourceObject.CheckType<T>(castErrorMessage, extendParameters);

            return (T)sourceObject;
        }

        /// <summary>
        /// 檢查對象是否為某個類別
        /// </summary>
        /// <param name="testObject"></param>
        /// <param name="message"></param>
        public static void CheckType<T>(this System.Object testObject,
            string message, params object[] extendParameters)
        {
            (testObject is T).CheckTrue(message
                , extendParameters.Concat(
                    "SourceType", testObject == null ? "Unkown" : testObject.GetType().AssemblyQualifiedName
                    , "DestType", typeof(T).AssemblyQualifiedName));
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public static T Cast<T>(this object sourceObject, T nullValue)
        {
            return sourceObject.Cast<T>(nullValue, "Object cannot cast to new type.");
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public static T Cast<T>(this object sourceObject)
        {
            return sourceObject.Cast<T>(default(T));
        }

        /// <summary>
        /// 轉換為數組
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        public static T[] ConvertToArray<T>(this object sourceObject)
        {
            T[] array;
            if (sourceObject is T[])
            {
                array = sourceObject.Cast<T[]>();
            }
            else if (sourceObject is T)
            {
                array = new T[] { sourceObject.Cast<T>() };
            }
            else
            {
                throw new Exception(String.Format("对象{0}不能转化为{1}类型数组", sourceObject.ToString(), typeof(T)));
            }
            return array;
        }
        #endregion

        #region DataSet/DataTable/DataRow

        public static string DataSetToXmlString(this System.Data.DataSet dataSet)
        {
            StringWriter stringWriter = new StringWriter();
            dataSet.WriteXml(stringWriter);
            return stringWriter.ToString();
        }

        public static string DataTableToXmlString(this System.Data.DataTable dataTable)
        {
            StringWriter stringWriter = new StringWriter();
            dataTable.WriteXml(stringWriter);
            return stringWriter.ToString();
        }

        public static void DataSetFromXmlString(this System.Data.DataSet dataSet, string xml)
        {
            StringReader stringReader = new StringReader(xml);
            dataSet.ReadXml(stringReader);
        }

        public static void DataTableFromXmlString(this System.Data.DataTable dataTable, string xml)
        {
            StringReader stringReader = new StringReader(xml);
            dataTable.ReadXml(stringReader);
        }

        public static string DataRowToXmlString(this System.Data.DataRow dataRow)
        {
            System.Data.DataSet dataSet = new System.Data.DataSet();
            dataSet.Merge(new System.Data.DataRow[] { dataRow });

            return DataTableToXmlString(dataSet.Tables[0]);
        }

        #endregion

        #region Object <-> XDocument

        public static object XmlDocumentToObject(this System.Xml.XmlDocument xmlData)
        {
            System.IO.MemoryStream memoryStream = new MemoryStream();
            xmlData.Save(memoryStream);

            // 读到Object中
            memoryStream.Seek(0, SeekOrigin.Begin);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter binaryFormatter = new BinaryFormatter();
            return binaryFormatter.Deserialize(memoryStream);
        }

        public static XmlDocument ObjectToXmlDocument(this object data)
        {
            // 写xml
            System.IO.MemoryStream memoryStream = new MemoryStream();
            System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(memoryStream);
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            serializer.Serialize(xmlWriter, data);

            // 读到Document中
            memoryStream.Seek(0, SeekOrigin.Begin);
            System.Xml.XmlDocument doc = new XmlDocument();
            doc.Load(memoryStream);

            return doc;
        }

        #endregion

        #region Object <-> XmlString

        public static string ObjectToXmlString(this object data)
        {
            System.IO.StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
            DataContractSerializer serializer = new DataContractSerializer(data.GetType());
            serializer.WriteObject(xmlWriter, data);

            return stringWriter.ToString();
        }

        public static object XmlStringToObject(this string xmlString, Type objectType)
        {
            if (xmlString.IsNullOrEmpty())
                return null;

            System.IO.StringReader stringReader = new StringReader(xmlString);
            XmlTextReader xmlReader = new XmlTextReader(stringReader);
            xmlReader.Read();
            DataContractSerializer serializer = new DataContractSerializer(objectType);
            return serializer.ReadObject(xmlReader);
        }

        public static object XmlStringToObject(this string xmlString, string objectTypeName)
        {
            return xmlString.XmlStringToObject(Type.GetType(objectTypeName));
        }

        public static T XmlStringToObject<T>(this string xmlString)
        {
            return (T)XmlStringToObject(xmlString, typeof(T));
        }
        #endregion

        #region Binary <-> Object

        /// <summary>
        /// 將對象轉換為二進制數據
        /// </summary>
        /// <param name="data">對象</param>
        /// <returns>將對象二進制序列化所得到的二進制數據</returns>
        public static byte[] ObjectToBinary(this object data)
        {
            // Write
            System.IO.MemoryStream memoryStream = new MemoryStream();
            data.WriteObjectToBinaryStream(memoryStream);

            return memoryStream.GetBuffer();
        }

        /// <summary>
        /// 將二進制數據轉換為對象
        /// </summary>
        /// <param name="bytes">二進制數據</param>
        /// <returns>二進制數據反序列化以後生成的對象</returns>
        public static object BinaryToObject(this byte[] bytes)
        {
            if (bytes.IsNullOrEmpty())
                return null;

            System.IO.MemoryStream memoryStream = new MemoryStream(bytes);
            return memoryStream.ReadObjectFromBinaryStream();
        }
        #endregion

        #region SortedList <-> KeyValuePair <-> KeyValueTable <-> Object
        public static SortedList<TKey, TValue> KeyValueTableToSortedList<TKey, TValue>(this KeyValueDataSet.KeyValueDataTable keyValueTable)
        {
            SortedList<TKey, TValue> sortedList = new SortedList<TKey, TValue>();
            if (keyValueTable.IsNull())
                return sortedList;

            foreach (KeyValueDataSet.KeyValueRow keyValueRow in keyValueTable)
            {
                sortedList.Add(keyValueRow.Key.Cast<TKey>(), keyValueRow.Value.Cast<TValue>());
            }

            return sortedList;
        }

        public static KeyValueDataSet.KeyValueDataTable SortedListToKeyValueTable<TKey, TValue>(this SortedList<TKey, TValue> sortedList)
        {
            KeyValueDataSet.KeyValueDataTable keyValueTable = new KeyValueDataSet.KeyValueDataTable();
            if (sortedList == null)
                return keyValueTable;

            for (int i = 0; i < sortedList.Count; i++)
            {
                keyValueTable.AddKeyValueRow(sortedList.Keys[i], sortedList.Values[i]);
            }
            return keyValueTable;
        }

        public static KeyValueDataSet.KeyValueDataTable KeyValuePairsToKeyValueTable<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            KeyValueDataSet.KeyValueDataTable keyValueTable = new KeyValueDataSet.KeyValueDataTable();
            if (keyValuePairs.IsNull())
                return keyValueTable;

            foreach (KeyValuePair<TKey, TValue> keyValuePair in keyValuePairs)
            {
                keyValueTable.AddKeyValueRow(keyValuePair.Key, keyValuePair.Value);
            }
            keyValueTable.AcceptChanges();

            return keyValueTable;
        }

        public static KeyValuePair<TKey, TValue>[] KeyValueTableToKeyValuePairs<TKey, TValue>(this KeyValueDataSet.KeyValueDataTable keyValueTable)
        {
            if (keyValueTable.IsNull())
                return new KeyValuePair<TKey, TValue>[0];

            KeyValuePair<TKey, TValue>[] keyValuePairs = new KeyValuePair<TKey, TValue>[keyValueTable.Count];
            for (int i = 0; i < keyValueTable.Count; i++)
            {
                keyValuePairs[i] = new KeyValuePair<TKey, TValue>(keyValueTable[i].Key.Cast<TKey>(), keyValueTable[i].Value.Cast<TValue>());
            }
            return keyValuePairs;
        }

        public static SortedList<TKey, TValue> KeyValuePairsToSortedList<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            SortedList<TKey, TValue> sortedList = new SortedList<TKey, TValue>();
            if (keyValuePairs.IsNull())
                return sortedList;

            foreach (KeyValuePair<TKey, TValue> keyValuePair in keyValuePairs)
            {
                sortedList.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return sortedList;
        }

        public static KeyValuePair<TKey, TValue>[] SortedListToKeyValuePairs<TKey, TValue>(this SortedList<TKey, TValue> sortedList)
        {
            if (sortedList.IsNull())
                return new KeyValuePair<TKey, TValue>[0];

            KeyValuePair<TKey, TValue>[] keyValuePairs = new KeyValuePair<TKey, TValue>[sortedList.Count];
            for (int i = 0; i < sortedList.Count; i++)
            {
                keyValuePairs[i] = new KeyValuePair<TKey, TValue>(sortedList.Keys[i], sortedList.Values[i]);
            }
            return keyValuePairs;
        }

        public static object[] KeyValuePairListToObjectList<TKey, TValue>(this KeyValuePair<TKey, TValue>[] keyValuePairList)
        {
            if (keyValuePairList.IsNull())
                return null;

            object[] array = new object[keyValuePairList.Length * 2];
            for (int i = 0; i < keyValuePairList.Length; i++)
            {
                array[i * 2] = keyValuePairList[i].Key;
                array[i * 2 + 1] = keyValuePairList[i].Value;
            }
            return array;
        }

        public static object[] SortedListToObjectList<TKey, TValue>(this SortedList<TKey, TValue> sortedList)
        {
            if (sortedList.IsNull())
                return null;

            object[] array = new object[sortedList.Count * 2];
            for (int i = 0; i < sortedList.Count; i++)
            {
                array[i * 2] = sortedList.Keys[i];
                array[i * 2 + 1] = sortedList.Values[i];
            }
            return array;
        }

        public static SortedList<TKey, TValue> ObjectListToSortedList<TKey, TValue>(this IEnumerable<object> objectList)
        {
            SortedList<TKey, TValue> sortedList = new SortedList<TKey, TValue>();
            if (objectList == null)
                return sortedList;

            int index = 1;
            TKey key = default(TKey);
            TValue value = default(TValue);
            foreach (object element in objectList)
            {
                if (index == 1)
                {
                    // 第一个
                    key = (TKey)element;
                    index = 2;
                }
                else
                {
                    value = (TValue)element;
                    index = 1;
                    sortedList.Add(key, value);
                }
            }
            return sortedList;
        }

        public static KeyValuePair<TKey, TValue>[] ObjectListToKeyValuePairList<TKey, TValue>(this IEnumerable<object> objectList)
        {
            List<KeyValuePair<TKey, TValue>> keyValuePairList = new List<KeyValuePair<TKey, TValue>>();
            if (objectList == null)
                return keyValuePairList.ToArray();

            int index = 1;
            TKey key = default(TKey);
            TValue value = default(TValue);
            foreach (object element in objectList)
            {
                if (index == 1)
                {
                    // 第一个
                    key = (TKey)element;
                    index = 2;
                }
                else
                {
                    value = (TValue)element;
                    index = 1;
                    keyValuePairList.Add(new KeyValuePair<TKey, TValue>(key, value));
                }
            }
            return keyValuePairList.ToArray();
        }

        #endregion

        #region Binary <-> Image
        /// <summary>
        /// 图片转换成字节数组
        /// </summary>
        //public static byte[] ImageToBinary(this System.Drawing.Image image,
        //    System.Drawing.Imaging.ImageFormat imageFormat)
        //{
        //    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        //    image.Save(memoryStream, imageFormat);
        //    return memoryStream.GetBuffer();
        //}

        /// <summary>
        /// Binary转换成图片
        /// </summary>
        //public static System.Drawing.Image BinaryToImage(this byte[] byteArray)
        //{
        //    if (byteArray.IsNullOrEmpty())
        //        return null;

        //    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(byteArray);
        //    return System.Drawing.Image.FromStream(memoryStream);
        //}
        #endregion Image

        #region Object <-> XElement
        public static XElement ObjectToXElement(this object dataObject)
        {
            System.IO.MemoryStream memoryStream = new MemoryStream();
            // 将对象写入Stream
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream);
            DataContractSerializer serializer = new DataContractSerializer(dataObject.GetType());
            serializer.WriteObject(xmlWriter, dataObject);

            // 将对象读入XElement
            memoryStream.Seek(0, SeekOrigin.Begin);
            XmlReader xmlReader = XmlReader.Create(memoryStream);
            return XElement.Load(xmlReader);
        }

        public static object XElementToObject(this XElement element,
            Type objectType)
        {
            System.IO.MemoryStream memoryStream = new MemoryStream();
            // 将Element写入Stream
            XmlWriter xmlWriter = XmlWriter.Create(memoryStream);
            element.Save(xmlWriter);

            // 将对象读出来
            memoryStream.Seek(0, SeekOrigin.Begin);
            XmlReader xmlReader = XmlReader.Create(memoryStream);
            DataContractSerializer serializer = new DataContractSerializer(objectType);
            return serializer.ReadObject(xmlReader);
        }

        public static T XElementToObject<T>(this XElement element)
        {
            return (T)XElementToObject(element, typeof(T));
        }
        #endregion

        #region String <-> Binary

        public static byte[] StringToBinary(this string binaryString)
        {
            byte[] array = Encoding.Unicode.GetBytes(binaryString);

            return array;
        }

        public static string BinaryToString(this byte[] binary)
        {
            string binaryString = Encoding.Unicode.GetString(binary);
            return binaryString;
        }

        #endregion

        #region json

        public static object ToJson(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject(Json);
        }
        public static string ToJson(this object obj)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        public static string ToJson(this object obj, string datetimeformats)
        {
            var timeConverter = new IsoDateTimeConverter { DateTimeFormat = datetimeformats };
            return JsonConvert.SerializeObject(obj, timeConverter);
        }
        public static T ToObject<T>(this string Json)
        {
            return Json == null ? default(T) : JsonConvert.DeserializeObject<T>(Json);
        }
        public static List<T> ToList<T>(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<List<T>>(Json);
        }
        public static DataTable ToTable(this string Json)
        {
            return Json == null ? null : JsonConvert.DeserializeObject<DataTable>(Json);
        }
        public static JObject ToJObject(this string Json)
        {
            return Json == null ? JObject.Parse("{}") : JObject.Parse(Json.Replace("&nbsp;", ""));
        }
        #endregion
    }
}
