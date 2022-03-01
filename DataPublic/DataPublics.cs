using Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace DataPublic
{
    public static class DataPublics
    {
        public static string[] DellMonth = new string[12]
                { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C" };

        public static string[] DellDay = new string[31]{
                  "1","2","3","4","5","6","7","8","9","A",
                  "B","C","D","E","F","G","H","I","J","K",
                  "L","M","N","O","P","Q","R","S","T","U", "V"};

        public static string[] DayNoIOQU = new string[31]{
                  "1","2","3","4","5","6","7","8","9","A",
                  "B","C","D","E","F","G","H","J","K","L",
                  "M","N","P","R","S","T","V","W","X","Y","Z"};

        public const decimal DBTrue = 1m;
        public const decimal DBFalse = 0m;

        public const string DBTRUE = "Y";
        public const string DBFALSE = "N";

        #region 基础

        public static bool IsDBString(this DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDBNumber(this DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Byte:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.SByte:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.VarNumeric:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDBDateTime(this DbType dbType)
        {
            switch (dbType)
            {
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Time:
                    return true;
                default:
                    return false;
            }
        }

        public static Type DBTypeToType(this DbType dbType)
        {
            if (dbType.IsDBDateTime())
                return typeof(DateTime);
            else if (dbType.IsDBNumber())
                return typeof(decimal);
            else
                return typeof(string);
        }

        public static void DataRowCopyTo(this System.Data.DataRow dataRow
            , System.Data.DataRow destDataRow)
        {
            destDataRow.CheckNull("Dest data row can not be null.");

            foreach (System.Data.DataColumn dataColumn in dataRow.Table.Columns)
            {
                if (destDataRow.Table.Columns.Contains(dataColumn.ColumnName))
                {
                    destDataRow[dataColumn.ColumnName] = dataRow[dataColumn];
                }
            }
        }

        public static void CheckStringLength(this string text, int length)
        {
            if (text.Length < length)
            {
                throw new Exception(string.Format("字符{0}長度少於{1}", text, length));
            }
        }

        #endregion

        #region DB Common
        // 从DBDataReader获取表格式
        public static System.Data.DataTable GetTableSchemaByDBDataReader(DbDataReader dataReader)
        {
            dataReader.CheckNull("Data reader cannot be null.");

            DataTable dataTable = new System.Data.DataTable();
            DataTable schemaTable = dataReader.GetSchemaTable();
            List<DataColumn> primaryKeyList = new List<DataColumn>();
            foreach (System.Data.DataRow schemaRow in schemaTable.Rows)
            {
                DataColumn newColumn = dataTable.Columns.Add(schemaRow.Field<string>("ColumnName"));
                newColumn.DataType = schemaRow.Field<Type>("DataType");
                newColumn.AllowDBNull = schemaRow.Field<bool>("AllowDBNull");

                if (!schemaRow.IsNull("IsKey") && schemaRow.Field<bool>("IsKey"))
                {
                    primaryKeyList.Add(newColumn);
                }
            }

            // 設定主鍵
            dataTable.PrimaryKey = primaryKeyList.ToArray();

            return dataTable;
        }
        #endregion

        #region DataSet

        public static DataRow FindRow(this DataTable dataTable
            , params object[] keyValues)
        {
            List<DataRow> deleteRowList = new List<DataRow>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i].RowState == DataRowState.Deleted)
                {
                    deleteRowList.Add(dataTable.Rows[i]);
                    dataTable.Rows[i].RejectChanges();
                }
            }

            DataRow dataRow = dataTable.Rows.Find(keyValues);

            foreach (DataRow deletedRow in deleteRowList)
            {
                deletedRow.Delete();
            }
            return dataRow;
        }

        public static List<DataRow> FindRows(this DataTable dataTable
            , DataColumn[] dataColumns
            , object[] keyValues)
        {
            List<DataRow> dataRowList = new List<DataRow>();
            if (dataColumns.IsNullOrEmpty() || keyValues.IsNullOrEmpty())
                return dataRowList;

            (dataColumns.Length == keyValues.Length).CheckTrue("Column count and value count must be equal.");

            List<DataRow> deleteRowList = new List<DataRow>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i].RowState == DataRowState.Deleted)
                {
                    deleteRowList.Add(dataTable.Rows[i]);
                    dataTable.Rows[i].RejectChanges();
                }
            }

            // 查找行
            foreach (System.Data.DataRow dataRow in dataTable.Rows)
            {
                bool isEqual = true;
                for (int i = 0; i < dataColumns.Length; i++)
                {
                    if (!dataRow[dataColumns[i]].IsEqual(keyValues[i]))
                    {
                        isEqual = false;
                        break;
                    }
                }
                if (isEqual)
                    dataRowList.Add(dataRow);
            }

            foreach (DataRow deletedRow in deleteRowList)
            {
                deletedRow.Delete();
            }
            return dataRowList;
        }

        /// <summary>
        /// 獲取主鍵值
        /// </summary>
        /// <param name="dataRow">數據行</param>
        /// <returns>主鍵值列表</returns>
        public static object[] GetKeyValues(this DataRow dataRow)
        {
            return GetColumnValues(dataRow, dataRow.Table.PrimaryKey);
        }

        /// <summary>
        /// 獲取列數組的值
        /// </summary>
        /// <param name="dataRow">數據行</param>
        /// <param name="dataColumns">列數組</param>
        /// <returns>列數組的值</returns>
        public static object[] GetColumnValues(this DataRow dataRow, params DataColumn[] dataColumns)
        {
            if (dataColumns.IsNullOrEmpty())
                return null;

            bool deleted = false;
            if (dataRow.RowState == DataRowState.Deleted)
            {
                deleted = true;
                dataRow.RejectChanges();
            }

            object[] keyValues = new object[dataColumns.Length];
            for (int i = 0; i < dataColumns.Length; i++)
            {
                keyValues[i] = dataRow[dataColumns[i]];
            }

            if (deleted)
                dataRow.Delete();
            return keyValues;
        }

        /// <summary>
        /// 檢查某個列是否屬於表
        /// </summary>
        public static void CreateForeignKeys(this DataSet dataSet)
        {
            // 刪除所有外鍵
            foreach (DataTable dataTable in dataSet.Tables)
            {
                int i = 0;
                while (i < dataTable.Constraints.Count)
                {
                    if (dataTable.Constraints[i] is ForeignKeyConstraint)
                    {
                        dataTable.Constraints.Remove(dataTable.Constraints[i]);
                    }
                    else
                        i++;
                }
            }

            foreach (DataRelation dataRelation in dataSet.Relations)
            {
                ForeignKeyConstraint fk = new ForeignKeyConstraint(dataRelation.ParentColumns
                    , dataRelation.ChildColumns);
                fk.AcceptRejectRule = AcceptRejectRule.None;
                fk.DeleteRule = Rule.Cascade;
                fk.UpdateRule = Rule.Cascade;
                dataRelation.ChildTable.Constraints.Add(fk);
            }
        }

        /// <summary>
        /// 檢查某個列是否屬於表
        /// </summary>
        public static void CheckColumnInTable(this DataTable dataTable, string columnName, string errorMessage)
        {
            System.Diagnostics.Debug.Assert(dataTable != null);
            if (!dataTable.Columns.Contains(columnName))
            {
                throw new Exception(string.Format(errorMessage, "TableName", dataTable.TableName, "ColumnName", columnName));
            }
        }

        /// <summary>
        /// 檢查某個列是否屬於表
        /// </summary>
        public static void CheckColumnInTable(this DataTable dataTable, string columnName)
        {
            CheckColumnInTable(dataTable, columnName, "Column is not belong to datatable.");
        }

        /// <summary>
        /// 獲取數據集中第一個表
        /// </summary>
        /// <returns>如果數據集中沒有表則返回空，否則返回第一個表</returns>
        public static DataTable FirstTable(this DataSet dataSet)
        {
            dataSet.CheckNull();

            if (dataSet.Tables.Count == 0)
                return null;

            return dataSet.Tables[0];
        }

        public static DataRow FirstRow(this DataTable dataTable)
        {
            dataTable.CheckNull();

            if (dataTable.Rows.Count == 0)
                return null;

            return dataTable.Rows[0];
        }

        /// <summary>
        /// 獲取除外變更
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static DataTable GetChangesExcept(this DataTable dataTable, DataRowState state)
        {
            dataTable.CheckNull();

            DataTable dataChangedTable = new DataTable();

            if (state != DataRowState.Added && !dataTable.GetChanges(DataRowState.Added).IsNullOrEmpty())
            {
                dataChangedTable.Merge(dataTable.GetChanges(DataRowState.Added));
            }
            if (state != DataRowState.Deleted && !dataTable.GetChanges(DataRowState.Deleted).IsNullOrEmpty())
            {
                dataChangedTable.Merge(dataTable.GetChanges(DataRowState.Deleted));
            }
            if (state != DataRowState.Modified && !dataTable.GetChanges(DataRowState.Modified).IsNullOrEmpty())
            {
                dataChangedTable.Merge(dataTable.GetChanges(DataRowState.Modified));
            }
            if (state != DataRowState.Unchanged && !dataTable.GetChanges(DataRowState.Unchanged).IsNullOrEmpty())
            {
                dataChangedTable.Merge(dataTable.GetChanges(DataRowState.Unchanged));
            }

            return dataChangedTable;
        }

        public static T FieldByColumnList<T>(this DataRow dataRow, T defaultValue, params string[] fieldNames)
        {
            fieldNames.CheckNull("字段列表不能为空");
            foreach (string fieldName in fieldNames)
            {
                if (dataRow.Table.Columns.Contains(fieldName))
                    return dataRow.Field<T>(fieldName, defaultValue);
            }

            throw new Exception(String.Format("字段列表数组不存在{0}", fieldNames.ToString()));
        }

        public static T Field<T>(this DataRow dataRow, string fieldName, T defaultValue)
        {
            if (dataRow.IsNull(fieldName))
                return defaultValue;
            return Convert.ChangeType(dataRow[fieldName], typeof(T)).Cast<T>(defaultValue);
        }

        /// <summary>
        /// 获取关联
        /// </summary>
        public static System.Data.DataRelation GetDataRelatedByLookupColumn(this DataColumn dataColumn)
        {
            DataTable dataTable = dataColumn.Table;
            System.Diagnostics.Debug.Assert(dataTable.DataSet != null);
            foreach (DataRelation dataRelation in dataTable.DataSet.Relations)
            {
                if (dataRelation.ChildColumns[0] == dataColumn && dataRelation.ChildColumns.Length == 1)
                {
                    return dataRelation;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取列所关联的表
        /// </summary>
        public static System.Data.DataTable GetRelatedTableByLookupColumn(DataColumn dataColumn)
        {
            DataRelation dataRelation = GetDataRelatedByLookupColumn(dataColumn);
            if (dataRelation.IsNull())
                return null;
            return dataRelation.ParentTable;
        }

        public static string GetLookupTableName(this DataColumn dataColumn)
        {
            DataTable dataTable = dataColumn.Table;
            return dataTable.TableName + "_" + dataColumn.ColumnName + "_" + "LookupTable";
        }

        public static System.Data.DataRow FindFirstRow(this DataTable dataTable
            , string filter
            , params object[] parameters)
        {
            DataRow[] dataRows = dataTable.Select(String.Format(filter, parameters));
            if (!dataRows.IsNullOrEmpty())
                return dataRows[0];
            return null;
        }

        public static void AddRelation(this DataSet dataSet, DataColumn parentColumn, DataColumn childColumn)
        {
            string relationName = String.Format("FK_{0}_{1}", childColumn.Table.TableName, childColumn.ColumnName);
            dataSet.Relations.Add(relationName, parentColumn, childColumn);
        }

        /// <summary>
        /// 獲取指定Distinct內容
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        #endregion Data

        #region 为空判断
        /// <summary>
        /// 空的Blob
        /// </summary>
        public static readonly byte[] NullBlob = new byte[] { 0 };

        /// <summary>
        /// 判斷是否為空Blob字段
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        public static bool IsNullBlob(byte[] blob)
        {
            if (blob != null && blob.Length == 1 && blob[0] == NullBlob[0])
                return true;

            return false;
        }

        /// <summary>
        /// 判断对象是否为空
        /// </summary>
        /// <param name="testObject">测试对象</param>
        /// <returns>如果对象为DBNull或者为null则返回true,否则返回false</returns>
        public static bool IsNull(this System.Object testObject)
        {
            return testObject == Convert.DBNull || ((object)testObject) == null;
        }

        /// <summary>
        /// 对象是否为空
        /// </summary>
        /// <param name="testObject">测试对象</param>
        /// <returns>如果对象为DBNull或为null或者为空字符串或者为空数组，则返回true；否则返回false</returns>
        public static bool IsNullOrEmpty(this System.Object testObject)
        {
            if (testObject.IsNull())
                return true;

            if (testObject is string)
            {
                return String.IsNullOrEmpty(((string)testObject));
            }
            else if (testObject is ICollection)
            {
                if (((ICollection)testObject).Count == 0)
                    return true;
            }
            else if (testObject is Array)
            {
                if (((Array)testObject).Length == 0)
                    return true;
            }

            return false;
        }

        public static string DateTimeANDConditionString(string columnName,
            string beginDateTime, string endDateTime)
        {
            if (string.IsNullOrEmpty(columnName)) return "";
            string rtn = "";
            rtn += (!string.IsNullOrEmpty(beginDateTime) ? (" AND " + columnName + ">= TO_DATE ('" + beginDateTime.Trim() + "','yyyy/mm/dd hh24:mi:ss')  ") : "");
            rtn += (!string.IsNullOrEmpty(endDateTime) ? (" AND " + columnName + "<=TO_DATE ('" + endDateTime.Trim() + "','yyyy/mm/dd hh24:mi:ss') ") : "");
            return rtn;
        }

        #endregion

        #region 檢查
        /// <summary>
        /// 檢查布爾型變量是否為True
        /// </summary>
        public static void CheckTrue(this bool boolValue, string message, params object[] extendParameters)
        {
            if (!boolValue)
                throw new Exception(message + extendParameters);
        }

        /// <summary>
        /// 檢查布爾型變量是否為False
        /// </summary>
        public static void CheckFalse(this bool boolValue, string message, params object[] extendParameters)
        {
            CheckTrue(!boolValue, message, extendParameters);
        }

        /// <summary>
        /// 判断对象是否为空
        /// </summary>
        /// <param name="testObject">测试对象</param>
        /// <returns>如果对象为DBNull或者为null则返回true,否则返回false</returns>
        public static void CheckNullOrEmpty(this System.Object testObject)
        {
            testObject.CheckNullOrEmpty("Object is null or empty.");
        }

        /// <summary>
        /// 判断对象是否为空
        /// </summary>
        /// <param name="testObject">测试对象</param>
        /// <returns>如果对象为DBNull或者为null则返回true,否则返回false</returns>
        public static void CheckNullOrEmpty(this System.Object testObject, string message, params object[] extendParamters)
        {
            if (testObject.IsNullOrEmpty())
                throw new Exception(message + extendParamters);
        }

        /// <summary>
        /// 判断对象是否为空
        /// </summary>
        /// <param name="testObject">测试对象</param>
        /// <returns>如果对象为DBNull或者为null则返回true,否则返回false</returns>
        public static void CheckNull(this System.Object testObject)
        {
            testObject.CheckNull("源对象类型不能为空");
        }

        /// <summary>
        /// 判断对象是否为空
        /// </summary>
        /// <param name="testObject">测试对象</param>
        /// <returns>如果对象为DBNull或者为null则返回true,否则返回false</returns>
        public static void CheckNull(this System.Object testObject, string message, params object[] extendParamters)
        {
            if (testObject.IsNull())
                throw new Exception(string.Format(message, extendParamters));
        }
        #endregion

        #region Object Clone
        public static object DataContractClone(this object srcObject)
        {
            if (srcObject == null)
                return null;

            Type dataType = srcObject.GetType();
            DataContractSerializer serializer = new DataContractSerializer(dataType);

            // Write
            System.IO.MemoryStream memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, srcObject);

            // Read
            memoryStream.Seek(0, SeekOrigin.Begin);
            object returnObject = serializer.ReadObject(memoryStream);
            return returnObject;
        }

        /// <summary>
        /// 二進制克隆
        /// </summary>
        /// <param name="srcObject">需要克隆的對象</param>
        /// <returns>克隆后的對象</returns>
        public static object BinaryClone(this object srcObject)
        {
            return srcObject.BinaryClone(true);
        }
        /// <summary>
        /// 二進制克隆
        /// </summary>
        /// <param name="srcObject">需要克隆的對象</param>
        /// <param name="newID">是否產生新ID</param>
        /// <returns>克隆后的對象</returns>
        public static object BinaryClone(this object srcObject, bool newID)
        {
            if (srcObject == null)
                return null;

            System.IO.MemoryStream memoryStream = new MemoryStream();
            srcObject.WriteObjectToBinaryStream(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            object newObject = memoryStream.ReadObjectFromBinaryStream();

            // 產生NewID
            if (newID && !newObject.IsNull() && newObject is INewID)
            {
                newObject.Cast<INewID>().NewID();
            }
            return newObject;
        }
        #endregion

        #region 对象是否相等
        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="object1">对象1</param>
        /// <param name="object2">对象2</param>
        /// <returns>如果两个对象相等，则返回true；否则返回false</returns>
        public static bool IsEqual(this object object1, object object2)
        {
            if (object1 == Convert.DBNull && object2 == Convert.DBNull)
                return true;

            if (object1 == Convert.DBNull && object2 != Convert.DBNull)
                return false;

            if (object1 != Convert.DBNull && object2 == Convert.DBNull)
                return false;

            if (object1 == object2)
                return true;

            return object1.Equals(object2);
        }

        /// <summary>
        /// 判断两个对象是否相等
        /// </summary>
        /// <param name="object1">对象1</param>
        /// <param name="object2">对象2</param>
        /// <returns>如果两个对象相等，则返回true；否则返回false</returns>
        public static bool IsArrayEqual(this object[] array1, object[] array2)
        {
            if (array1 == array2)
                return true;

            if (array1 == null && array2 == null)
                return true;

            if (array1 == null && array2 != null)
                return false;

            if (array1 != null && array2 == null)
                return false;

            if (array1.Length != array2.Length)
                return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1[i].IsEqual(array2[i]))
                    return false;
            }

            return true; ;
        }

        #endregion

        #region 数组操作

        /// <summary>
        /// 數組連接
        /// </summary>
        public static object[] Concat(this object[] array1, params object[] array2)
        {
            if (array1.IsNull())
                return array2;

            if (array2.IsNull())
                return array1;
            IEnumerable<object> resultList = array1.Concat<object>(array2);
            return resultList.ToArray();
        }

        // 獲取子數組
        public static T[] SubArray<T>(this T[] sortedList, int startIndex)
        {
            if (sortedList.IsNull())
                return null;

            if (startIndex > sortedList.Length)
                return null;

            T[] array = new T[sortedList.Length - startIndex];
            sortedList.CopyTo(array, startIndex);
            return array;
        }

        /// <summary>
        /// 從鍵值對中獲取值
        /// </summary>
        /// <typeparam name="K">鍵類型</typeparam>
        /// <typeparam name="V">值類型</typeparam>
        /// <param name="keyValuePairs">鍵值對</param>
        /// <param name="key">鍵</param>
        /// <returns>值</returns>
        public static V GetValueByKey<K, V>(this KeyValuePair<K, V>[] keyValuePairs, K key)
        {
            keyValuePairs.CheckNullOrEmpty("Key value pair list is empty");

            KeyValuePair<K, V> keyValue = keyValuePairs.FirstOrDefault(p => p.Key.IsEqual(key));
            keyValue.CheckNull("Key does not exist in key value pair list"
                , "Key", key);

            return keyValue.Value;
        }

        #endregion

        /// <summary>
        /// 数值转换为字节
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static byte[] GetByteByInt(int number)
        {
            byte[] bytes = new byte[1];
            bytes[0] = (byte)number;
            return bytes;
        }
    }
}
