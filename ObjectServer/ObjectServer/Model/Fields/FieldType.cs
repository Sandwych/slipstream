using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public enum FieldType
    {
        Chars,
        Text,
        Integer,
        BigInteger,
        Float,
        DateTime,
        Boolean,
        Money,
        Selection,
        OneToMany,
        ManyToOne,
        ManyToMany,
    }
}
