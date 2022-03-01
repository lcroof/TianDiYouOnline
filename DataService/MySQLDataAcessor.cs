using DataPublic;
using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public class MySQLDataAcessor : DataAccessor
    {
        public MySQLDataAcessor(IDBAdapter adapter, string connectCommand) : base(adapter, connectCommand)
        {
            DataAccessObject = new MySQLDataOperation(adapter, connectCommand, GlobalVariables.MYSQL_SERVER);
        }
    }
}
