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
            this.DbType = ConvertPgTypeToDbType(sqlTypeStr);

            var charsMaxLength = row["character_maximum_length"];
            if (!charsMaxLength.IsNull()) {
                this.Length = Convert.ToInt32(charsMaxLength);
            }
        }


        public string Name { get; private set; }
        public bool Nullable { get; private set; }
        public DbType DbType { get; private set; }
        public int Length { get; private set; }
        public int Precision { get; private set; }

        private static DbType ConvertPgTypeToDbType(String pgTypeStr) {
            DbType sdt = DbType.Int32;
            switch (pgTypeStr) {
                case "boolean":
                    sdt = DbType.Boolean;
                    break;

                case "integer":
                    sdt = DbType.Int32;
                    break;

                case "bigint":
                    sdt = DbType.Int64;
                    break;

                case "timestamp without time zone":
                    sdt = DbType.DateTime;
                    break;

                case "date":
                    sdt = DbType.Date;
                    break;

                case "time":
                    sdt = DbType.Time;
                    break;

                case "float8":
                    sdt = DbType.Double;
                    break;

                case "decimal":
                    sdt = DbType.Decimal;
                    break;

                case "character varying":
                case "text":
                    sdt = DbType.String;
                    break;

                case "xml":
                    sdt = DbType.Xml;
                    break;

                case "bytea":
                    sdt = DbType.Binary;
                    break;

                default:
                    throw new NotSupportedException(
                        string.Format("Not supported PostgreSQL type: [{0}]", pgTypeStr));
            }
            return sdt;
        }
    }
}
