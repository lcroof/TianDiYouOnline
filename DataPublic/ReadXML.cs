using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace DataPublic
{
    public static class ReadXML
    {
        public static List<DataBaseModel> XMLReader()
        {
            List<DataBaseModel> arrayList = new List<DataBaseModel>();
            XmlDocument doc = new XmlDocument();
            doc.Load(AppDomain.CurrentDomain.BaseDirectory + "\\Config\\DataConnentions.xml");
            foreach (XmlNode child in doc.DocumentElement)
            {
                DataBaseModel model = new DataBaseModel();
                model.DATABASE_NAME = child["DATABASE_NAME"].InnerText;
                model.DATABASE_TYPE = child["DATABASE_TYPE"].InnerText;
                model.DATABASE_STRING = child["DATABASE_STRING"].InnerText;
                model.PROD = child["PROD"].InnerText;
                arrayList.Add(model);
            }
            return arrayList;
        }
    }
}
