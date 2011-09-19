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
        public override Dictionary<string, object>[] ReadInternal(
                 IServiceContext scope, long[] ids, string[] requiredFields = null)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("ctx");
            }

            this.VerifyReadPermission(scope);

            if (ids == null || ids.Count() == 0)
            {
                return new Dictionary<string, object>[] { };
            }

            IList<string> allFields;
            if (requiredFields == null || requiredFields.Count() == 0)
            {
                allFields = this.Fields.Where(p => !p.Value.Lazy)
                    .Select(p => p.Value.Name).ToList();
            }
            else
            {
                //检查是否有不存在的列
                var userFields = requiredFields.Select(o => (string)o);
                allFields = userFields.ToList();
            }

            if (!allFields.Contains(IDFieldName))
            {
                allFields.Add(IDFieldName);
            }

            //表里的列，也就是可以直接用 SQL 查的列
            var columnFields = from f in allFields
                               where this.Fields[f].IsColumn()
                               select f;

            columnFields = columnFields.Union(this.Inheritances.Select(i => i.RelatedField));

            var selectStmt = new SqlStringBuilder();
            selectStmt.Add("select ");

            bool commaNeeded = false;
            foreach (var col in columnFields)
            {
                if(commaNeeded)
                {
                    selectStmt.Add(",");        
                }
                commaNeeded = true;

                var quotedColumn = DataProvider.Dialect.QuoteForColumnName(col);
                selectStmt.Add(quotedColumn);
            }

            selectStmt.Add(" from ");
            selectStmt.Add(this.TableName);
            var idColumn = DataProvider.Dialect.QuoteForColumnName(AbstractModel.IDFieldName);
            selectStmt.Add(" where " + idColumn + " in (");

            commaNeeded = false;
            foreach (var id in ids)
            {
                if (commaNeeded)
                {
                    selectStmt.Add(",");
                }
                commaNeeded = true;

                selectStmt.Add(id.ToString());
            }

            selectStmt.Add(")");

            var sql = selectStmt.ToSqlString();

            //先查找表里的简单字段数据
            var records = scope.DBContext.QueryAsDictionary(sql);

            this.ReadBaseModels(scope, allFields, records);

            this.PostProcessFieldValues(scope, allFields, records);

            return records.ToArray();
        }

        private void PostProcessFieldValues(
            IServiceContext scope, IList<string> allFields, IList<Dictionary<string, object>> records)
        {
            Debug.Assert(scope != null);
            Debug.Assert(allFields != null);
            Debug.Assert(records != null);

            foreach (var fieldName in allFields)
            {
                var f = this.Fields[fieldName];
                if (f.Name == IDFieldName)
                {
                    continue;
                }

                var fieldValues = f.GetFieldValues(scope, records);
                foreach (var record in records)
                {
                    var id = (long)record[IDFieldName];
                    record[f.Name] = fieldValues[id];
                }
            }
        }

        private void ReadBaseModels(
            IServiceContext scope, IList<string> allFields, Dictionary<string, object>[] records)
        {
            Debug.Assert(scope != null);
            Debug.Assert(allFields != null);
            Debug.Assert(records != null);

            //本尊及各个关联到基类模型的字段已经读出来了，现在读各个基类模型
            foreach (var bm in this.Inheritances)
            {
                var baseModel = (IModel)scope.GetResource(bm.BaseModel);
                var baseFieldsToRead = allFields.Intersect(baseModel.Fields.Keys).ToArray();
                var baseIds = records.Select(r => (long)r[bm.RelatedField]).ToArray();
                var baseRecords = baseModel.ReadInternal(scope, baseIds, baseFieldsToRead);
                //合并到结果中
                for (int i = 0; i < baseRecords.Length; i++)
                {
                    foreach (var baseField in baseRecords[i])
                    {
                        if (!records[i].ContainsKey(baseField.Key))
                        {
                            records[i].Add(baseField.Key, baseField.Value);
                        }
                    }
                }
            }
        }


    } //class
}
