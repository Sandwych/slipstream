using System;
using System.Collections.Generic;
using System.Text;

namespace Malt.Layout.Widgets
{
    public interface IFieldWidget
    {
        string FieldName { get; }
        object Value { get; set; }
        void Empty();
    }
}
