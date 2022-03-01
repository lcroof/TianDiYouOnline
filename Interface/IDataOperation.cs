using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NameValuePair = System.Collections.Generic.KeyValuePair<string, object>;

namespace Interface
{
    public interface IDataOperation
    {
        /// <summary>
        /// 根據SQL及參數獲取DataSet數據結構
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        System.Data.DataSet ExecuteGetDataSet(string commandString, KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 根據SQL及參數獲取DataSet數據格式結構
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        System.Data.DataSet ExecuteGetDataSetSchema(string commandString, KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 根據SQL及參數執行數據庫操作不返回命令
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        void ExecuteNoQuery(string commandString, KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 根據SQL及參數執行後台存儲過程命令
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        SortedList<string, object> ExecuteProcedure(string procedureName, KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 自定義存儲過程參數
        /// 根據SQL及參數執行後台存儲過程命令
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        SortedList<string, object> ExecuteProcedure(string procedureName, DbParameter[] commandParameters);

        /// <summary>
        /// 根據SQL及參數返回單個值命令
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object ExecuteScalar(string commandString, params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// 獲取服務器時間
        /// </summary>
        /// <returns></returns>
        string GetServerDateCommand();

        /// <summary>
        /// 更新DataSet中指定表名的數據
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="tableName"></param>
        void UpdateDataSet(System.Data.DataSet dataSet, string tableName);

        /// <summary>
        /// 更新DataSet中根據SQL查詢的數據
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="commandString"></param>
        /// <param name="paramValues"></param>
        void UpdateDataSet(System.Data.DataSet dataSet, string commandString, System.Collections.Generic.KeyValuePair<string, object>[] paramValues);

        /// <summary>
        /// 獲取服務器名
        /// </summary>
        /// <returns></returns>
        string GetDataBaseServerName();

        /// <summary>
        /// 連接字符
        /// </summary>
        string ConnectionString { get; set; }

        DbCommand CreateCommandBySQLString(bool removeNullParameter, string commandString, NameValuePair[] paramValues);

    }
}
