using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public class DBTransaction : DbTransaction
    {
        public DbTransaction Transaction = null;
        private string connectionString;
        private static List<DBTransaction> current;
        private bool disposedFlag;
        private int hashCode;

        /// <summary>
        /// 区别线程库字符
        /// </summary>
        public string IdentifyString
        {
            get
            {
                return hashCode.ToString() + connectionString;
            }
        }

        /// <summary>
        /// 数据库连接
        /// </summary>
        public new DbConnection Connection
        {
            get
            {
                if (Transaction == null)
                    return null;
                return Transaction.Connection;
            }
        }

        /// <summary>
        /// 数据库事务列表
        /// </summary>
        public static List<DBTransaction> Current
        {
            get
            {
                if (current == null)
                {
                    current = new List<DBTransaction>();
                }
                return current;
            }
        }

        /// <summary>
        /// 事务消除
        /// </summary>
        public bool DisposedFlag
        {
            get
            {
                return this.disposedFlag;
            }
            private set
            {
                this.disposedFlag = value;
            }
        }

        protected override DbConnection DbConnection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override IsolationLevel IsolationLevel
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 新建事务
        /// </summary>
        /// <param name="code"></param>
        /// <param name="command"></param>
        public DBTransaction(int code, string command, string dbName)
        {
            this.DisposedFlag = false;
            this.hashCode = code;
            this.connectionString = command;
            ConnectionPool connectPool = new ConnectionPool(command);
            connectPool.FactoryDBName = dbName;
            DbConnection conn = connectPool.GetConnection();
            ConnectionPool.OpenConnection(conn);
            Transaction = conn.BeginTransaction();
        }

        /// <summary>
        /// 提交
        /// </summary>
        public override void Commit()
        {
            DbConnection conn = this.Connection;

            if (conn == null)
            {
                throw new Exception("Connection can't be null");
            }
            try
            {
                Transaction.Commit();
            }
            catch
            {

            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public override void Rollback()
        {
            DbConnection conn = this.Connection;

            if (conn == null)
            {
                throw new Exception("Connection can't be null");
            }
            try
            {
                Transaction.Rollback();
            }
            catch
            {

            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 清除事务
        /// </summary>
        public new void Dispose()
        {
            this.DisposedFlag = true;
            try
            {
                if (this.Connection != null)
                {
                    this.Connection.Close();
                }
            }
            catch
            {

            }
            finally
            {
                if (Transaction != null)
                {
                    Transaction.Dispose();
                }
            }
        }
    }
}
