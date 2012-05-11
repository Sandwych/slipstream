using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Malt;

namespace ObjectServer.Data.Mssql
{
    internal sealed class MssqlColumnMetadata : IColumnMetadata
    {
        public MssqlColumnMetadata(IDictionary<string, object> row)
        {
            this.Name = (string)row["column_name"];
            this.Nullable = (string)row["is_nullable"] == "YES";
            var sqlTypeStr = (string)row["data_type"];
            this.SqlType = (SqlDbType)Enum.Parse(typeof(SqlDbType), sqlTypeStr, true);

            var charsMaxLength = row["character_maximum_length"];
            if (!charsMaxLength.IsNull())
            {
                this.Length = Convert.ToInt32(charsMaxLength);
            }
        }


        public string Name { get; private set; }
        public bool Nullable { get; private set; }
        public SqlDbType SqlType { get; private set; }
        public int Length { get; private set; }
        public int Precision { get; private set; }
    }
}
