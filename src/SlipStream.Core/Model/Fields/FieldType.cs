using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Sandwych.Utility;

namespace SlipStream.Model
{
    public enum FieldType
    {
        [EnumStringValue("id")]
        Identifier,

        [EnumStringValue("int32")]
        Integer,

        [EnumStringValue("int64")]
        BigInteger,

        [EnumStringValue("double")]
        Double,

        [EnumStringValue("datetime")]
        DateTime,

        [EnumStringValue("date")]
        Date,

        [EnumStringValue("time")]
        Time,

        [EnumStringValue("boolean")]
        Boolean,

        [EnumStringValue("decimal")]
        Decimal,

        [EnumStringValue("chars")]
        Chars,

        [EnumStringValue("text")]
        Text,

        [EnumStringValue("binary")]
        Binary,

        [EnumStringValue("enum")]
        Enumeration,

        [EnumStringValue("reference")]
        Reference,

        [EnumStringValue("one-to-many")]
        OneToMany,

        [EnumStringValue("many-to-one")]
        ManyToOne,

        [EnumStringValue("many-to-many")]
        ManyToMany,
    }

    public static class FieldTypeExtensions
    {
        public static string ToKeyString(this FieldType ft)
        {
            return EnumExtensions<FieldType>.ToKeyString(ft);
        }

        public static FieldType ParseAsFieldType(string ft)
        {
            return EnumExtensions<FieldType>.Parse(ft);
        }
    }
}

