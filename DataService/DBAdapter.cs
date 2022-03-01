using DataPublic;
using Interface;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataService
{
    public class DBAdapter : IDBAdapter
    {
        protected const string StringPattern = @"""[^""]*""";
        protected const string ParamPattern = @":\w+";
        protected const string HideForNullPattern = @"\(\s*\/\*\?\s*(?<ParamNumber>\w+)\s*\<\s*(?<NotNullFilterString>[^|]*)\|(?<NullFilterString>[^>]*)\s*\>\s*\*\/[^)]*\)";

        public string ServerDateCommand
        {
            get
            {
                return "SELECT SYSDATE FROM DUAL";
            }
        }

        public virtual void BuildCommands(DbDataAdapter adapter)
        {

        }

        public virtual DbDataAdapter CreateDBAdapter()
        {
            return null;
        }

        /// <summary>
        /// 查找SQL中的IN，找到並將查詢條件值填充到SQL
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="paramters"></param>
        /// <returns></returns>
        public static string FillValueForInParam(string cmd, SortedList<string, object> paramValues)
        {
            string tmp = cmd.Replace(" ", "");
            string pattern = "IN(:{0})";
            string finalCmd = cmd;

            for (int i = 0; i < paramValues.Count; i++)
            {
                if (paramValues.Values[i] == null || paramValues.Values[i] == DBNull.Value) continue;
                if (tmp.Contains(string.Format(pattern, paramValues.Keys[i])) ||
                    tmp.Contains(string.Format(pattern.ToLower(), paramValues.Keys[i])))
                {
                    bool isNumbers = false;

                    string value = paramValues.Values[i].ToString().Trim();

                    string[] values = value.Split(',');

                    if (isNumbers == false)
                    {
                        value = value.Trim(',');
                        value = "'" + value.Replace(",", "','") + "'";
                    }

                    finalCmd = finalCmd.Replace(":" + paramValues.Keys[i], value);
                }
            }

            return finalCmd;
        }

        public virtual void DeriveParameters(bool removeNullParameter, DbCommand command, SortedList<string, object> paramValues)
        {
            string commandString = command.CommandText;
            MatchCollection matches;
            Regex regex;

            // 去掉空參數
            if (removeNullParameter)
            {
                regex = new System.Text.RegularExpressions.Regex(HideForNullPattern);
                matches = regex.Matches(commandString);
                commandString = regex.Replace(commandString,
                    delegate (Match match)
                    {
                        string parameterName = match.Groups["ParamNumber"].Value;
                        string notNullString = match.Groups["NotNullFilterString"].Value;
                        string nullString = match.Groups["NullFilterString"].Value;

                        string result;
                        result = nullString;
                        if (paramValues.ContainsKey(parameterName)
                            && !paramValues[parameterName].IsNullOrEmpty())
                        {
                            result = notNullString;
                        }

                        return result;
                    });
            }
            commandString = FillValueForInParam(commandString, paramValues);
            command.CommandText = commandString;

            //清空字符串,防止干扰
            System.Text.RegularExpressions.Regex stringRegex = new Regex(StringPattern);
            commandString = stringRegex.Replace(commandString, "");

            //找到参数並添加到系統中
            System.Text.RegularExpressions.Regex paramRegex = new Regex(ParamPattern);
            matches = paramRegex.Matches(commandString);
            this.CommandParameterInsert(matches, command, paramValues);
        }

        public virtual void CommandParameterInsert(MatchCollection matches, DbCommand command, SortedList<string, object> paramValues)
        {

        }

        public virtual void DeriveProcedureParameters(DbCommand command)
        {

        }
    }
}
