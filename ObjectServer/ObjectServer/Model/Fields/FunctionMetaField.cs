using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class FunctionMetaField : MetaField
    {
        public FunctionMetaField(string name, FieldType ft, FieldGetter getter)
            : base(name, ft)
        {
            this.Getter = getter;
        }


        public override Dictionary<long, object> GetFieldValues(
            ISession session, List<Dictionary<string, object>> records)
        {
            var ids = records.Select(p => p["id"]).ToArray();

            var result = this.Getter(session, ids);

            return result;
        }

    }
}
