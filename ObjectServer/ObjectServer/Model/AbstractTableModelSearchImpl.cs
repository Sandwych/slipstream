using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using ObjectServer.Backend;
using ObjectServer.Utility;
using ObjectServer.SqlTree;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {

        public override long[] SearchInternal(
            IResourceScope ctx, object[] domain = null, long offset = 0, long limit = 0)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            object[] domainInternal = domain;
            if (domain == null)
            {
                domainInternal = new object[][] { };
            }

            var fields = domainInternal.Select(d => (string)((object[])d)[0]);
            var parser = new DomainParser(this, domain);
            var columnExps = new AliasExpressionList(new string[] { "id" });
            var whereExp = parser.ToExpressionTree();
            var select = new SelectStatement(
                columnExps,
                new FromClause(new string[] { this.TableName }),
                new WhereClause(whereExp));

            if (offset > 0)
            {
                select.OffsetClause = new OffsetClause(offset);
            }

            if (limit > 0)
            {
                select.LimitClause = new LimitClause(limit);
            }

            //TODO: 这里检查权限等，处理查询非表中字段等

            //TODO: 自动添加 active 字段
         
            var sv = new StringifierVisitor();
            select.Traverse(sv);
            var sql = sv.ToString();

            using (var cmd = ctx.DatabaseProfile.DataContext.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<long>();
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt64(0));
                    }
                    return result.ToArray();
                }
            }
        }

    }
}
