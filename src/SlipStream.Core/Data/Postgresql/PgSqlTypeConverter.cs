using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlipStream.Entity;

namespace SlipStream.Data
{
    internal sealed class PgSqlTypeConverter
    {
        private PgSqlTypeConverter()
        {
        }

        private static readonly Dictionary<FieldType, Func<IFieldDescriptor, string>> mapping =
            new Dictionary<FieldType, Func<IFieldDescriptor, string>>()
            {
                { FieldType.Identifier, f => "bigserial not null primary key" },
                { FieldType.Boolean, f => "boolean" },
                { FieldType.Integer, f => "integer"  },
                { FieldType.BigInteger, f => "bigint"  },
                { FieldType.DateTime, f => "timestamp" },
                { FieldType.Date, f => "date" },
                { FieldType.Time, f => "time" },
                { FieldType.Double, f => "float8" },
                { FieldType.Decimal, f => "decimal" },
                { FieldType.Text, f => "text" },
                { FieldType.Xml, f => "xml" },
                { FieldType.Binary, f =>  "bytea" },
                { FieldType.ManyToOne, f => "int8" },
                { FieldType.Chars, f => f.Size > 0 ? string.Format("varchar({0})", f.Size) : "varchar" },
                { FieldType.Enumeration, f => string.Format("varchar({0})", f.Size) },
                { FieldType.Reference, f => "varchar(128)" },
            };

        public static string GetSqlType(IFieldDescriptor field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }

            var func = mapping[field.Type];

            var sb = new StringBuilder();
            sb.Append(func(field));
            if (field.IsRequired)
            {
                sb.Append(' ');
                sb.Append("not null");
            }

            if (field.IsUnique)
            {
                sb.Append(' ');
                sb.Append("unique");
            }
            return sb.ToString();
        }
    }
}
