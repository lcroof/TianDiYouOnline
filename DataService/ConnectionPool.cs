using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public class ConnectionPool
    {
        private string connectString;
        private bool connectionPoolEnabled;
        private DbProviderFactory dbProviderFactory;
        private string factoryDBName;

        public string FactoryDBName
        {
            get
            {
                return this.factoryDBName;
            }
            set
            {
                this.factoryDBName = value;
            }
        }

        public DbProviderFactory DBProviderFactory
        {
            get
            {
                if (dbProviderFactory == null)
                {
                    if (this.FactoryDBName == "Oracle Server")
                    {
                        dbProviderFactory = DbProviderFactories.GetFactory(new OracleConnection());
                    }
                    else if (this.FactoryDBName == "SQL Server")
                    {
                        dbProviderFactory = DbProviderFactories.GetFactory(new SqlConnection());
                    }
                    else if (this.FactoryDBName == "MySQL Server")
                    {
                        dbProviderFactory = DbProviderFactories.GetFactory(new MySqlConnection());
                    }
                }
                return this.dbProviderFactory;
            }
        }

        List<DbConnection> connectionList = new List<DbConnection>();

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectString
        {
            get
            {
                return this.connectString;
            }
            set
            {
                this.connectString = value;
            }
        }

        /// <summary>
        /// 连接池列表
        /// </summary>
        public List<DbConnection> ConnectionList
        {
            get
            {
                return this.connectionList;
            }
        }

        /// <summary>
        /// 连接池开关
        /// </summary>
        public bool ConnectionPoolEnabled
        {
            get
            {
                return connectionPoolEnabled;
            }
            set
            {
                connectionPoolEnabled = value;
            }
        }

        public ConnectionPool()
        {

        }

        public ConnectionPool(string connectCommand)
        {
            this.ConnectString = connectCommand;
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        /// <param name="conn"></param>
        public static void OpenConnection(DbConnection conn)
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }

        /// <summary>
        /// 获取连接
        /// 如果存在空位就放进连接池
        /// </summary>
        public DbConnection GetConnection()
        {
            DbConnection connection = null;
            if (connectionPoolEnabled)
            {
                foreach (OracleConnection conn in connectionList)
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        connection = conn;
                    }
                }

                if (connection == null)
                {
                    connection = DBProviderFactory.CreateConnection();
                    connectionList.Add(connection);
                }
            }
            else
            {
                connection = DBProviderFactory.CreateConnection();
                connection.ConnectionString = this.connectString;
            }
            return connection;
        }
    }
}
