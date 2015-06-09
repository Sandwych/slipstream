using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sandwych;

namespace SlipStream.Data.Postgresql {
    internal sealed class PgColumnMetadata : IColumnMetadata {
        public PgColumnMetadata(IDictionary<string, object> row) {
            this.Name = (string)row["column_name"];
            this.Nullable = (string)row["is_nullable"] == "YES";
            var sqlTypeStr = (string)row["data_type"];
            this.SqlType = ConvertPgTypeToSqlDbType(sqlTypeStr);

            var charsMaxLength = row["character_maximum_length"];
            if (!charsMaxLength.IsNull()) {
                this.Length = Convert.ToInt32(charsMaxLength);
            }
        }


        public string Name { get; private set; }
        public bool Nullable { get; private set; }
        public SqlDbType SqlType { get; private set; }
        public int Length { get; private set; }
        public int Precision { get; private set; }

        private static SqlDbType ConvertPgTypeToSqlDbType(String pgTypeStr) {
            /*
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
        */

            SqlDbType sdt = SqlDbType.UniqueIdentifier;
            switch (pgTypeStr) {
                case "boolean":
                    sdt = SqlDbType.Bit;
                    break;

                case "integer":
                    sdt = SqlDbType.Int;
                    break;

                case "bigint":
                    sdt = SqlDbType.BigInt;
                    break;

                case "timestamp without time zone":
                    sdt = SqlDbType.DateTime;
                    break;

                case "date":
                    sdt = SqlDbType.Date;
                    break;

                case "time":
                    sdt = SqlDbType.Time;
                    break;

                case "float8":
                    sdt = SqlDbType.Real;
                    break;

                case "decimal":
                    sdt = SqlDbType.Decimal;
                    break;

                case "character varying":
                    sdt = SqlDbType.VarChar;
                    break;

                case "text":
                    sdt = SqlDbType.Text;
                    break;

                case "xml":
                    sdt = SqlDbType.Xml;
                    break;

                case "bytea":
                    sdt = SqlDbType.VarBinary;
                    break;

                default:
                    throw new NotSupportedException(
                        string.Format("Not supported PostgreSQL type: [{0}]", pgTypeStr));
            }
            return sdt;
        }
    }
}
