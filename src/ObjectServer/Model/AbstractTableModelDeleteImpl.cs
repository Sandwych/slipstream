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

        public override void DeleteInternal(IResourceScope scope, IEnumerable<long> ids)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids");
            }

            //继承的删除策略很简单：先删除本尊，再删除各个基类表
            List<Dictionary<string, object>> existedRecords = null;
            if (this.Inheritances.Count > 0)
            {
                var sql = string.Format(
                    "SELECT * FROM \"{0}\" WHERE \"id\" IN ({1})",
                    this.TableName,
                    ids.ToCommaList());
                existedRecords = scope.DatabaseProfile.DataContext.QueryAsDictionary(sql);
            }

            DoDelete(scope, ids, this);
            this.ProcessBaseModelsDeletion(scope, existedRecords);
        }

        private void ProcessBaseModelsDeletion(IResourceScope scope, List<Dictionary<string, object>> existedRecords)
        {

            if (this.Inheritances.Count > 0)
            {
                Debug.Assert(existedRecords != null);

                foreach (var inheritInfo in this.Inheritances)
                {
                    var baseIds = from r in existedRecords
                                  let f = r[inheritInfo.RelatedField]
                                  where f != DBNull.Value && f != null
                                  select (long)f;

                    if (baseIds.Count() > 0)
                    {
                        var baseModel = (AbstractTableModel)scope
                            .DatabaseProfile.GetResource(inheritInfo.BaseModel);
                        DoDelete(scope, baseIds, baseModel);
                    }
                }
            }
        }

        private static void DoDelete(IResourceScope scope, IEnumerable<long> ids, AbstractTableModel tableModel)
        {
            Debug.Assert(scope != null);
            Debug.Assert(ids != null);
            Debug.Assert(tableModel != null);

            var sql = string.Format(
                "DELETE FROM \"{0}\" WHERE \"id\" IN ({1})",
                tableModel.TableName, ids.ToCommaList());

            var rowCount = scope.DatabaseProfile.DataContext.Execute(sql);
            if (rowCount != ids.Count())
            {
                var msg = string.Format("Failed to delete model '{0}'", tableModel.Name);
                throw new DataException(msg);
            }
        }

    }
}
