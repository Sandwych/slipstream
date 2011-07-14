using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    internal sealed class PgSqlTypeConverter
    {
        private PgSqlTypeConverter()
        {
        }

        private static readonly Dictionary<FieldType, Func<IField, string>> mapping =
            new Dictionary<FieldType, Func<IField, string>>()
            {
                { FieldType.Boolean, f => "BOOLEAN" },
                { FieldType.Integer, f => "INT4"  },
                { FieldType.BigInteger, f => "INT8"  },
                { FieldType.DateTime, f => "TIMESTAMP" },
                { FieldType.Float, f => "FLOAT8" },
                { FieldType.Decimal, f => "DECIMAL" },
                { FieldType.Text, f => "TEXT" },
                { FieldType.Binary, f =>  "BYTEA" },
                { FieldType.ManyToOne, f => "INT8" },
                { FieldType.Chars, f => f.Size > 0 ? string.Format("VARCHAR({0})", f.Size) : "VARCHAR" },
                { FieldType.Enumeration, f => string.Format("VARCHAR({0})", f.Size) },
                { FieldType.Reference, f => "VARCHAR(128)" },
            };

        public static string GetSqlType(IField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            var func = mapping[field.Type];

            return func(field);
        }
    }
}
