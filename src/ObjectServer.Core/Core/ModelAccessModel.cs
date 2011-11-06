using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using NHibernate.SqlCommand;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    /// <summary>
    /// 模型访问控制列表
    /// </summary>
    [Resource]
    public sealed class ModelAccessModel : AbstractSqlModel
    {
        public const string ModelName = "core.model_access";

        private const string SqlToQuery = @"
select max(case when a.allow_{0} then 1 else 0 end) > 0
    from core_model_access a 
    join core_model m on (a.model = m._id) 
    left join core_user_role_rel ur on (ur.role = a.role) 
    where m.name = ? and (ur.user = ? or a.role is null)
";

        public ModelAccessModel()
            : base(ModelName)
        {
            Fields.ManyToOne("role", "core.role").Required().SetLabel("Role");
            Fields.ManyToOne("model", "core.model").Required().SetLabel("Model");
            Fields.Chars("name").SetLabel("Name");
            Fields.Boolean("allow_create").SetLabel("Allow Creation")
                .Required().SetDefaultValueGetter(s => true);
            Fields.Boolean("allow_read").SetLabel("Allow Reading")
                .Required().SetDefaultValueGetter(s => true);
            Fields.Boolean("allow_write").SetLabel("Allow Writing")
                .Required().SetDefaultValueGetter(s => true);
            Fields.Boolean("allow_delete").SetLabel("Allrow Deletion")
                .Required().SetDefaultValueGetter(s => true);
        }

        /// <summary>
        /// TODO: 此方法每次 CRUD 的时候都会被调用用来检查 CRUD 权限，因此需要缓存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CheckForCurrentUser(
            ITransactionContext ctx, string model, string action)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
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
            var sqlStr = SqlString.Parse(sql);
            var result = ctx.DBContext.QueryValue(sqlStr, model, ctx.Session.UserId);

            if (!result.IsNull())
            {
                return (bool)result;
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
        public override long CreateInternal(
            ITransactionContext ctx, IDictionary<string, object> userRecord)
        {
            return base.CreateInternal(ctx, userRecord);
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="id"></param>
        /// <param name="userRecord"></param>
        public override void WriteInternal(
            ITransactionContext ctx, long id, IDictionary<string, object> userRecord)
        {
            base.WriteInternal(ctx, id, userRecord);
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ids"></param>
        public override void DeleteInternal(ITransactionContext ctx, long[] ids)
        {
            base.DeleteInternal(ctx, ids);
        }
    }
}
