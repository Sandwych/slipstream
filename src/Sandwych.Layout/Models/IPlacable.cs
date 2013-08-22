using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Layout
{
    public interface IPlacable
    {
        int RowSpan { get; }

        int ColumnSpan { get; }

        bool Fill { get; }
    }
}
