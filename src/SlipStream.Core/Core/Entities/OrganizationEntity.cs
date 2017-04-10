using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{
    [Resource]
    public sealed class OrganizationEntity : AbstractSqlEntity
    {

        public OrganizationEntity() : base("core.organization")
        {
            Hierarchy = true;

            Fields.Chars("code").WithLabel("Code").WithSize(64).WithRequired().WithUnique();
            Fields.Chars("name").WithLabel("Name").WithRequired();
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
