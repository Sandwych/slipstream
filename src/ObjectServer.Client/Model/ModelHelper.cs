using System;
using System.Net;

namespace ObjectServer.Client.Model
{
    public static class ModelHelper
    {
        public static Tuple<string, long> ConvertReferencedField(object rawField)
        {
            if (rawField == null)
            {
                throw new ArgumentNullException("rawField");
            }

            var action = (object[])rawField;
            return new Tuple<string, long>((string)action[0], (long)action[1]);
        }
    }
}
