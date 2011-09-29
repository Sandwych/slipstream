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
using System.Windows.Data;
using System.Linq;
using System.Collections.Generic;

using ObjectServer.Client.Agos.Windows.ListView.ValueConverters;

namespace ObjectServer.Client.Agos.Controls.Tree
{
    public class TreeDataGrid : DataGrid
    {
        private static Dictionary<string, Tuple<Type, IValueConverter>> COLUMN_TYPE_MAPPING
            = new Dictionary<string, Tuple<Type, IValueConverter>>()
        {
            {"id", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"int32", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"float8", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"decimal", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"chars", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"text", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"boolean", new Tuple<Type, IValueConverter>(typeof(DataGridCheckBoxColumn), null) },
            {"datetime", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), null) },
            {"date", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new DateFieldConverter()) },
            {"time", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new TimeFieldConverter()) },
            {"many-to-one", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new ManyToOneFieldConverter()) },
            {"enum", new Tuple<Type, IValueConverter>(typeof(DataGridTextColumn), new EnumFieldConverter()) },
        };
    }
}
