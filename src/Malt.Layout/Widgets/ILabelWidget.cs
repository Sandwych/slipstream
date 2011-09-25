using System;
using System.Collections.Generic;
using System.Text;

namespace Malt.Layout.Widgets
{
    public interface ILabelWidget
    {
        string Text { get; set; }
        string Field { get; }
    }
}
