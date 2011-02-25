using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public static class MetaFieldExtensions
    {
        public static IMetaField SetLabel(this IMetaField f, string label)
        {
            f.Label = label;
            return f;
        }

        public static IMetaField SetRequired(this IMetaField f)
        {
            f.Required = true;
            return f;
        }

        public static IMetaField SetNotRequired(this IMetaField f)
        {
            f.Required = false;
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
