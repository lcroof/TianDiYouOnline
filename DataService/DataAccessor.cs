using System;
using System.Collections.Generic;
using System.Data.Common;
using NameValuePair = System.Collections.Generic.KeyValuePair<string, object>;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using DataPublic;
using Interface;

namespace DataService
{
    public class DataAccessor
    {
        private IDataOperation dataAccessObject;
        private string connectionString;        

        public IDataOperation DataAccessObject
        {
            get
            {
                dataAccessObject.CheckNull("Data access object has not been initialized.");
                return dataAccessObject;
            }
            set
            {
                this.dataAccessObject = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="connectionstring"></param>
        public DataAccessor(IDBAdapter adapter, string connectCommand)
        {
            this.ConnectionString = connectCommand;
            //dataAccessObject = new DataOperation(adapter, connectCommand);
        }

        #region 執行命令
        /// <summary>
        /// 执行sql语句，无返回值，参数KeyValuePair
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        public void ExecuteNoQueryByKeyValuePairs(string commandString, string dbName, params NameValuePair[] parameters)
        {
            using (DBTransactionScope transScope = new DBTransactionScope(this.ConnectionString, dbName))
            {
                DataAccessObject.ExecuteNoQuery(commandString, parameters);
                transScope.Commit();
            }
        }
        /// <summary>
        /// 执行sql语句，无返回值，参数params object
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        public void ExecuteNoQuery(string commandString, string dbName, params object[] parameters)
        {
            ExecuteNoQueryByKeyValuePairs(commandString, dbName, parameters.ObjectListToKeyValuePairList<string, object>());
        }

        #endregion

        #region 获取数据表
        /// <summary>
        /// 执行SQL语句，返回DataSet对象，参数KeyValuePair
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet ExecuteGetDataSetByKeyValuePairs(string commandString,
            params NameValuePair[] parameters)
        {
            DataSet dataSet = DataAccessObject.ExecuteGetDataSet(commandString, parameters);
            return dataSet;
        }

        /// <summary>
        /// 执行SQL语句，返回DataSet对象，参数params object
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataSet ExecuteGetDataSetByKeyValuePairs(string commandString, params object[] parameters)
        {
            return ExecuteGetDataSetByKeyValuePairs(commandString,
                parameters.ObjectListToKeyValuePairList<string, object>());
        }

        /// <summary>
        /// 执行SQL语句，返回DataTable对象，参数KeyValuePair
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteGetDataByKeyValuePairs(string commandString,
            params NameValuePair[] parameters)
        {
            DataSet dataSet = ExecuteGetDataSetByKeyValuePairs(commandString, parameters);
            System.Diagnostics.Debug.Assert(dataSet != null);

            return dataSet.FirstTable();
        }
        /// <summary>
        /// 执行SQL语句，返回DataTable对象，参数params object
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public DataTable ExecuteGetData(string commandString, params object[] parameters)
        {
            return ExecuteGetDataByKeyValuePairs(commandString,
                parameters.ObjectListToKeyValuePairList<string, object>());
        }
        /// <summary>
        /// 执行SQL语句，返回泛型对象，约束继承了DataTable，参数KeyValuePair
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteGetDataByKeyValuePairs<T>(string commandString, params NameValuePair[] parameters)
            where T : DataTable, new()
        {
            T dataTable = new T();
            dataTable.Merge(ExecuteGetDataByKeyValuePairs(commandString, parameters));
            return dataTable;
        }

        /// <summary>
        /// 执行SQL语句，返回泛型对象，约束继承了DataTable，参数params object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteGetData<T>(string commandString, params object[] parameters)
            where T : DataTable, new()
        {
            T dataTable = new T();
            dataTable.Merge(ExecuteGetData(commandString, parameters));
            return dataTable;
        }

        /// <summary>
        /// 执行SQL语句，返回泛型对象，约束继承了DataSet，参数KeyValuePair
        /// 注意目標DataSet只能有一個表，如果多個表將無數轉化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteGetDataSetByKeyValuePairs<T>(string commandString,
            params NameValuePair[] parameters)
            where T : DataSet, new()
        {
            T dataSet = new T();
            dataSet.Merge(ExecuteGetDataSetByKeyValuePairs(commandString, parameters));
            return dataSet;
        }

        /// <summary>
        /// 执行SQL语句，返回泛型对象，约束继承了DataSet，参数param object
        /// 注意目標DataSet只能有一個表，如果多個表將無數轉化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteGetDataSet<T>(string commandString, params object[] parameters)
            where T : DataSet, new()
        {
            T dataSet = new T();
            dataSet.Merge(ExecuteGetDataSetByKeyValuePairs(commandString, parameters));
            return dataSet;
        }

        #endregion

        #region 执行过程
        /// <summary>
        /// 执行存储过程，返回SortedList对象，参数KeyValuePair
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SortedList<string, object> ExecuteProcedureByKeyValuePairs(string procedureName, string dbName,
            params NameValuePair[] parameters)
        {
            using (DBTransactionScope transScope = new DBTransactionScope(this.ConnectionString, dbName))
            {
                SortedList<string, object> resultList =
                    DataAccessObject.ExecuteProcedure(procedureName, parameters);
                transScope.Commit();
                return resultList;
            }
        }
        /// <summary>
        /// 执行存储过程，返回SortedList对象，参数SortedList
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="inParameters"></param>
        /// <returns></returns>
        public SortedList<string, object> ExecuteProcedure(string procedureName, string dbName,
            SortedList<string, object> inParameters)
        {
            return ExecuteProcedureByKeyValuePairs(procedureName, dbName,
                inParameters.SortedListToKeyValuePairs<string, object>());
        }
        /// <summary>
        /// 执行存储过程，返回SortedList对象，参数params object
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="inParameters"></param>
        /// <returns></returns>
        public SortedList<string, object> ExecuteProcedure(string procedureName, string dbName,
            params object[] inParameters)
        {
            return ExecuteProcedure(procedureName, dbName,
                inParameters.ObjectListToSortedList<string, object>());
        }

        /// <summary>
        /// 自定義參數，用於重載存儲過程使用
        /// 执行存储过程，返回SortedList对象，参数KeyValuePair
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SortedList<string, object> ExecuteProcedureByKeyValuePairs(string procedureName, string dbName,
            DbParameter[] commandParameters)
        {
            using (DBTransactionScope transScope = new DBTransactionScope(this.ConnectionString, dbName))
            {
                SortedList<string, object> resultList =
                    DataAccessObject.ExecuteProcedure(procedureName, commandParameters);
                transScope.Commit();
                return resultList;
            }
        }

        #endregion
    }
}
