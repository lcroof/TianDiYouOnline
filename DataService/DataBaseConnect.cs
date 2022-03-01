using DataPublic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataService
{
    public static class DataBaseConnect
    {
        private static DataAccessorCollection dataAccessor = new DataAccessorCollection();

        public static DataAccessorCollection GlobalDataAccessor
        {
            get
            {
                dataAccessor.CheckNull("");
                return dataAccessor;
            }
        }
    }
}
