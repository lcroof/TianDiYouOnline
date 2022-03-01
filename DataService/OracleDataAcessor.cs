using DataPublic;
using Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public class OracleDataAcessor : DataAccessor
    {
        public OracleDataAcessor(IDBAdapter adapter, string connectCommand) : base(adapter, connectCommand)
        {
            DataAccessObject = new OracleDataOperation(adapter, connectCommand, GlobalVariables.ORACLE_SERVER);
        }
    }
}
