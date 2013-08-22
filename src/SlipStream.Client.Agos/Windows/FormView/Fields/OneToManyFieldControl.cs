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

using SlipStream.Client.Agos.Models;
using SlipStream.Client.Agos.Controls;

namespace SlipStream.Client.Agos.Windows.FormView
{
    [TemplatePart(Name = ManyToManyFieldControl.ElementRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ManyToManyFieldControl.ElementAddButton, Type = typeof(Button))]
    [TemplatePart(Name = ManyToManyFieldControl.ElementRemoveButton, Type = typeof(Button))]
    [TemplatePart(Name = ManyToManyFieldControl.ElementTreeGrid, Type = typeof(TreeDataGrid))]
    public sealed class OneToManyFieldControl : Control, IFieldWidget
    {
        public const string ElementRoot = "Root";
        public const string ElementNewButton = "NewButton";
        public const string ElementDeleteButton = "DeleteButton";
        public const string ElementOpenButton = "OpenButton";
        public const string ElementTreeGrid = "TreeGrid";

        private readonly IDictionary<string, object> metaField;
        private FrameworkElement root;
        private Button newButton;
        private Button deleteButton;
        private Button openButton;
        private TreeDataGrid treeGrid;

        private readonly string relatedModel;

        public OneToManyFieldControl(object metaField)
        {
            var app = (App)Application.Current;

            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];
            this.relatedModel = (string)this.metaField["relation"];

            //载入数据
            this.Loaded += new RoutedEventHandler(this.OnLoaded);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            this.newButton = this.GetTemplateChild(ElementNewButton) as Button;
            this.openButton = this.GetTemplateChild(ElementOpenButton) as Button;
            this.deleteButton = this.GetTemplateChild(ElementDeleteButton) as Button;
            this.treeGrid = this.GetTemplateChild(ElementTreeGrid) as TreeDataGrid;

            if (this.treeGrid != null)
            {
                this.treeGrid.Init(relatedModel, null);
            }

            if (this.openButton != null)
            {
                this.openButton.Click += new RoutedEventHandler(this.OnOpenButtonClicked);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {

        }

        public string FieldName { get; private set; }

        private object fieldValue;
        public object Value
        {
            get
            {
                return null;
            }
            set
            {
                this.fieldValue = value;
                if (this.treeGrid != null && this.fieldValue != null)
                {
                    var refIDs = (object[])this.fieldValue;
                    var getFieldsArgs = new object[] { (string)this.metaField["relation"] };
                    if (refIDs.Length > 0)
                    {
                        this.treeGrid.Reload(refIDs.Cast<long>());
                    }
                }
            }
        }

        public void Empty()
        {
        }

        private void OnOpenButtonClicked(object sender, RoutedEventArgs args)
        {
            Debug.Assert(this.metaField != null);
            Debug.Assert(!string.IsNullOrEmpty(this.relatedModel));
            Debug.Assert(this.treeGrid != null);

            var ids = this.treeGrid.GetSelectedIDs();

            if (ids.Length == 1)
            {
                var dlg = new Agos.Windows.FormView.FormDialog(relatedModel, ids.First());
                dlg.ShowDialog();
            }
        }
    }
}
