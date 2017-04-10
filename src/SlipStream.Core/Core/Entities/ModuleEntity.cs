using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using NHibernate.SqlCommand;

using SlipStream.Entity;
using SlipStream.Runtime;
using SlipStream.Data;

namespace SlipStream.Core
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [Resource]
    public sealed class ModuleEntity : AbstractSqlEntity
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

        public const string EntityName = "core.module";
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>

        public ModuleEntity() : base(EntityName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(128).WithUnique().WithReadonly();
            Fields.Chars("label").WithLabel("Short Description").WithSize(256).WithReadonly();
            Fields.Enumeration("state", new Dictionary<string, string>()
            {
                { States.Uninstalled, "Uninstalled" },
                { States.Installed, "Installed" },
                { States.ToInstall, "To Install" },
                { States.ToUpgrade, "To Upgrade" },
                { States.ToUninstall, "To Uninstall" },
            }).WithRequired().WithLabel("State").WithReadonly();
            Fields.Boolean("demo").WithLabel("Demostration?").WithRequired().WithReadonly().WithDefaultValueGetter(getter => false);
            Fields.Chars("author").WithLabel("Author").WithSize(128).WithReadonly();
            Fields.Chars("url").WithLabel("Web Site").WithSize(128).WithReadonly();
            Fields.Chars("version").WithLabel("Version").WithSize(64).WithReadonly();
            Fields.Chars("license").WithLabel("License").WithSize(32).WithReadonly();
            Fields.OneToMany("depends", "core.module_dependency", "module")
                .WithLabel("Dependencies");
            Fields.Text("info").WithLabel("Information").WithReadonly();
        }


        [ServiceMethod("ButtonMark")]
        public static void ButtonMark(dynamic entity, IServiceContext ctx, dynamic ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            var records = entity.Read(ctx, ids, null);
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

                var sql = String.Format("update {0} set state=? where _id=?", entity.TableName);
                var id = (long)r[IdFieldName];
                var rows = ctx.DataContext.Execute(SqlString.Parse(sql), newState, id);
                if (rows != 1)
                {
                    throw new Exceptions.DataException(
                        string.Format("Failed to update table [{0}] with ID [{1}]", entity.TableName, id));
                }
            }
        }

    }
}
