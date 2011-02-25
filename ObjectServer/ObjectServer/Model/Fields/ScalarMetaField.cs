using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ScalarMetaField : MetaField
    {
        public ScalarMetaField(string name, FieldType ft)
            : base(name, ft)
        {
        }

    
        protected override Dictionary<long, object> OnGetFieldValues(
            ICallingContext session, List<Dictionary<string, object>> records)
        {
            var result = new Dictionary<long, object>(records.Count());

            foreach (var r in records)
            {
                result[(long)r["id"]] = r[this.Name];
            }

            return result;
        }

    }
}
