using System;
using System.Collections.Generic;
using NameValuePair = System.Collections.Generic.KeyValuePair<string, object>;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using DataPublic;
using System.Data.SqlClient;
using Interface;

namespace DataService
{
    public class SQLDataOperation : DataOperation, IDataOperation
    {
        public SQLDataOperation(IDBAdapter adapter, string connectCommand, string dbType) : base(adapter, connectCommand, dbType)
        {
            Initialize(adapter, connectCommand, GlobalVariables.SQL_SERVER);
        }

        /// <summary>
        /// 根據SQL創建Command
        /// </summary>
        /// <param name="removeNullParameter"></param>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public override DbCommand CreateCommandBySQLString(bool removeNullParameter, string commandString, NameValuePair[] paramValues)
        {
            // 創建Command
            SqlCommand command = new SqlCommand(commandString);
            InitCommand(command);

            // 生成參數
            Adapter.DeriveParameters(removeNullParameter, command, paramValues.KeyValuePairsToSortedList<string, object>());

            return command;
        }

        /// <summary>
        /// 根據存儲過程創建Command
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        public override DbCommand CreateCommandByProcedureName(string procedureName,
           NameValuePair[] inputParameters)
        {
            //創建Command
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedureName;

            InitCommand(command);

            if (command.Connection.State == ConnectionState.Closed)
            {
                ConnectionPool.OpenConnection(command.Connection);
            }

            //生成參數
            Adapter.DeriveProcedureParameters(command);

            //设定参数值
            if (inputParameters != null)
            {
                SortedList<string, object> inParameterList = inputParameters.KeyValuePairsToSortedList<string, object>();
                foreach (DbParameter parameter in command.Parameters)
                {
                    if (parameter.Direction == System.Data.ParameterDirection.Input ||
                        parameter.Direction == System.Data.ParameterDirection.InputOutput)
                    {
                        if (inParameterList.ContainsKey(parameter.ParameterName))
                        {
                            object parameterValue = inParameterList[parameter.ParameterName];
                            command.Parameters[parameter.ParameterName].Value =
                                parameterValue.IsNull() ? Convert.DBNull : parameterValue;
                        }
                        else
                        {
                            command.Parameters[parameter.ParameterName].Value = Convert.DBNull;
                        }
                    }
                }
            }
            return command;
        }

        /// <summary>
        /// 自定義存儲過程參數
        /// 根據存儲過程創建Command
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="inputParameters"></param>
        /// <returns></returns>
        public override DbCommand CreateCommandByProcedureName(string procedureName, DbParameter[] commandParameters)
        {
            //創建Command
            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedureName;

            InitCommand(command);

            if (command.Connection.State == ConnectionState.Closed)
            {
                ConnectionPool.OpenConnection(command.Connection);
            }

            if (commandParameters != null)
            {
                command.Parameters.AddRange(commandParameters);
            }

            return command;
        }
    }
}
