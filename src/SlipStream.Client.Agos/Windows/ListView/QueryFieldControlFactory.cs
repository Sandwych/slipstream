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

namespace SlipStream.Client.Agos.Windows.TreeView
{
    public class QueryFieldControlFactory : Sandwych.Layout.IWidgetFactory
    {
        private static readonly Dictionary<String, Type> fieldTypeControlMapping =
            new Dictionary<string, Type>()
            {
                { "boolean", typeof(QueryFieldControls.BooleanQueryFieldControl) },
                { "chars", typeof(QueryFieldControls.StringQueryFieldControl) },
                { "text", typeof(QueryFieldControls.StringQueryFieldControl) },
                { "enum", typeof(QueryFieldControls.EnumerationQueryFieldControl) },
                { "int32", typeof(QueryFieldControls.Int32QueryFieldControl) },
                { "decimal", typeof(QueryFieldControls.DecimalQueryFieldControl) },
                { "double", typeof(QueryFieldControls.FloatQueryFieldControl) },
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

        public object CreateInputWidget(Sandwych.Layout.Models.Input field)
        {
            var metaField = this.metaFields.Where(i => (string)i["name"] == field.Field).Single();
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

        public Sandwych.Layout.ITableLayoutWidget CreateTableLayoutWidget(Sandwych.Layout.Models.IContainer container)
        {
            return new FormView.GridLayoutPanelWidget();
        }

        public object CreateLabelWidget(Sandwych.Layout.Models.Label label)
        {
            var labelControl = new Label() { Content = label.Text };
            return labelControl;
        }

        public object CreateHorizontalLineWidget(Sandwych.Layout.Models.HorizontalLine hl)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, IQueryField> CreatedQueryFields
        {
            get
            {
                return this.createdQueryFields;
            }
        }

        public object CreateNotebookWidget(Sandwych.Layout.Models.Notebook notebook)
        {
            throw new NotSupportedException();
        }

        public object CreatePageWidget(Sandwych.Layout.Models.Page page, object parentWidget, object childContent)
        {
            throw new NotSupportedException();
        }

        public object CreateButtonWidget(Sandwych.Layout.Models.Button button)
        {
            throw new NotSupportedException();
        }
    }
}
