using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public enum FieldType
    {
        Characters,
        Text,
        Int,
        Long,
        SmallInt,
        Boolean,
        Selection,
        OneToMany,
        ManyToOne,
        ManyToMany,
    }
}
