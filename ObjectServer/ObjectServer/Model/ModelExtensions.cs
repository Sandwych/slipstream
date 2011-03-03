using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal static class ModelExtensions
    {
        public static bool ContainsField(this IModel model, string fieldName)
        {
            return model.Fields.ContainsKey(fieldName);
        }

        public static IEnumerable<IMetaField> GetAllStorableFields(this IModel model)
        {
            return model.Fields.Values.Where(f => f.IsColumn() && f.Name != "id");
        }

    }
}
