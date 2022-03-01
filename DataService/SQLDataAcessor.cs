using DataPublic;
using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public class SQLDataAcessor : DataAccessor
    {
        public SQLDataAcessor(IDBAdapter adapter, string connectCommand) : base(adapter, connectCommand)
        {
            DataAccessObject = new SQLDataOperation(adapter, connectCommand, GlobalVariables.SQL_SERVER);
        }
    }
}
