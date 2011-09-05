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

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        private static readonly string[] HierarchyFields =
            new string[] { IDFieldName, LeftFieldName, RightFieldName };

        public override void DeleteInternal(IServiceScope scope, long[] ids)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            if (!scope.CanDeleteModel(scope.Session.UserId, this.Name))
            {
                throw new UnauthorizedAccessException("Access denied");
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

        private void ProcessBaseModelsDeletion(IServiceScope scope, IEnumerable<Dictionary<string, object>> existedRecords)
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
            IServiceScope scope, IEnumerable<long> ids, AbstractTableModel tableModel)
        {
            Debug.Assert(scope != null);
            Debug.Assert(ids != null);
            Debug.Assert(tableModel != null);

            if (tableModel.Hierarchy)
            {
                //TODO 做一下化简，如果提供的ID已经是其他ID的子节点了就去掉
                var records = tableModel.ReadInternal(scope, ids.ToArray(), HierarchyFields);
                foreach (var record in records)
                {
                    var left = (long)record[LeftFieldName];
                    var right = (long)record[RightFieldName];
                    var width = right - left;
                    //删除指定ID节点及其下面包含的子孙节点
                    var sql = new SqlString(
                        "delete from ", tableModel.quotedTableName,
                        " where ", LeftFieldName,
                        " between ", Parameter.Placeholder, " and ", Parameter.Placeholder);
                    scope.DBContext.Execute(sql, left, right);

                    //更新所有右边节点的 _left 与 _right
                    var sqlUpdate1 = String.Format(
                        "update {0} set _right = _right - {1} where _right > {2}",
                        tableModel.quotedTableName, width, right);

                    var sqlUpdate2 = String.Format(
                        "update {0} set _left = _left - {1} where _left > {2}",
                        tableModel.quotedTableName, width, right);

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
                    throw new DataException(msg);
                }
            }
        }
    }
}
