using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace SlipStream.Data
{
    public interface IColumnMetadata
    {
        string Name { get; }

        bool Nullable { get; }

        DbType DbType { get; }

        int Length { get; }

        int Precision { get; }
    }
}
