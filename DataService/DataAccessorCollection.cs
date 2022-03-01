using DataPublic;
using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public class DataAccessorCollection
    {
        private SortedList<string, object> dataAccessor = new SortedList<string, object>();

        /// <summary>
        /// 索引器訪問
        /// </summary>
        /// <param name="dbname"></param>
        /// <returns></returns>
        public DataAccessor this[string dbname]
        {
            get
            {
                if (!dataAccessor.ContainsKey(dbname))
                    return null;
                return dataAccessor[dbname].Cast<DataAccessor>();
            }
        }


        /// <summary>
        /// 初始化所有數據庫連接
        /// </summary>
        /// <param name="dBAdapter"></param>
        /// <param name="connectionString"></param>
        public void Initialize(IDBAdapter dbAdapter, string dbName, string dbType, string connectionString)
        {
            if (!dataAccessor.ContainsKey(dbName))
            {
                if (dbType == GlobalVariables.ORACLE_SERVER)
                {
                    dataAccessor.Add(dbName, new OracleDataAcessor(dbAdapter, connectionString));
                }
                if (dbType == GlobalVariables.SQL_SERVER)
                {
                    dataAccessor.Add(dbName, new SQLDataAcessor(dbAdapter, connectionString));
                }
                if (dbType == GlobalVariables.MYSQL_SERVER)
                {
                    dataAccessor.Add(dbName, new MySQLDataAcessor(dbAdapter, connectionString));
                }
            }
        }

    }
}
