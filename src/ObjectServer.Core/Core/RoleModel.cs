using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    /// <summary>
    /// 用户组信息
    /// </summary>
    [Resource]
    public sealed class RoleModel : AbstractSqlModel
    {

        public RoleModel()
            : base("core.role")
        {

            Fields.Chars("name").SetLabel("Name").SetSize(128).Required();
            Fields.ManyToMany("users", "core.user_role", "role", "user").SetLabel("Users");
            Fields.ManyToMany("rules", "core.rule_role", "role", "rule").SetLabel("Rules");
            Fields.OneToMany("model_access_entries", "core.model_access", "role")
                .SetLabel("Model Access Control");
            Fields.OneToMany("field_access_entries", "core.field_access", "role")
                .SetLabel("Field Access Control");
        }

    }
}
