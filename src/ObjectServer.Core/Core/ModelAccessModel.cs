using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public ModelAccessModel()
            : base(ModelName)
        {
            Fields.ManyToOne("group", "core.group").Required().SetLabel("User Group");
            Fields.ManyToOne("model", "core.model").Required().SetLabel("Model");
            Fields.Chars("name").SetLabel("Name");
            Fields.Boolean("allow_create").SetLabel("Allow Creation")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_read").SetLabel("Allow Reading")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_write").SetLabel("Allow Writing")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_delete").SetLabel("Allrow Deletion")
                .Required().DefaultValueGetter(s => true);
        }

        /// <summary>
        /// TODO: 此方法每次 CRUD 的时候都会被调用用来检查 CRUD 权限，因此需要缓存
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Dictionary<string, object>[]
            FindByModelAndUserId(IServiceScope scope, string model, long userId)
        {
            var sql = @"
SELECT DISTINCT ma._id, ma.allow_create, ma.allow_read, ma.allow_write, ma.allow_delete
    FROM core_model_access ma
    INNER JOIN core_model m ON m._id = ma.model
    INNER JOIN core_user_group_rel ugr ON ugr.gid = ma.group
    WHERE (ugr.uid = @0) AND (m.name = @1)
";
            var result = scope.Connection.QueryAsDictionary(sql, userId, model);

            return result;
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="userRecord"></param>
        /// <returns></returns>
        public override long CreateInternal(IServiceScope scope, IDictionary<string, object> userRecord)
        {
            return base.CreateInternal(scope, userRecord);
        }

        /// <summary>
        /// TODO 更新缓存
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="id"></param>
        /// <param name="userRecord"></param>
        public override void WriteInternal(IServiceScope scope, long id, IDictionary<string, object> userRecord)
        {
            base.WriteInternal(scope, id, userRecord);
        }


    }
}
