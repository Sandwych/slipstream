using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Malt.Utility
{

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumStringValueAttribute : Attribute
    {
        public EnumStringValueAttribute(string value)
        {
            this.StringValue = value;
        }

        public string StringValue { get; private set; }
    }
}
