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

namespace ObjectServer.Client.Agos.Utility
{
    public static class XamlLoader
    {
        public static object LoadFromXaml(Uri uri)
        {
            var streamInfo = System.Windows.Application.GetResourceStream(uri);

            if ((streamInfo != null) && (streamInfo.Stream != null))
            {
                using (var reader = new System.IO.StreamReader(streamInfo.Stream))
                {
                    return System.Windows.Markup.XamlReader.Load(reader.ReadToEnd());
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Note that in the XAML you must declare a default XML namespace as 
        /// "xmlns='http://schemas.microsoft.com/client/2007'"</remarks>
        /// <param name="xamlControl"></param>
        /// <returns></returns>
        public static object LoadFromXamlString(string xamlControl)
        {
            return System.Windows.Markup.XamlReader.Load(xamlControl);
        }
    }
}
