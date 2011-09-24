using System;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;

using Malt.Layout.Widgets;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public class FieldControlFactory : Malt.Layout.IWidgetFactory
    {
        private readonly static IDictionary<String, Type> fieldTypeMapping =
            new Dictionary<String, Type>()
            {
                { "Chars", typeof(StringFieldControl) },
                { "Boolean", typeof(BooleanFieldControl) },
            };

        private IDictionary<string, object>[] fields;

        public FieldControlFactory(IDictionary<string, object>[] fields)
        {
            this.fields = fields;
        }

        /*

        IFieldWidget CreateFieldControl(string fieldType)
        {
        }
        */


        public object CreateFieldWidget(Malt.Layout.Models.Field field)
        {
            var metaField = this.fields.Where(i => (string)i["name"] == field.Name).Single();
            var fieldType = (string)metaField["type"];

            var t = fieldTypeMapping[fieldType];
            var widget = Activator.CreateInstance(t, field.Name) as IFieldWidget;
            return widget;
        }

        public Malt.Layout.ITableLayoutWidget CreateTableLayoutWidget()
        {
            return new GridLayoutPanelWidget();
        }

        public ILabelWidget CreateLabelWidget(Malt.Layout.Models.Label label)
        {
            var labelWidget = new FieldLabel();
            var metaField = this.fields.Where(i => (string)i["name"] == label.Field).Single();
            labelWidget.Text = metaField["label"] as string;

            return labelWidget;
        }

        public IHorizontalLineWidget CreateHorizontalLineWidget()
        {
            throw new NotImplementedException();
        }
    }
}
