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
        Date,
        Time,
        Boolean,
        Decimal,
        Chars,
        Text,
        Binary,

        Enumeration,
        Reference,
        OneToMany,
        ManyToOne,
        ManyToMany,

    }
}
