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
            return model.DefinedFields.ContainsKey(fieldName);
        }

        public static IEnumerable<IMetaField> GetAllStorableFields(this IModel model)
        {
            return model.DefinedFields.Values.Where(f => f.IsStorable() && f.Name != "id");
        }

    }
}
