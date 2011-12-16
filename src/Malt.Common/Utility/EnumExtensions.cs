using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Malt.Utility
{
    public static class EnumExtensions<T>
    {
        private readonly static Dictionary<string, T> parseMapping = new Dictionary<string, T>();
        private readonly static Dictionary<T, string> toStringMapping = new Dictionary<T, string>();

        static EnumExtensions()
        {
            foreach (var fi in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attrs = fi.GetCustomAttributes(false);
                var fta = (EnumStringValueAttribute)attrs.Where(
                    a => a is EnumStringValueAttribute).SingleOrDefault();
                if (fta != null)
                {
                    var itemValue = (T)fi.GetValue(null);
                    parseMapping.Add(fta.StringValue, (T)fi.GetValue(null));
                    toStringMapping.Add((T)itemValue, fta.StringValue);
                }
            }
        }

        public static string ToKeyString(T ft)
        {
            return toStringMapping[ft];
        }

        public static T Parse(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            return parseMapping[key];
        }
    }
}
