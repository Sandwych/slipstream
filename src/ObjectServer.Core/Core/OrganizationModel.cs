using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class OrganizationModel : AbstractSqlModel
    {

        public OrganizationModel()
            : base("core.organization")
        {
            Hierarchy = true;

            Fields.Chars("code").SetLabel("Code").SetSize(64).Required().Unique();
            Fields.Chars("name").SetLabel("Name").Required();
        }

        /// <summary>
        /// Organization 与 user model 比较特殊，因为在 init.sql 里已经最早建立了这个表
        /// 为了防止循环依赖，所以这里返回空
        /// </summary>
        /// <returns></returns>
        public override string[] GetReferencedObjects()
        {
            return new string[] { };
        }
    }
}
