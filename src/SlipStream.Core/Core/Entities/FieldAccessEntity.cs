using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using NHibernate.SqlCommand;

using SlipStream.Entity;

namespace SlipStream.Core
{
    /// <summary>
    /// 字段访问控制列表
    /// </summary>
    [Resource]
    public sealed class FieldAccessEntity : AbstractSqlEntity
    {
        public const string EntityName = "core.field_access";

        public FieldAccessEntity()
            : base(EntityName)
        {
            Fields.Chars("name").WithLabel("Name");
            Fields.ManyToOne("role", "core.role")
                .OnDelete(OnDeleteAction.Cascade).WithLabel("Role");
            Fields.ManyToOne("field", "core.meta_field").WithRequired()
                .OnDelete(OnDeleteAction.Cascade).WithLabel("Field");
            Fields.Boolean("allow_read").WithLabel("Allow Reading")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("allow_write").WithLabel("Allow Writing")
                .WithRequired().WithDefaultValueGetter(s => true);
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="userRecord"></param>
        /// <returns></returns>
        public override long CreateInternal(IDictionary<string, object> userRecord)
        {
            return base.CreateInternal(userRecord);
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="id"></param>
        /// <param name="userRecord"></param>
        public override void WriteInternal(long id, IDictionary<string, object> userRecord)
        {
            base.WriteInternal(id, userRecord);
        }

        public IDictionary<string, bool> GetFieldAccess(string entityName, IEnumerable<string> fields, string action)
        {
            if (string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentNullException(nameof(EntityName));
            }

            if (action != "read" && action != "write")
            {
                throw new ArgumentOutOfRangeException("action");
            }

            var metaEntityEntity = (IEntity)this.DbDomain.GetResource(MetaEntityEntity.EntityName );
            var fieldModel = (IEntity)this.DbDomain.GetResource(MetaFieldEntity.EntityName);
            var userRoleRelModel = (IEntity)this.DbDomain.GetResource("core.user_role");
            var sql = String.Format(CultureInfo.InvariantCulture,
                @"select ""f"".""name"" ""field_name"", (max(case when ""a"".""allow_{0}"" = '1' then 1 else 0 end)) ""allow"" " +
                @"from ""{1}"" ""a"" " +
                @"inner join ""{2}"" ""f"" on (""a"".""field"" = ""f"".""_id"") " +
                @"inner join ""{3}"" ""m"" on (""f"".""meta_entity"" = ""m"".""_id"") " +
                @"left join ""{4}"" ""ur"" on (""ur"".""role"" = ""a"".""role"") " +
                @"where ""m"".""name"" = ? and (""ur"".""user"" = ? or ""a"".""role"" is null) " +
                @"group by ""f"".""name"" ",
                action, this.TableName, fieldModel.TableName, metaEntityEntity.TableName, userRoleRelModel.TableName);

            var ctx = this.DbDomain.CurrentSession;
            Debug.Assert(ctx.UserSession != null);
            var userId = ctx.UserSession.UserId;
            var records = ctx.DataContext.QueryAsDictionary(sql, entityName, userId);
            if (records.Count() == 0)
            {
                return new Dictionary<string, bool>();
            }
            else
            {
                var result = new Dictionary<string, bool>(records.Length);
                foreach (var r in records)
                {
                    var name = (string)r["field_name"];
                    var value = (int)r["allow"] > 0;
                    result.Add(name, value);
                }
                return result;
            }
        }

    }
}
