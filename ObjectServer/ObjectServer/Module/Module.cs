using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using Newtonsoft.Json;

using ObjectServer.Model;
using ObjectServer.Runtime;

namespace ObjectServer.Module
{

    [ServiceObject]
    public sealed class Module : ModelBase
    {

        public Module()
        {
            this.Name = "core.module";
            this.Automatic = false;
            this.Versioned = false;

            this.CharsField("name", "Name", 128, true, null);
            this.CharsField("state", "State", 16, true, null);
            this.TextField("description", "Description", false, null);
        }

        public static void LoadModules(IDbConnection conn, string dbName, ObjectPool pool)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "select name from core_module where state = 'installed';";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);
                        LoadModule(dbName, conn, name, pool);
                    }
                }

            }
        }

        private static void LoadModule(string dbName, IDbConnection conn, string module, ObjectPool pool)
        {
            var moduleDir = Path.Combine(@"c:\objectserver-modules", module);
            var moduleFilePath = Path.Combine(moduleDir, "module.js");

            ModuleInfo moduleInfo;
            var json = File.ReadAllText(moduleFilePath, Encoding.UTF8);

            moduleInfo = JsonConvert.DeserializeObject<ModuleInfo>(json);
            var assembly = CompileSourceFiles(moduleDir, moduleInfo);
            pool.RegisterModelsInAssembly(assembly);
        }

        private static System.Reflection.Assembly CompileSourceFiles(
            string moduleDir, ModuleInfo moduleInfo)
        {
            var sourceFiles = new List<string>();
            foreach (var file in moduleInfo.SourceFiles)
            {
                sourceFiles.Add(Path.Combine(moduleDir, file));
            }

            //编译模块程序并注册所有对象
            var bc = new BooCompiler();
            var assembly = bc.Compile(sourceFiles);
            return assembly;
        }

    }
}
