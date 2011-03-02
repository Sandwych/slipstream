using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public enum FieldType
    {
        Integer,
        BigInteger,
        Float,
        DateTime,
        Boolean,
        Decimal,
        Money,
        Chars,
        Text,
        Binary,

        Enumeration,
        Reference,
        Property,
        OneToMany,
        ManyToOne,
        ManyToMany,
    }
}
