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
    public abstract partial class AbstractSqlModel : AbstractModel
    {
        public override Dictionary<string, object>[] ReadInternal(
                 ITransactionContext scope, long[] ids, string[] requiredFields)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("ctx");
            }

            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            if (ids.Length == 0)
            {
                return new Dictionary<string, object>[] { };
            }

            //检查列是否有重复的

            if (ids == null || ids.Count() == 0)
            {
                return new Dictionary<string, object>[] { };
            }

            IList<string> allFields;
            if (requiredFields == null || requiredFields.Count() == 0)
            {
                allFields = this.Fields.Select(p => p.Value.Name).ToList();
            }
            else
            {
                //TODO 检查是否有不存在的列

                var userFields = requiredFields.Select(o => (string)o).ToArray();
                //检查重复的列
                var distinctedFields = userFields.Distinct();

                if (userFields.Length != distinctedFields.Count())
                {
                    throw new ArgumentException("包含重复的字段名", "requiredFields");
                }

                allFields = userFields.ToList();
            }

            if (!allFields.Contains(IdFieldName))
            {
                allFields.Add(IdFieldName);
            }

            //表里的列，也就是可以直接用 SQL 查的列
            var columnFields = from f in allFields
                               let fieldInfo = this.Fields[f]
                               where fieldInfo.IsColumn  //TODO 这里可能要重新考虑下
                               select f;

            columnFields = columnFields.Union(this.Inheritances.Select(i => i.RelatedField));

            var selectStmt = new SqlStringBuilder();
            selectStmt.Add("select ");

            bool commaNeeded = false;
            foreach (var col in columnFields)
            {
                if (commaNeeded)
                {
                    selectStmt.Add(",");
                }
                commaNeeded = true;

                var quotedColumn = DataProvider.Dialect.QuoteForColumnName(col);
                selectStmt.Add(quotedColumn);
            }

            selectStmt.Add(" from ");
            selectStmt.Add(this.TableName);
            var idColumn = DataProvider.Dialect.QuoteForColumnName(AbstractModel.IdFieldName);
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
            ITransactionContext scope, IList<string> allFields, IList<Dictionary<string, object>> records)
        {
            Debug.Assert(scope != null);
            Debug.Assert(allFields != null);
            Debug.Assert(records != null);

            foreach (var fieldName in allFields)
            {
                var f = this.Fields[fieldName];
                if (f.Name == IdFieldName
                    || f.Name == LeftFieldName
                    || f.Name == RightFieldName)
                {
                    continue;
                }

                var fieldValues = f.GetFieldValues(scope, records);
                foreach (var record in records)
                {
                    var id = (long)record[IdFieldName];
                    record[f.Name] = fieldValues[id];
                }
            }
        }

        private void ReadBaseModels(
            ITransactionContext scope, IList<string> allFields, Dictionary<string, object>[] records)
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
