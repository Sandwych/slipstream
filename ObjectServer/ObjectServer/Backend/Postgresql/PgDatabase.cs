using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend.Postgresql
{
    public class PgDatabase : DatabaseBase, IDatabase
    {
        protected static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region IDatabase 成员

        public string[] List()
        {
            var dbUser = "objectserver"; //TODO: 改成可配置的
            var sql = @"
                select datname from pg_database  
                    where datdba = (select distinct usesysid from pg_user where usename=@0) 
                        and datname not in ('template0', 'template1', 'postgres')  
	                order by datname asc;";

            if (Log.IsDebugEnabled)
            {
                Log.Info(sql);
            }

            using (var cmd = this.PrepareCommand(sql))
            {
                PrepareCommandParameters(cmd, dbUser);
                var result = new List<string>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
                return result.ToArray();
            }
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
