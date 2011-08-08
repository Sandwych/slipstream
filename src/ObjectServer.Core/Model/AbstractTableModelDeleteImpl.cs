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

        public override void DeleteInternal(IServiceScope scope, long[] ids)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids");
            }

            if (!scope.CanDeleteModel(scope.Session.UserId, this.Name))
            {
                throw new UnauthorizedAccessException("Access denied");
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

            DoDelete(scope, ids, this);
            this.ProcessBaseModelsDeletion(scope, existedRecords);
        }

        private void ProcessBaseModelsDeletion(IServiceScope scope, IEnumerable<Dictionary<string, object>> existedRecords)
        {

            if (this.Inheritances.Count > 0)
            {
                Debug.Assert(existedRecords != null);

                foreach (var inheritInfo in this.Inheritances)
                {
                    var baseIds = from r in existedRecords
                                  let f = r[inheritInfo.RelatedField]
                                  where !f.IsNull() select (long)f;

                    if (baseIds.Count() > 0)
                    {
                        var baseModel = (AbstractTableModel)scope
                            .GetResource(inheritInfo.BaseModel);
                        DoDelete(scope, baseIds, baseModel);
                    }
                }
            }
        }

        private static void DoDelete(IServiceScope scope, IEnumerable<long> ids, AbstractTableModel tableModel)
        {
            Debug.Assert(scope != null);
            Debug.Assert(ids != null);
            Debug.Assert(tableModel != null);

            var sql = new SqlString(
                "delete from ",
                tableModel.quotedTableName,
                " where ",
                QuotedIdColumn,
                " in (",
                ids.ToCommaList(),
                ")");

            var rowCount = scope.DBContext.Execute(sql);
            if (rowCount != ids.Count())
            {
                var msg = string.Format("Failed to delete model '{0}'", tableModel.Name);
                throw new DataException(msg);
            }
        }

    }
}
