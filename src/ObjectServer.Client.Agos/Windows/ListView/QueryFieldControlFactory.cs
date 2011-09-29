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
using System.Collections.Generic;
using System.Linq;

namespace ObjectServer.Client.Agos.Windows.ListView
{
    public class QueryFieldControlFactory : Malt.Layout.IWidgetFactory
    {
        private static readonly Dictionary<String, Type> fieldTypeControlMapping =
            new Dictionary<string, Type>()
            {
                { "boolean", typeof(QueryFieldControls.BooleanQueryFieldControl) },
                { "chars", typeof(QueryFieldControls.StringQueryFieldControl) },
                { "text", typeof(QueryFieldControls.StringQueryFieldControl) },
                { "enum", typeof(QueryFieldControls.EnumerationQueryFieldControl) },
                { "int32", typeof(QueryFieldControls.IntegerQueryFieldControl) },
                { "date", typeof(QueryFieldControls.DateQueryFieldControl) },
                { "datetime", typeof(QueryFieldControls.DateQueryFieldControl) },
            };

        private readonly IDictionary<string, object>[] metaFields;
        private readonly Dictionary<string, IQueryField> createdQueryFields =
            new Dictionary<string, IQueryField>();

        public QueryFieldControlFactory(IDictionary<string, object>[] fields)
        {
            this.metaFields = fields;
        }

        public object CreateFieldWidget(Malt.Layout.Models.Field field)
        {
            var metaField = this.metaFields.Where(i => (string)i["name"] == field.Name).Single();
            var fieldType = (string)metaField["type"];

            var fieldName = (string)metaField["name"];
            //TODO FUCK ME
            IQueryField queryField = null;
            if (fieldTypeControlMapping.ContainsKey(fieldType))
            {
                var t = fieldTypeControlMapping[fieldType];
                queryField = (IQueryField)Activator.CreateInstance(t, metaField);
            }
            else
            {
                queryField = new QueryFieldControls.StringQueryFieldControl(metaField);
            }

            this.createdQueryFields.Add(fieldName, queryField);
            return queryField;
        }

        public Malt.Layout.ITableLayoutWidget CreateTableLayoutWidget()
        {
            return new FormView.GridLayoutPanelWidget();
        }

        public object CreateLabelWidget(Malt.Layout.Models.Label label)
        {
            var labelControl = new Label() { Content = label.Text };
            return labelControl;
        }

        public object CreateHorizontalLineWidget(Malt.Layout.Models.HorizontalLine hl)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IQueryField> CreatedQueryField
        {
            get
            {
                return this.createdQueryFields;
            }
        }

    }
}
