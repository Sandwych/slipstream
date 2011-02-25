using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public static class MetaFieldExtensions
    {
        public static IMetaField SetRequired(this IMetaField f, bool required)
        {
            f.Required = required;
            return f;
        }

        public static IMetaField SetGetter(this IMetaField f, FieldGetter fieldGetter)
        {
            f.Getter = fieldGetter;
            return f;
        }

        public static IMetaField SetDefaultProc(this IMetaField f, FieldDefaultProc defaultProc)
        {
            f.DefaultProc = defaultProc;
            return f;
        }

        public static IMetaField SetSize(this IMetaField f, int size)
        {
            f.Size = size;
            return f;
        }

    }
}
