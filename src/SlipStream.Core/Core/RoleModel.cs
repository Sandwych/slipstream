using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
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

            Fields.Chars("name").WithLabel("Name").WithSize(128).WithRequired();
            Fields.ManyToMany("users", "core.user_role", "role", "user").WithLabel("Users");
            Fields.ManyToMany("rules", "core.rule_role", "role", "rule").WithLabel("Rules");
            Fields.OneToMany("model_access_entries", "core.model_access", "role")
                .WithLabel("Model Access Control");
            Fields.OneToMany("field_access_entries", "core.field_access", "role")
                .WithLabel("Field Access Control");
        }

    }
}
