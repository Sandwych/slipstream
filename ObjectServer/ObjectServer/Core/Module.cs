using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class Module : ModelBase
    {

        public Module()
        {
            this.Name = "core.module";
            this.Automatic = false;
            this.Versioned = false;

            this.DefineField("name", "Name", "varchar", 128, true);
            this.DefineField("state", "State", "varchar", 16, true);
            this.DefineField("description", "Description", "text", 0xffff, false);
        }

        public static void LoadModules(string dbName, IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select name from core_module;";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);
                        LoadModule(dbName, conn, name);
                    }
                }

            }
        }

        private static void LoadModule(string dbName, IDbConnection conn, string module)
        {
        }

    }
}
