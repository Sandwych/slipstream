using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Model;

namespace ObjectServer.Data
{
    internal sealed class PgSqlTypeConverter
    {
        private PgSqlTypeConverter()
        {
        }

        private static readonly Dictionary<FieldType, Func<IField, string>> mapping =
            new Dictionary<FieldType, Func<IField, string>>()
            {
                { FieldType.Boolean, f => "boolean" },
                { FieldType.Integer, f => "int4"  },
                { FieldType.BigInteger, f => "int8"  },
                { FieldType.DateTime, f => "timestamp" },
                { FieldType.Date, f => "date" },
                { FieldType.Time, f => "time" },
                { FieldType.Float, f => "float8" },
                { FieldType.Decimal, f => "decimal" },
                { FieldType.Text, f => "text" },
                { FieldType.Binary, f =>  "bytea" },
                { FieldType.ManyToOne, f => "int8" },
                { FieldType.Chars, f => f.Size > 0 ? string.Format("varchar({0})", f.Size) : "varchar" },
                { FieldType.Enumeration, f => string.Format("varchar({0})", f.Size) },
                { FieldType.Reference, f => "varchar(128)" },
            };

        public static string GetSqlType(IField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            var func = mapping[field.Type];

            var sqlTypeStr = func(field);
            if (field.IsRequired)
            {
                sqlTypeStr = sqlTypeStr + ' ' + "not null";
            }
            return sqlTypeStr;
        }
    }
}
