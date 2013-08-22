using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Layout.Models
{
    public interface IContainer : IPlacable
    {
        int ColumnCount { get; }

        IEnumerable<IPlacable> Children { get; }
    }
}
