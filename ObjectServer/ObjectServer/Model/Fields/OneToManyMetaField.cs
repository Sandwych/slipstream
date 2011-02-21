using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class OneToManyMetaField : MetaField
    {
        public OneToManyMetaField(string name)
            : base(name, FieldType.OneToMany)
        {
        }

        public override Dictionary<long, object> GetFieldValues(
           ISession session, List<Dictionary<string, object>> records)
        {
            //查询字表
            var childModel = (TableModel)session.Pool.LookupObject(this.Relation);
            //TODO 权限等处理

            var children = new Dictionary<long, long[]>();
            foreach (var master in records)
            {
                var masterId = (long)master["id"];
                var domain = new List<object[]>();
                domain.Add(new object[] { this.RelatedField, "=", masterId });
                var childIds = childModel.Search(session, domain.ToArray(), 0, 0xffff);
                children[masterId] = childIds;
            }


            var result = new Dictionary<long, object>();
            foreach (var p in records)
            {
                var masterId = (long)p["id"];
                result.Add(masterId, children[masterId]);
            }

            return result;
        }
    }
}
