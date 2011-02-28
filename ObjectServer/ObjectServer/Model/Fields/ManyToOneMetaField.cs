using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ManyToOneMetaField : MetaField
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="masterModel">主表对象</param>
        public ManyToOneMetaField(string name, string masterModel)
            : base(name, FieldType.ManyToOne)
        {
            this.Relation = masterModel;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IContext ctx, List<Dictionary<string, object>> records)
        {
            var result = new Dictionary<long, object>(records.Count());
            var masterModel = (TableModel)ctx.Database.ServiceObjects.Resolve(this.Relation);
            if (masterModel.ContainsField("name")) //如果有 name 字段
            {
                //从原始记录里把所有该字段的值取出
                var masterTableIds = (
                    from r in records
                    where r[this.Name] != null && !(r[this.Name] is DBNull)
                    select r[this.Name]).ToArray();

                if (masterTableIds.Length > 0)
                {
                    var masterNames = masterModel.NameGetter(ctx, masterTableIds);

                    foreach (var r in records)
                    {
                        var id = (long)r["id"];
                        var masterId = (long)r[this.Name];
                        result.Add(id, new RelatedField(masterId, masterNames[masterId]));
                    }
                }
                else
                {
                    foreach (var r in records)
                    {
                        var id = (long)r["id"];
                        result.Add(id, DBNull.Value);
                    }
                }
            }
            else
            {
                foreach (var r in records)
                {
                    var id = (long)r["id"];
                    var masterId = (long)r[this.Name];
                    result.Add(id, new RelatedField(masterId, string.Empty));
                }
            }



            return result;
        }
    }
}
