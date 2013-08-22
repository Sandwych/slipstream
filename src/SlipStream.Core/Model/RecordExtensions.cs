using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Model
{
    internal static class RecordExtensions
    {
        public static Dictionary<long, object> ExtractFieldValues(
            this ICollection<Dictionary<string, object>> records, string field)
        {
            var result = new Dictionary<long, object>(records.Count);
            foreach (var r in records)
            {
                var id = (long)r[AbstractModel.IdFieldName];
                var fieldValue = r[field];
                result.Add(id, fieldValue);
            }

            return result;
        }
    }
}
