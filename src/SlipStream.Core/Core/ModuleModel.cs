using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using NHibernate.SqlCommand;

using SlipStream.Model;
using SlipStream.Runtime;
using SlipStream.Data;

namespace SlipStream.Core
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [Resource]
    public sealed class ModuleModel : AbstractSqlModel
    {
        public static class States
        {
            public const string Uninstalled = "uninstalled";
            public const string Installed = "installed";
            public const string ToInstall = "to-install";
            public const string ToUpgrade = "to-upgrade";
            public const string ToUninstall = "to-uninstall";
        }

        public const string DbSequenceName = "core_module__id_seq";

        public const string ModelName = "core.module";
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>

        public ModuleModel()
            : base(ModelName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").SetLabel("Name").Required().SetSize(128).Unique().Readonly();
            Fields.Chars("label").SetLabel("Short Description").SetSize(256).Readonly();
            Fields.Enumeration("state", new Dictionary<string, string>()
            {
                { States.Uninstalled, "Uninstalled" },
                { States.Installed, "Installed" },
                { States.ToInstall, "To Install" },
                { States.ToUpgrade, "To Upgrade" },
                { States.ToUninstall, "To Uninstall" },
            }).Required().SetLabel("State").Readonly();
            Fields.Boolean("demo").SetLabel("Demostration?").Required().Readonly().SetDefaultValueGetter(getter => false);
            Fields.Chars("author").SetLabel("Author").SetSize(128).Readonly();
            Fields.Chars("url").SetLabel("Web Site").SetSize(128).Readonly();
            Fields.Chars("version").SetLabel("Version").SetSize(64).Readonly();
            Fields.Chars("license").SetLabel("License").SetSize(32).Readonly();
            Fields.OneToMany("depends", "core.module_dependency", "module")
                .SetLabel("Dependencies");
            Fields.Text("info").SetLabel("Information").Readonly();
        }


        [ServiceMethod("ButtonMark")]
        public static void ButtonMark(dynamic model, IServiceContext ctx, dynamic ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            var records = model.Read(ctx, ids, null);
            foreach (var r in records)
            {
                var originalState = (string)r["state"][0];
                string newState = originalState;

                if (originalState == States.Uninstalled || originalState == States.ToUninstall)
                {
                    newState = States.ToInstall;
                }
                else if (originalState == States.Installed)
                {
                    newState = States.ToUpgrade;
                }

                var newRecord = new Dictionary<string, object>()
                {
                    { "state", newState },
                };

                var sql = String.Format("update {0} set state=? where _id=?", model.TableName);
                var id = (long)r[IdFieldName];
                var rows = ctx.DataContext.Execute(SqlString.Parse(sql), newState, id);
                if (rows != 1)
                {
                    throw new Exceptions.DataException(
                        string.Format("Failed to update table [{0}] with ID [{1}]", model.TableName, id));
                }
            }
        }

    }
}
