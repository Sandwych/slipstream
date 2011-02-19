using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Backend
{
    internal sealed class PgSqlTypeConverter : ISqlTypeConverter
    {

        private static readonly Dictionary<FieldType, Func<IField, string>> mapping =
            new Dictionary<FieldType, Func<IField, string>>()
            {
                { FieldType.Boolean, f => "boolean"  },
                { FieldType.Integer, f => "int4"  },
                { FieldType.BigInteger, f => "int8"  },
                { FieldType.DateTime, f => "timestamp" },
                { FieldType.Float, f => "float8" },
                { FieldType.Money, f => "money" },
                { FieldType.ManyToOne, f => "int8" },
                { FieldType.Chars, f => string.Format("varchar({0})", f.Size) },
            };



        #region ISqlTypeConverter 成员

        public string ToSqlType(IField field)
        {
            throw new NotImplementedException();
            //return mapping[field.fiel
        }

        #endregion

    }
}
