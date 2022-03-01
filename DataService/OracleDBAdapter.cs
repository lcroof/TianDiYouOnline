using Interface;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataService
{
    public class OracleDBAdapter : DBAdapter, IDBAdapter
    {
        public override void BuildCommands(DbDataAdapter adapter)
        {
            OracleCommandBuilder commandBuilder = new OracleCommandBuilder(adapter as OracleDataAdapter);
            commandBuilder.ConflictOption = System.Data.ConflictOption.OverwriteChanges;
        }

        public override DbDataAdapter CreateDBAdapter()
        {
            return new OracleDataAdapter();
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
                        (command as OracleCommand).Parameters.Add(parameterName, paramValues[parameterName]);
                    }
                    else
                    {
                        (command as OracleCommand).Parameters.Add(parameterName, DBNull.Value);

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
            OracleCommandBuilder.DeriveParameters(command as OracleCommand);
        }
    }
}
