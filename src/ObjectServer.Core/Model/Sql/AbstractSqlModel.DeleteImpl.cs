using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;
using System.Globalization;

using NHibernate.SqlCommand;

using ObjectServer.Data;
using ObjectServer.Utility;
using ObjectServer.Exceptions;

namespace ObjectServer.Model
{
    public abstract partial class AbstractSqlModel : AbstractModel
    {
        private static readonly string[] HierarchyFields =
            new string[] { IdFieldName, LeftFieldName, RightFieldName };

        public override void DeleteInternal(ITransactionContext ctx, long[] ids)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            if (ids.Length == 0)
            {
                return;
            }

            //继承的删除策略很简单：先删除本尊，再删除各个基类表
            Dictionary<string, object>[] existedRecords = null;
            if (this.Inheritances.Count > 0)
            {
                var sql = new SqlString(
                    "select * from ",
                    DataProvider.Dialect.QuoteForTableName(this.TableName),
                    " where ",
                    DataProvider.Dialect.QuoteForColumnName(AbstractModel.IdFieldName),
                    " in (",
                    ids.ToCsv(),
                    ")");

                existedRecords = ctx.DBContext.QueryAsDictionary(sql);
            }

            DeleteRows(ctx, ids, this);
            this.ProcessBaseModelsDeletion(ctx, existedRecords);
        }

        private void ProcessBaseModelsDeletion(ITransactionContext scope, Dictionary<string, object>[] existedRecords)
        {
            Debug.Assert(scope != null);

            if (this.Inheritances.Count > 0)
            {
                Debug.Assert(existedRecords != null);

                foreach (var inheritInfo in this.Inheritances)
                {
                    var baseIds = from r in existedRecords
                                  let f = r[inheritInfo.RelatedField]
                                  where !f.IsNull()
                                  select (long)f;

                    if (baseIds.Any())
                    {
                        var baseModel = (AbstractSqlModel)scope
                            .GetResource(inheritInfo.BaseModel);
                        DeleteRows(scope, baseIds.ToArray(), baseModel);
                    }
                }
            }
        }

        private static void DeleteRows(
            ITransactionContext ctx, long[] ids, AbstractSqlModel tableModel)
        {
            Debug.Assert(ctx != null);
            Debug.Assert(ids != null);
            Debug.Assert(tableModel != null);

            if (tableModel.Hierarchy)
            {
                UpdateTreeForDeletion(ctx, ids, tableModel);
            }
            else
            {
                var sql = new SqlString(
                    "delete from ", tableModel.quotedTableName,
                    " where ", QuotedIdColumn, " in (", ids.ToCsv(), ")");

                var rowCount = ctx.DBContext.Execute(sql);
                if (rowCount != ids.Count())
                {
                    var msg = string.Format("Failed to delete model '{0}'", tableModel.Name);
                    throw new ObjectServer.Exceptions.DataException(msg);
                }
            }
        }

        private static void UpdateTreeForDeletion(
            ITransactionContext ctx, long[] ids, AbstractSqlModel tableModel)
        {
            var records =
                from r in tableModel.ReadInternal(ctx, ids, HierarchyFields)
                select new
                {
                    ID = (long)r[IdFieldName],
                    Left = (long)r[LeftFieldName],
                    Right = (long)r[RightFieldName]
                };

            //这里是做化简，如果 ids 里面有的节点 id 已经是被其它节点包含了，那么就去掉
            //TODO 这里是个 O(n^2) 的复杂度，应该可以用二分搜索优化的
            //先删最右侧的很重要，否则 parentRecords 是保存在内存里的，没法反应出后面的 update 语句带来的更改
            var parentRecords =
                from r in records
                where !records.Any(i => i.Left < r.Left && i.Right > r.Right)
                orderby r.Right descending
                select r;

            ctx.DBContext.LockTable(tableModel.TableName);

            foreach (var record in parentRecords)
            {

                var width = record.Right - record.Left;
                //删除指定ID节点及其下面包含的子孙节点
                var sql = new SqlString(
                    "delete from ", tableModel.quotedTableName,
                    " where ", LeftFieldName,
                    " between ", Parameter.Placeholder, " and ", Parameter.Placeholder);
                ctx.DBContext.Execute(sql, record.Left, record.Right);

                //更新所有右边节点的 _left 与 _right
                var sqlUpdate1 = String.Format(CultureInfo.InvariantCulture,
                    "update {0} set _right = _right - {1} where _right > {2}",
                    tableModel.quotedTableName, width + 1, record.Right);

                var sqlUpdate2 = String.Format(CultureInfo.InvariantCulture,
                    "update {0} set _left = _left - {1} where _left > {2}",
                    tableModel.quotedTableName, width + 1, record.Left);

                ctx.DBContext.Execute(SqlString.Parse(sqlUpdate1));
                ctx.DBContext.Execute(SqlString.Parse(sqlUpdate2));
            }
        }
    }
}
