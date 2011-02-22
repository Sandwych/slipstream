using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend.Postgresql
{
    internal sealed class ColumnMetadata
    {
        public string Name { get; set; }
        public bool Nullable { get; set; }
        public string SqlType { get; set; }
        public long Length { get; set; }
        public int Precision { get; set; }
    }
}
