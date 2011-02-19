using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend.Postgresql
{
    internal sealed class Column
    {
        public string Name { get; set; }
        public bool NotNull { get; set; }
        public string SqlType { get; set; }
    }
}
