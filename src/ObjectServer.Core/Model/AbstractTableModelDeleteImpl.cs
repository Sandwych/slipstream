using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using NHibernate.SqlCommand;

using ObjectServer.Data;
using ObjectServer.Utility;
using ObjectServer.Exceptions;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        private static readonly string[] HierarchyFields =
            new string[] { IDFieldName, LeftFieldName, RightFieldName };

        public override void DeleteInternal(IServiceContext scope, long[] ids)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            if (!scope.CanDeleteModel(scope.Session.UserID, this.Name))
            {
                throw new SecurityException("Access denied");
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
                    DataProvider.Dialect.QuoteForColumnName(AbstractModel.IDFieldName),
                    " in (",
                    ids.ToCommaList(),
                    ")");

                existedRecords = scope.DBContext.QueryAsDictionary(sql);
            }

            DeleteRows(scope, ids, this);
            this.ProcessBaseModelsDeletion(scope, existedRecords);
        }

        private void ProcessBaseModelsDeletion(IServiceContext scope, IEnumerable<Dictionary<string, object>> existedRecords)
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

                    if (baseIds.Count() > 0)
                    {
                        var baseModel = (AbstractTableModel)scope
                            .GetResource(inheritInfo.BaseModel);
                        DeleteRows(scope, baseIds, baseModel);
                    }
                }
            }
        }

        private static void DeleteRows(
            IServiceContext scope, IEnumerable<long> ids, AbstractTableModel tableModel)
        {
            Debug.Assert(scope != null);
            Debug.Assert(ids != null);
            Debug.Assert(tableModel != null);

            if (tableModel.Hierarchy)
            {
                var records =
                    from r in tableModel.ReadInternal(scope, ids.ToArray(), HierarchyFields)
                    select new
                    {
                        ID = (long)r[IDFieldName],
                        Left = (long)r[LeftFieldName],
                        Right = (long)r[RightFieldName]
                    };

                //这里是做化简，如果 ids 里面有的节点 id 已经是被其它节点包含了，那么就去掉
                //TODO 这里是个 O(n^2) 的复杂度，应该可以用二分搜索优化的
                //先删最右侧的很重要，否则 parentRecords 是保存在内存里的，没法反应出后面的 update 语句带来的更改
                var parentRecords =
                    from r in records
                    where records.Count(i => i.Left < r.Left && i.Right > r.Right) == 0
                    orderby r.Right descending  
                    select r;

                scope.DBContext.LockTable(tableModel.TableName);

                foreach (var record in parentRecords)
                {

                    var width = record.Right - record.Left;
                    //删除指定ID节点及其下面包含的子孙节点
                    var sql = new SqlString(
                        "delete from ", tableModel.quotedTableName,
                        " where ", LeftFieldName,
                        " between ", Parameter.Placeholder, " and ", Parameter.Placeholder);
                    scope.DBContext.Execute(sql, record.Left, record.Right);

                    //更新所有右边节点的 _left 与 _right
                    var sqlUpdate1 = String.Format(
                        "update {0} set _right = _right - {1} where _right > {2}",
                        tableModel.quotedTableName, width + 1, record.Right);

                    var sqlUpdate2 = String.Format(
                        "update {0} set _left = _left - {1} where _left > {2}",
                        tableModel.quotedTableName, width + 1, record.Left);

                    scope.DBContext.Execute(SqlString.Parse(sqlUpdate1));
                    scope.DBContext.Execute(SqlString.Parse(sqlUpdate2));
                }
            }
            else
            {
                var sql = new SqlString(
                    "delete from ", tableModel.quotedTableName,
                    " where ", QuotedIdColumn, " in (", ids.ToCommaList(), ")");

                var rowCount = scope.DBContext.Execute(sql);
                if (rowCount != ids.Count())
                {
                    var msg = string.Format("Failed to delete model '{0}'", tableModel.Name);
                    throw new ObjectServer.Exceptions.DataException(msg);
                }
            }
        }
    }
}
