using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;

namespace ObjectServer.Data
{
    public static class DbCommandExtensions
    {
        public static void PrepareNamedParameters(this IDbCommand sqlCommand, object[] args)
        {
            Debug.Assert(args != null);
            Debug.Assert(sqlCommand != null);

            for (int i = 0; i < args.Length; i++)
            {
                var value = args[i];
                var param = sqlCommand.CreateParameter();
                param.ParameterName = 'p' + i.ToString();
                param.Value = value;
                sqlCommand.Parameters.Add(param);
            }
        }
    }
}
