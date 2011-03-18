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

        public ModelAccessModel()
            : base("core.model_access")
        {
            Fields.ManyToOne("group", "core.group").Required().SetLabel("User Group");
            Fields.ManyToOne("model", "core.model").Required().SetLabel("Model");
            Fields.Chars("description").SetLabel("Description");
            Fields.Boolean("allow_create").SetLabel("Allow Creation")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_read").SetLabel("Allow Reading")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_write").SetLabel("Allow Writing")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("allow_delete").SetLabel("Allrow Deletion")
                .Required().DefaultValueGetter(s => true);
        }

        public List<Dictionary<string, object>>
            FindAllByUserId(IResourceScope scope, long userId)
        {
            var sql = @"
SELECT DISTINCT ma.id, ma.allow_create, ma.allow_read, ma.allow_write, ma.allow_delete
    FROM core_model_access ma
    INNER JOIN core_model m ON m.id = ma.model
    INNER JOIN core_user_group_rel ugr ON ugr.gid = ma.group
    WHERE (ugr.uid = @0) AND (m.name = @1)
";
            var result = scope.DatabaseProfile.DataContext.QueryAsDictionary(sql, userId, this.Name);

            return result;
        }


    }
}
