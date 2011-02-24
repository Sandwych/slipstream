using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ManyToOneMetaField : MetaField
    {
        public ManyToOneMetaField(string name)
            : base(name, FieldType.ManyToOne)
        {
        }

        public override Dictionary<long, object> GetFieldValues(
           ICallingContext session, List<Dictionary<string, object>> records)
        {
            var result = new Dictionary<long, object>(records.Count());
            var masterModel = (TableModel)session.Pool[this.Relation];
            if (masterModel.ContainsField("name")) //如果有 name 字段
            {
                //从原始记录里把所有该字段的值取出
                var masterTableIds = (
                    from r in records
                    where r[this.Name] != null && !(r[this.Name] is DBNull)
                    select r[this.Name]).ToArray();

                if (masterTableIds.Length > 0)
                {
                    var masterNames = masterModel.NameGetter(session, masterTableIds);

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
