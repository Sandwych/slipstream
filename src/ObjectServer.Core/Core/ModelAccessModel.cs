using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.SqlCommand;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    /// <summary>
    /// 模型访问控制列表
    /// </summary>
    [Resource]
    public sealed class ModelAccessModel : AbstractTableModel
    {
        public const string ModelName = "core.model_access";

        private static readonly SqlString SqlToQuery = SqlString.Parse(@"
SELECT DISTINCT ma._id, ma.allow_create, ma.allow_read, ma.allow_write, ma.allow_delete
    FROM core_model_access ma
    INNER JOIN core_model m ON m._id=ma.model
    INNER JOIN core_user_role_rel urr ON urr.role=ma.role
    WHERE (urr.user=?) AND (m.name=?)
");

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
        public Dictionary<string, object>[]
            FindByModelAndUserId(IServiceContext ctx, string model, long userID)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentNullException("model");
            }

            var sql = SqlToQuery;
            var result = ctx.DBContext.QueryAsDictionary(sql, userID, model);

            return result;
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="userRecord"></param>
        /// <returns></returns>
        public override long CreateInternal(IServiceContext scope, IDictionary<string, object> userRecord)
        {
            return base.CreateInternal(scope, userRecord);
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="id"></param>
        /// <param name="userRecord"></param>
        public override void WriteInternal(IServiceContext scope, long id, IDictionary<string, object> userRecord)
        {
            base.WriteInternal(scope, id, userRecord);
        }


    }
}
