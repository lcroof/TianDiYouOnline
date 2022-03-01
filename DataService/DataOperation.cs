using System;
using System.Collections.Generic;
using NameValuePair = System.Collections.Generic.KeyValuePair<string, object>;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using DataPublic;
using Interface;

namespace DataService
{
    public abstract class DataOperation : IDataOperation
    {
        private ConnectionPool connectionPool;
        private string connectionString;
        private IDBAdapter adapter;

        /// <summary>
        /// 表结构查询字符串
        /// </summary>
        const string CommandString_TableSchemaSelect = "SELECT * FROM ({0}) \"TableSchema\" WHERE 1=2";

        /// <summary>
        /// 表查询命令字符串
        /// </summary>
        /// 
        const string CommandString_TableSelect = "SELECT * FROM {0}";

        public IDBAdapter Adapter
        {
            get
            {
                adapter.CheckNull("Data adapter is not initialized.");
                return adapter;
            }
        }

        /// <summary>
        /// 連接池
        /// </summary>
        public ConnectionPool ConnectionPool
        {
            get
            {
                return connectionPool;
            }
            set
            {
                connectionPool = value;
            }
        }

        /// <summary>
        /// 連接字符
        /// </summary>
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
        /// 當前的事務
        /// </summary>
        public DBTransaction currentTransaction
        {
            get
            {
                lock (this)
                {
                    for (int i = 0; i < DBTransaction.Current.Count; i++)
                    {
                        if (DBTransaction.Current[i].IdentifyString ==
                            System.Threading.Thread.CurrentThread.GetHashCode().ToString() + this.ConnectionString)
                        {
                            return DBTransaction.Current[i];
                        }
                    }
                }

                return null;
            }
        }

        public DataOperation(IDBAdapter adapter, string connectCommand, string dbType)
            : base()
        {
            Initialize(adapter, connectCommand, dbType);
        }

        /// <summary>
        /// 初始化adapter
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="connectionString"></param>
        public void Initialize(IDBAdapter adapter, string connectCommand, string dbType)
        {
            this.adapter = adapter;
            this.ConnectionPool = new ConnectionPool(connectCommand);
            this.ConnectionString = connectCommand;
            this.ConnectionPool.FactoryDBName = dbType;
        }

        /// <summary>
        /// 初始化Command
        /// </summary>
        /// <param name="command"></param>
        public void InitCommand(DbCommand command)
        {
            if (currentTransaction == null)
            {
                command.Connection = ConnectionPool.GetConnection();
                ConnectionPool.OpenConnection(command.Connection);
            }
            else
            {
                command.Connection = currentTransaction.Connection;
                command.Transaction = currentTransaction.Transaction;
            }
        }

        /// <summary>
        /// 根據SQL創建DataAdapter
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public DbDataAdapter CreateDataAdapterBySQLString(string commandString, NameValuePair[] paramValues)
        {
            return CreateDataAdapterBySQLString(true, commandString, paramValues);
        }

        /// <summary>
        /// 根據SQL創建DataAdapter
        /// </summary>
        /// <param name="removeNullParameter"></param>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public DbDataAdapter CreateDataAdapterBySQLString(bool removeNullParameter, string commandString, NameValuePair[] paramValues)
        {
            DbDataAdapter dataAdapter = Adapter.CreateDBAdapter();
            dataAdapter.SelectCommand = CreateCommandBySQLString(removeNullParameter, commandString, paramValues);
            return dataAdapter;
        }

        /// <summary>
        /// 根據SQL創建Command
        /// </summary>
        /// <param name="removeNullParameter"></param>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public virtual DbCommand CreateCommandBySQLString(bool removeNullParameter, string commandString, NameValuePair[] paramValues)
        {
            // 創建Command
            DbCommand command = null;
            return command;
        }

        /// <summary>
        /// 根据SQL创建Command，默认去除空参数
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public DbCommand CreateCommandBySQLString(string commandString, NameValuePair[] paramValues)
        {
            return CreateCommandBySQLString(true, commandString, paramValues);
        }

        /// <summary>
        /// 根據存儲過程創建Command
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        public virtual DbCommand CreateCommandByProcedureName(string procedureName,
           NameValuePair[] inputParameters)
        {
            //創建Command
            DbCommand command = null;
            return command;
        }

        /// <summary>
        /// 自定義存儲過程參數
        /// 根據存儲過程創建Command
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        public virtual DbCommand CreateCommandByProcedureName(string procedureName, DbParameter[] commandParameters)
        {
            //創建Command
            DbCommand command = null;
            return command;
        }

        /// <summary>
        /// 根據SQL創建FullDataAdapter
        /// </summary>
        /// <param name="removeNullParameter"></param>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public DbDataAdapter CreateFullDataAdapterBySQLString(bool removeNullParameter, string commandString, NameValuePair[] paramValues)
        {
            DbDataAdapter adapter = CreateDataAdapterBySQLString(removeNullParameter, commandString, paramValues);
            Adapter.BuildCommands(adapter);
            if (!adapter.SelectCommand.IsNull())
                adapter.SelectCommand.Transaction = currentTransaction.Transaction;
            if (!adapter.UpdateCommand.IsNull())
                adapter.UpdateCommand.Transaction = currentTransaction.Transaction;
            if (!adapter.InsertCommand.IsNull())
                adapter.InsertCommand.Transaction = currentTransaction.Transaction;
            if (!adapter.DeleteCommand.IsNull())
                adapter.DeleteCommand.Transaction = currentTransaction.Transaction;
            return adapter;
        }

        /// <summary>
        /// 根據SQL創建FullDataAdapter
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public DbDataAdapter CreateFullDataAdapterBySQLString(string commandString, NameValuePair[] paramValues)
        {
            return CreateFullDataAdapterBySQLString(true, commandString, paramValues);
        }

        // <summary>
        /// 根據TableName創建FullDataAdapter
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DbDataAdapter CreateFullDataAdapterByTableName(string tableName)
        {
            return CreateFullDataAdapterBySQLString(String.Format(CommandString_TableSelect, tableName), new NameValuePair[0]);
        }

        /// <summary>
        /// 根据SQL和参数执行命令
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        public void ExecuteNoQuery(string commandString, KeyValuePair<string, object>[] parameters)
        {
            DbCommand command = CreateCommandBySQLString(commandString, parameters);
            try
            {
                if (command.Connection.State == ConnectionState.Closed)
                    ConnectionPool.OpenConnection((DbConnection)command.Connection);

                command.ExecuteNonQuery();
            }
            finally
            {
                if (currentTransaction == null)
                    command.Connection.Close();
            }
        }

        /// <summary>
        /// 根據SQL及參數執行後台存儲過程命令
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SortedList<string, object> ExecuteProcedure(string procedureName, KeyValuePair<string, object>[] parameters)
        {
            DbCommand command = CreateCommandByProcedureName(procedureName, parameters);
            try
            {
                if (command.Connection.State == ConnectionState.Closed)
                    ConnectionPool.OpenConnection((DbConnection)command.Connection);

                command.ExecuteNonQuery();
            }
            finally
            {
                if (currentTransaction == null)
                    command.Connection.Close();
            }
            SortedList<string, object> outputParameterList = new SortedList<string, object>();
            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Direction == System.Data.ParameterDirection.InputOutput ||
                    parameter.Direction == System.Data.ParameterDirection.Output ||
                    parameter.Direction == System.Data.ParameterDirection.ReturnValue)
                {
                    outputParameterList.Add(parameter.ParameterName, parameter.Value);
                }
            }
            return outputParameterList;
        }

        // <summary>
        /// 自定義存儲過程的參數
        /// 根據SQL及參數執行後台存儲過程命令
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public SortedList<string, object> ExecuteProcedure(string procedureName, DbParameter[] commandParameters)
        {
            DbCommand command = CreateCommandByProcedureName(procedureName, commandParameters);
            try
            {
                if (command.Connection.State == ConnectionState.Closed)
                    ConnectionPool.OpenConnection((DbConnection)command.Connection);

                command.ExecuteNonQuery();
            }
            finally
            {
                if (currentTransaction == null)
                    command.Connection.Close();
            }

            SortedList<string, object> outputParameterList = new SortedList<string, object>();

            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Direction == System.Data.ParameterDirection.InputOutput ||
                    parameter.Direction == System.Data.ParameterDirection.Output ||
                    parameter.Direction == System.Data.ParameterDirection.ReturnValue)
                {
                    outputParameterList.Add(parameter.ParameterName, parameter.Value);
                }
            }
            return outputParameterList;
        }

        /// <summary>
        /// 根據SQL及參數獲取DataSet數據結構
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public System.Data.DataSet ExecuteGetDataSet(string commandString, KeyValuePair<string, object>[] parameters)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = dataSet.Tables.Add();
            DbDataAdapter adapter = CreateDataAdapterBySQLString(commandString, parameters);
            //dataSet.EnforceConstraints = false; 用於無限制數據結構之間dataSet或dataTable的Merge，加快運行速度
            try
            {
                adapter.Fill(dataTable);
            }
            finally
            {
                if (currentTransaction == null &&
                    adapter.SelectCommand.Connection.State != ConnectionState.Closed)
                    adapter.SelectCommand.Connection.Close();
            }
            return dataSet;
        }

        /// <summary>
        /// 根據SQL及參數獲取DataSet數據格式結構
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public System.Data.DataSet ExecuteGetDataSetSchema(string commandString, KeyValuePair<string, object>[] parameters)
        {
            DataSet dataSet = new DataSet();
            DataTable dataTable = dataSet.Tables.Add();
            DbDataAdapter adapter = CreateDataAdapterBySQLString(
                false,
                String.Format(CommandString_TableSchemaSelect, commandString),
                parameters);
            try
            {
                adapter.Fill(dataTable);
            }
            finally
            {
                if (currentTransaction == null && adapter.SelectCommand.Connection.State != ConnectionState.Closed)
                    adapter.SelectCommand.Connection.Close();
            }
            return dataSet;
        }

        public object ExecuteScalar(string commandString, params NameValuePair[] parameters)
        {
            DbCommand command = CreateCommandBySQLString(commandString, parameters);
            try
            {
                if (command.Connection.State == ConnectionState.Closed)
                    ConnectionPool.OpenConnection((DbConnection)command.Connection);
                return command.ExecuteScalar();
            }
            finally
            {
                if (currentTransaction == null)
                    command.Connection.Close();
            }
        }

        public string GetServerDateCommand()
        {
            return Adapter.ServerDateCommand;
        }

        public void UpdateDataSet(DataSet dataSet, string tableName)
        {
            dataSet.CheckNull("Null dataset cannot be updated.");

            DataTable dataTable = dataSet.FirstTable();
            if (tableName == null)
            {
                tableName = dataTable.TableName;
            }

            DbDataAdapter adapter = CreateFullDataAdapterByTableName(tableName);
            try
            {
                adapter.Update(dataTable);
            }
            finally
            {
                if (currentTransaction == null && adapter.SelectCommand.Connection.State != ConnectionState.Closed)
                    adapter.SelectCommand.Connection.Close();
            }
        }

        public void UpdateDataSet(DataSet dataSet, string commandString, NameValuePair[] paramValues)
        {
            dataSet.CheckNull("Null dataset cannot be updated.");

            DataTable dataTable = dataSet.FirstTable();
            if (commandString == null)
            {
                string tableName = dataTable.TableName;
                UpdateDataSet(dataSet, tableName);
                return;
            }
            DbDataAdapter adapter = CreateFullDataAdapterBySQLString(commandString, paramValues);
            try
            {
                adapter.Update(dataTable);
            }
            finally
            {
                if (currentTransaction == null &&
                    adapter.SelectCommand.Connection.State != ConnectionState.Closed)
                    adapter.SelectCommand.Connection.Close();
            }
        }

        public string GetDataBaseServerName()
        {
            DbConnection connection = this.ConnectionPool.GetConnection();
            return connection.DataSource.ToString();
        }
    }
}
