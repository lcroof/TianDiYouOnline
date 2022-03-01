using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Interface
{
    public interface IDBAdapter
    {
        /// <summary>
        /// 創建一個適配器
        /// </summary>
        /// <returns></returns>
        DbDataAdapter CreateDBAdapter();

        /// <summary>
        /// 給Adapter設定更新衝突依據
        /// </summary>
        /// <param name="adapter"></param>
        void BuildCommands(DbDataAdapter adapter);

        /// <summary>
        /// 取得參數
        /// </summary>
        /// <param name="removeNullParameter"></param>
        /// <param name="command"></param>
        /// <param name="paramValues"></param>
        void DeriveParameters(bool removeNullParameter, DbCommand command, SortedList<string, object> paramValues);

        /// <summary>
        /// 取得存儲過程參數
        /// </summary>
        /// <param name="command"></param>
        void DeriveProcedureParameters(DbCommand command);

        /// <summary>
        /// 取服務器時間
        /// </summary>
        string ServerDateCommand { get; }
    }
}
