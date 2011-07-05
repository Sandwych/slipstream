using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics;

namespace ObjectServer.Backend
{
    public sealed class DynamicRecord : DynamicObject
    {
        private Dictionary<string, object> columns;

        internal DynamicRecord(Dictionary<string, object> cols)
        {
            Debug.Assert(cols != null);
            this.columns = cols;
        }

        public object this[string name]
        {
            get
            {
                return this.columns[name];
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.columns.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            return this.columns.TryGetValue(binder.Name, out result);
        }

    }
}
