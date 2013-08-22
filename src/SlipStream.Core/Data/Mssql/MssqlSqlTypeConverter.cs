using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using SlipStream.Model;

namespace SlipStream.Data.Mssql
{
    internal sealed class MssqlSqlTypeConverter
    {
        private MssqlSqlTypeConverter()
        {
        }

        private static readonly Dictionary<FieldType, Func<IFieldDescriptor, string>> mapping =
            new Dictionary<FieldType, Func<IFieldDescriptor, string>>()
            {
                { FieldType.Identifier, f => "bigint identity(1,1) primary key" },
                { FieldType.Boolean, f => "bit" },
                { FieldType.Integer, f => "int"  },
                { FieldType.BigInteger, f => "bigint"  },
                { FieldType.DateTime, f => "datetime" },
                { FieldType.Date, f => "date" },
                { FieldType.Time, f => "time" },
                { FieldType.Double, f => "float" },
                { FieldType.Decimal, f => "decimal" },
                { FieldType.Text, f => "ntext" },
                { FieldType.Binary, f =>  "varbinary(max)" },
                { FieldType.ManyToOne, f => "bigint" },
                { FieldType.Chars, f => f.Size > 0 ? string.Format("nvarchar({0})", f.Size) : "nvarchar(max)" },
                { FieldType.Enumeration, f => string.Format("nvarchar({0})", f.Size) },
                { FieldType.Reference, f => "nvarchar(128)" },
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
                sb.Append(" not null");
            }
            else
            {
                sb.Append(" null");
            }

            if (field.IsUnique)
            {
                sb.Append(" unique");
            }
            return sb.ToString();
        }
    }
}
