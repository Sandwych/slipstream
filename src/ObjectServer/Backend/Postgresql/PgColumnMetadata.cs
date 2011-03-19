using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend.Postgresql
{
    internal sealed class PgColumnMetadata : IColumnMetadata
    {
        public PgColumnMetadata(IDictionary<string, object> row)
        {
            Name = (string)row["column_name"];
            Nullable = (string)row["is_nullable"] == "YES";
            SqlType = (string)row["data_type"];
        }


        public string Name { get; private set; }
        public bool Nullable { get; private set; }
        public string SqlType { get; private set; }
        public long Length { get; private set; }
        public int Precision { get; private set; }
    }
}
