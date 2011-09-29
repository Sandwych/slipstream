using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectServer.Client.Model
{
    public abstract class AbstractModel
    {

        protected static void RegisterAllFields(IDictionary<string, PropertyInfo> props, Type selfType)
        {
            foreach (var pi in selfType.GetProperties())
            {
                var fas =
                    from a in pi.GetCustomAttributes(true)
                    where a is FieldAttribute
                    select (FieldAttribute)a;
                var fa = fas.SingleOrDefault();
                if (fa != null)
                {
                    props.Add(fa.Name, pi);
                }
            }
        }

        protected static void SetPropertiesByRecord(
            object self,
            IDictionary<string, PropertyInfo> props,
            IDictionary<string, object> record)
        {
            foreach (var p in record)
            {
                var pi = props[p.Key];
                pi.SetValue(self, p.Value, null);
            }
        }

    }
}
