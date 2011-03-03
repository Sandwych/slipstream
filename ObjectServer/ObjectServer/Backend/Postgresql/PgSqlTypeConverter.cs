using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    internal sealed class PgSqlTypeConverter : ISqlTypeConverter
    {
        private PgSqlTypeConverter()
        {
        }

        private static readonly Dictionary<FieldType, Func<IMetaField, string>> mapping =
            new Dictionary<FieldType, Func<IMetaField, string>>()
            {
                { FieldType.Boolean, f => "BOOLEAN" },
                { FieldType.Integer, f => "INT4"  },
                { FieldType.BigInteger, f => "INT8"  },
                { FieldType.DateTime, f => "TIMESTAMP" },
                { FieldType.Float, f => "FLOAT8" },
                { FieldType.Money, f => "MONEY" },
                { FieldType.Text, f => "TEXT" },
                { FieldType.Binary, f =>  "BYTEA" },
                { FieldType.ManyToOne, f => "INT8" },
                { FieldType.Chars, f => f.Size > 0 ? string.Format("VARCHAR({0})", f.Size) : "VARCHAR" },
                { FieldType.Enumeration, f => string.Format("VARCHAR({0})", f.Size) },
            };

        #region ISqlTypeConverter 成员

        public string FieldToColumn(IMetaField field)
        {
            //mapping[
            throw new NotImplementedException();
        }

        #endregion


        public static string GetSqlType(IMetaField field)
        {
            Debug.Assert(field != null);

            return mapping[field.Type](field);
        }
    }
}
