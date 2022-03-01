using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataPublic
{
    public static class GlobalVariables
    {
        public const string Ttl_GroupPanelText = @"請將列頭拖至此處分組";
        public const string Ttl_RecordCount = @"{0} / {1}";
        public const string Ttl_NewRow = @"點擊此處新增一行";

        public const string ENABLED = "ENABLED";
        public const string ThreadRunning = "Running";
        public const string ThreadStop = "Stop";

        public const string Yes = "Y";
        public const string No = "N";

        #region
        public const string ORACLE_SERVER = "Oracle Server";
        public const string SQL_SERVER = "SQL Server";
        public const string MYSQL_SERVER = "MySQL Server";
        public const string MEGV_ORACLE = "MEGV";
        public const string OHUB_ORACLE = "OHUB";
        public const string CAR_SQL = "CAR";
        #endregion

        #region tableColumnName
        public const string ID = "ID";
        public const string SHIPMENT_NUMBER = "SHIPMENT_NUMBER";
        public const string NAME = "NAME";
        public const string VESSEL_NUMBER = "VESSEL_NUMBER";
        public const string PROCESS_END_DATE = "PROCESS_END_DATE";
        public const string PROCESS_BEGIN_DATE = "PROCESS_BEGIN_DATE";
        public const string PROCESS_FLAG = "PROCESS_FLAG";
        public const string TYPE = "TYPE";
        public const string COMPANY_CODE = "COMPANY_CODE";
        public const string DELETED_FLAG = "DELETED_FLAG";
        public const string ATTRIBUTE1 = "ATTRIBUTE1";
        public const string SHIPMENT_NO = "SHIPMENT_NO";
        public const string SHIPPING_ID = "SHIPPING_ID";
        public const string RTV_LOT_NO = "RTV_LOT_NO";
        public const string PART_NUMBER = "PART_NUMBER";
        public const string COMPANY_ID = "COMPANY_ID";
        public const string STATUS = "STATUS";
        public const string BEGIN_DATE = "BEGIN_DATE";
        public const string END_DATE = "END_DATE";
        public const string IS_ACTIVE = "IS_ACTIVE";
        public const string MASTER_ID = "MASTER_ID";
        public const string TRACKING_ID = "TRACKING_ID";
        public const string TRACKING_DETAIL_ID = "TRACKING_DETAIL_ID";

        #endregion

        #region TNS PROD
        public const string MegvOracleTNS = @"Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(Host = 10.86.0.191)(Port = 1521)))(CONNECT_DATA = (SID = mslkq)));USER ID=megv;Password=meacgv;";
        public const string OhubOracleTNS = @"Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.86.0.25)(PORT = 1521))(CONNECT_DATA = (SID = OHUB)));USER ID=mslweb;Password=mslweb;";
        #endregion

        #region TNS TEST
        //public const string MegvOracleTNS = @"Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(Host = 10.86.3.41)(Port = 1528)))(CONNECT_DATA = (SID = mslkq)));USER ID=megv;Password=megvdev;";
        //public const string OhubOracleTNS = @"Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = 10.86.1.144)(PORT = 1531))(CONNECT_DATA = (SID = OHUB)));USER ID=mslweb;Password=mslweb;";
        #endregion 
    }
}
