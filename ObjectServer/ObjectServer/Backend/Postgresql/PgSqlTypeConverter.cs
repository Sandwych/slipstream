using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    public class PgSqlTypeConverter : ISqlTypeConverter
    {

        private static readonly Dictionary<FieldType, Func<IField, string>> mapping =
            new Dictionary<FieldType, Func<IField, string>>()
            {
                { FieldType.Boolean, f => "boolean"  },
                { FieldType.Int, f => "integer"  },
                { FieldType.Long, f => "bigint"  },
            };



        #region ISqlTypeConverter 成员

        public string ToSqlType(ObjectServer.Model.IField field)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
