using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public class DBTransactionScope : IDisposable
    {
        private int hasCode;
        private string connectionString;

        /// <summary>
        /// 是滞为最高层事务
        /// </summary>
        private bool isFirstScope = true;

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
            set
            {
                this.connectionString = value;
            }
        }

        /// <summary>
        /// 区别线程库字符
        /// </summary>
        private string identifyString
        {
            get
            {
                return this.hasCode.ToString() + this.connectionString;
            }
        }

        public DBTransactionScope(string connectCommand, string dbName)
        {
            this.hasCode = System.Threading.Thread.CurrentThread.GetHashCode();
            this.ConnectionString = connectCommand;
            lock (DBTransaction.Current)
            {
                if (DBTransaction.Current.Count > 0)
                {
                    int i = DBTransaction.Current.Count;
                    while (i > 0)
                    {
                        i = i - 1;
                        if (DBTransaction.Current.Count <= i)
                        {
                            //如果事务在循环过程中被释放就会小于函数值
                            i = DBTransaction.Current.Count;
                            continue;
                        }

                        DBTransaction transaction = null;
                        try
                        {
                            transaction = DBTransaction.Current[i];
                            if (transaction == null)
                            {
                                //取到的事务为空直接跳过取下一个
                                continue;
                            }
                        }
                        catch
                        {
                            //防溢出重新取值
                            i = DBTransaction.Current.Count;
                            continue;
                        }

                        if (transaction != null && transaction.DisposedFlag == false && this.identifyString == transaction.IdentifyString)
                        {
                            isFirstScope = false;
                            break;
                        }
                    }
                    if (isFirstScope)
                    {
                        //最高层先锁在最外面
                        DBTransaction.Current.Add(new DBTransaction(this.hasCode, connectCommand, dbName));
                    }
                }
                else
                {
                    //当前没有连接的时候视为主连接
                    DBTransaction.Current.Add(new DBTransaction(this.hasCode, connectCommand, dbName));
                }
            }
        }

        /// <summary>
        /// 提交
        /// </summary>
        public void Commit()
        {
            if (!isFirstScope)
            {
                return;
            }

            lock (DBTransaction.Current)
            {
                int i = DBTransaction.Current.Count;
                while (i > 0)
                {
                    i = i - 1;
                    if (DBTransaction.Current.Count <= i)
                    {
                        i = DBTransaction.Current.Count;
                        continue;
                    }
                    DBTransaction transaction = null;
                    try
                    {
                        transaction = DBTransaction.Current[i];
                    }
                    catch
                    {
                        i = DBTransaction.Current.Count;
                        continue;
                    }
                    if (transaction != null && transaction.DisposedFlag == false && this.identifyString == transaction.IdentifyString)
                    {
                        transaction.Commit();
                    }
                }
                this.Dispose();
            }
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public void Rollback()
        {
            if (!isFirstScope)
            {
                return;
            }

            lock (DBTransaction.Current)
            {
                int i = DBTransaction.Current.Count;
                while (i > 0)
                {
                    i = i - 1;
                    if (DBTransaction.Current.Count <= i)
                    {
                        i = DBTransaction.Current.Count;
                        continue;
                    }
                    DBTransaction transaction = null;
                    try
                    {
                        transaction = DBTransaction.Current[i];
                    }
                    catch
                    {
                        i = DBTransaction.Current.Count;
                        continue;
                    }
                    if (transaction != null && transaction.DisposedFlag == false && this.identifyString == transaction.IdentifyString)
                    {
                        transaction.Rollback();
                    }
                }
                this.Dispose();
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (!isFirstScope)
            {
                return;
            }

            lock (DBTransaction.Current)
            {
                int i = DBTransaction.Current.Count;
                while (i > 0)
                {
                    i = i - 1;
                    if (DBTransaction.Current.Count <= i)
                    {
                        i = DBTransaction.Current.Count;
                        continue;
                    }
                    DBTransaction transaction = null;
                    try
                    {
                        transaction = DBTransaction.Current[i];
                    }
                    catch
                    {
                        i = DBTransaction.Current.Count;
                        continue;
                    }
                    if (transaction != null && this.identifyString == transaction.IdentifyString)
                    {
                        DBTransaction.Current.Remove(transaction);
                        transaction.Dispose();
                    }
                }
            }
        }
    }
}
