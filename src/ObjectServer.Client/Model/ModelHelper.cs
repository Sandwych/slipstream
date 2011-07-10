using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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
