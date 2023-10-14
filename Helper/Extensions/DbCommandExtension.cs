using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Helper.Extensions
{
    public static class DbCommandExtension
    {
        public static void AddParameterWithValue(this DbCommand command, string parameterName, object? parameterValue)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = parameterValue ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}