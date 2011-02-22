using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Backend.Postgresql
{
    internal interface IColumnMetadata
    {
        string Name { get; }

        bool Nullable { get; }

        string SqlType { get; }

        long Length { get; }

        int Precision { get; }
    }
}
