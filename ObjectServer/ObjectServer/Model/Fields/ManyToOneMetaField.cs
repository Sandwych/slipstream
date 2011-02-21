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
           ISession session, List<Dictionary<string, object>> records)
        {
            var result = new Dictionary<long, object>(records.Count());
            var masterModel = (TableModel)session.Pool.LookupObject(this.Relation);
            if (masterModel.ContainsField("name")) //如果有 name 字段
            {
                var masterTableIds = records.Select(d => d[this.Name]).ToArray();
                var masters = masterModel.Read(session, masterTableIds, new object[] { "name" });
                var masterNames = new Dictionary<long, string>(masters.Length);
                foreach (var master in masters)
                {
                    var masterId = (long)master["id"];
                    masterNames[masterId] = (string)master["name"];
                }
                foreach (var p in records)
                {
                    var id = (long)p["id"];
                    result.Add(id, new RelatedField(id, masterNames[id]));
                }
            }
            else
            {
                foreach (var p in records)
                {
                    var id = (long)p["id"];
                    result.Add(id, new RelatedField(id, string.Empty));
                }
            }

            return result;
        }
    }
}
