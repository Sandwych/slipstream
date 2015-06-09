using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace SlipStream.Data {
    public sealed class SqlCommandParameter {

        public SqlCommandParameter(DbType type, object value) {
            this.Type = type;
            this.Value = value;
        }

        public DbType Type { get; private set; }
        public object Value { get; private set; }
    }
}
