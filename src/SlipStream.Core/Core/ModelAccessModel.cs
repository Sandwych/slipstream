using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using Sandwych;
using NHibernate.SqlCommand;

using SlipStream.Model;

namespace SlipStream.Core
{
    /// <summary>
    /// 模型访问控制列表
    /// </summary>
    [Resource]
    public sealed class ModelAccessModel : AbstractSqlModel
    {
        public const string ModelName = "core.model_access";

        private const string SqlToQuery = @"
select max(case when ""a"".""allow_{0}"" = '1' then 1 else 0 end)
    from ""core_model_access"" ""a"" 
    join ""core_model"" ""m"" on (""a"".""model"" = ""m"".""_id"") 
    left join ""core_user_role_rel"" ""ur"" on (""ur"".""role"" = ""a"".""role"") 
    where ""m"".""name"" = ? and (""ur"".""user"" = ? or ""a"".""role"" is null)
";

        public ModelAccessModel()
            : base(ModelName)
        {
            Fields.ManyToOne("role", "core.role").WithRequired().WithLabel("Role");
            Fields.ManyToOne("model", "core.model").WithRequired().WithLabel("Model");
            Fields.Chars("name").WithLabel("Name");
            Fields.Boolean("allow_create").WithLabel("Allow Creation")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("allow_read").WithLabel("Allow Reading")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("allow_write").WithLabel("Allow Writing")
                .WithRequired().WithDefaultValueGetter(s => true);
            Fields.Boolean("allow_delete").WithLabel("Allrow Deletion")
                .WithRequired().WithDefaultValueGetter(s => true);
        }

        /// <summary>
        /// TODO: 此方法每次 CRUD 的时候都会被调用用来检查 CRUD 权限，因此需要缓存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CheckForCurrentUser(
            IServiceContext ctx, string model, string action)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("session");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentNullException("model");
            }

            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException("action");
            }

            var sql = String.Format(CultureInfo.InvariantCulture, SqlToQuery, action);
            var result = ctx.DataContext.QueryValue(sql, model, ctx.UserSession.UserId);

            if (!result.IsNull())
            {
                return (Convert.ToInt32(result) > 0);
            }
            else
            {
                return true;
            }
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

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ids"></param>
        public override void DeleteInternal(long[] ids)
        {
            base.DeleteInternal(ids);
        }
    }
}
