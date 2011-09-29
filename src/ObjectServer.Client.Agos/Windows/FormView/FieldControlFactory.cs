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

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    public class FieldControlFactory : Malt.Layout.IWidgetFactory
    {
        private readonly static IDictionary<String, Type> fieldTypeMapping =
            new Dictionary<String, Type>()
            {
                { "Chars", typeof(CharsFieldControl) },
                { "Boolean", typeof(BooleanFieldControl) },
                { "Enumeration", typeof(EnumerationFieldControl) },
                { "Text", typeof(TextFieldControl) },
                { "Integer", typeof(IntegerFieldControl) },
                { "ManyToOne", typeof(ManyToOneFieldControl) },
                { "OneToMany", typeof(OneToManyFieldControl) },
                { "ManyToMany", typeof(ManyToManyFieldControl) },
                { "Reference", typeof(ReferenceFieldControl) },
                { "DateTime", typeof(DateTimeFieldControl) },
                { "Date", typeof(DateFieldControl) },
                { "Float", typeof(FloatFieldControl) },
                { "Decimal", typeof(DecimalFieldControl) },
            };

        private IDictionary<string, object>[] metaFields;
        private IDictionary<string, IFieldWidget> createdFieldWidgets =
            new Dictionary<string, IFieldWidget>();
        private List<Label> createdLabels = new List<Label>();

        public FieldControlFactory(IDictionary<string, object>[] fields)
        {
            this.metaFields = fields;
        }

        public object CreateFieldWidget(Malt.Layout.Models.Field field)
        {
            var metaField = this.metaFields.Where(i => (string)i["name"] == field.Name).Single();
            var fieldType = (string)metaField["type"];

            var t = fieldTypeMapping[fieldType];
            var widget = (IFieldWidget)Activator.CreateInstance(t, metaField);
            this.createdFieldWidgets.Add(field.Name, widget);
            return widget;
        }

        public Malt.Layout.ITableLayoutWidget CreateTableLayoutWidget()
        {
            return new GridLayoutPanelWidget();
        }

        public Object CreateLabelWidget(Malt.Layout.Models.Label label)
        {
            var labelWidget = new FieldLabel(label.Field, label.Text);
            if (!String.IsNullOrEmpty(label.Field))
            {
                var metaField = this.metaFields.Where(i => (string)i["name"] == label.Field).Single();
                labelWidget.Text = metaField["label"] as string;
            }
            this.createdLabels.Add(labelWidget);

            return labelWidget;
        }

        public object CreateHorizontalLineWidget(Malt.Layout.Models.HorizontalLine hl)
        {
            var hlWidget = new HLine();
            hlWidget.Text = hl.Text;
            return hlWidget;
        }

        public IDictionary<string, IFieldWidget> CreatedFieldWidgets
        {
            get { return this.createdFieldWidgets; }
        }


    }
}
