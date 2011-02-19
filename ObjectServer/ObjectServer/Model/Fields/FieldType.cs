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
        Money,
        Chars,
        Text,
        Binary,

        Selection,
        Reference,
        Property,
        OneToMany,
        ManyToOne,
        ManyToMany,
    }
}
