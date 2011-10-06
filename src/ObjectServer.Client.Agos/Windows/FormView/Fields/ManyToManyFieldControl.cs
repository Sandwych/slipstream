using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    [TemplatePart(Name = ManyToManyFieldControl.ElementRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ManyToManyFieldControl.ElementAddButton, Type = typeof(Button))]
    [TemplatePart(Name = ManyToManyFieldControl.ElementRemoveButton, Type = typeof(Button))]
    [TemplatePart(Name = ManyToManyFieldControl.ElementTreeGrid, Type = typeof(DataGrid))]
    public class ManyToManyFieldControl : Control, IFieldWidget
    {
        public const string ElementRoot = "Root";
        public const string ElementAddButton = "AddButton";
        public const string ElementRemoveButton = "RemoveButton";
        public const string ElementTreeGrid = "TreeGrid";

        private FrameworkElement root;
        private Button addButton;
        private Button removeButton;
        private DataGrid dataTree;

        private readonly IDictionary<string, object> metaField;
        private string relatedModel;

        public ManyToManyFieldControl(object metaField)
        {
            DefaultStyleKey = typeof(ManyToManyFieldControl);

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            //载入数据
            this.SetFieldInfo();

            this.Loaded += new RoutedEventHandler(this.OnLoaded);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            this.addButton = this.GetTemplateChild(ElementAddButton) as Button;
            this.removeButton = this.GetTemplateChild(ElementRemoveButton) as Button;
            this.dataTree = this.GetTemplateChild(ElementTreeGrid) as DataGrid;

            this.addButton.Click += new RoutedEventHandler(this.OnAddButtonClicked);

        }

        public void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (this.fieldValue != null && this.dataTree != null)
            {
                this.LoadData();
            }
        }

        private void SetFieldInfo()
        {
            var app = (App)Application.Current;
            var relatedFieldName = (string)this.metaField["related_field"];
            var getFieldsArgs = new object[] { (string)this.metaField["relation"] };
            app.ClientService.BeginExecute("core.model", "GetFields", getFieldsArgs, o =>
            {
                var fields = (object[])o;
                var relatedField =
                    (from i in fields
                     let f = (IDictionary<string, object>)i
                     where (string)f["name"] == relatedFieldName
                     select f).Single();
                this.relatedModel = (string)relatedField["relation"];
                Debug.Assert(!string.IsNullOrEmpty(this.relatedModel));
            });
        }

        public string FieldName { get; private set; }

        private object fieldValue = null;
        public object Value
        {
            get
            {
                return null;
            }
            set
            {
                Debug.Assert(!string.IsNullOrEmpty(this.relatedModel));
                this.fieldValue = value;
                if (this.dataTree != null && this.fieldValue != null)
                {
                    this.LoadData();
                }
            }
        }

        private void LoadData()
        {
            var refIDs = (object[])this.fieldValue;
            var app = (App)Application.Current;
            var args = new object[] { refIDs, null };
            app.ClientService.BeginExecute(this.relatedModel, "Read", args, o2 =>
            {
                var objs = (object[])o2;
                var records = objs.Select(r => (Dictionary<string, object>)r).ToArray();

                this.dataTree.ItemsSource = DataSourceCreator.ToDataSource(records);
            });
        }

        public void Empty()
        {
        }

        public void OnAddButtonClicked(object sender, RoutedEventArgs args)
        {
            Debug.Assert(!string.IsNullOrEmpty(this.relatedModel));

            var dlg = new Controls.SelectionDialog(this.relatedModel);
            dlg.ShowDialog();
        }
    }
}
