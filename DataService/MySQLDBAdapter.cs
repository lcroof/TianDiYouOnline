using Interface;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataService
{
    public class MySQLDBAdapter : DBAdapter, IDBAdapter
    {
        public override void BuildCommands(DbDataAdapter adapter)
        {
            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(adapter as MySqlDataAdapter);
            commandBuilder.ConflictOption = System.Data.ConflictOption.OverwriteChanges;
        }

        public override DbDataAdapter CreateDBAdapter()
        {
            return new MySqlDataAdapter();
        }

        public override void CommandParameterInsert(MatchCollection matches, DbCommand command, SortedList<string, object> paramValues)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                if (string.Compare(matches[i].Value, ":mi", true) == 0 ||
                    string.Compare(matches[i].Value, ":ss", true) == 0)
                {
                    continue;
                }

                string parameterName = matches[i].Value.Substring(1);
                if (!command.Parameters.Contains(parameterName))
                {
                    if (paramValues.ContainsKey(parameterName) && paramValues[parameterName] != null)
                    {
                        (command as MySqlCommand).Parameters.AddWithValue(parameterName, paramValues[parameterName]);
                    }
                    else
                    {
                        (command as MySqlCommand).Parameters.AddWithValue(parameterName, DBNull.Value);
                    }
                }
                else
                {
                    if (paramValues.ContainsKey(parameterName) && paramValues[parameterName] != null)
                    {
                        command.Parameters[parameterName].Value = paramValues[parameterName];
                    }
                    else
                    {
                        command.Parameters[parameterName].Value = DBNull.Value;
                    }
                }
            }
        }

        public override void DeriveProcedureParameters(DbCommand command)
        {
            MySqlCommandBuilder.DeriveParameters(command as MySqlCommand);
        }
    }
}
