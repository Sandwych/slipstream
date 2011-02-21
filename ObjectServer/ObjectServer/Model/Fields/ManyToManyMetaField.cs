using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ManyToManyMetaField : MetaField
    {
        public ManyToManyMetaField(string name)
            : base(name, FieldType.ManyToMany)
        {
        }

        public override Dictionary<long, object> GetFieldValues(
           ISession session, List<Dictionary<string, object>> records)
        {

            throw new NotImplementedException();
        }
    }
}
