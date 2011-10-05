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
using System.Diagnostics;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{
    [TemplatePart(Name = ManyToOneFieldControl.ElementRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ManyToOneFieldControl.ElementTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = ManyToOneFieldControl.ElementSelectButton, Type = typeof(Button))]
    [TemplatePart(Name = ManyToOneFieldControl.ElementOpenButton, Type = typeof(Button))]
    public class ManyToOneFieldControl : Control, IFieldWidget
    {
        public const string ElementRoot = "Root";
        public const string ElementTextBox = "TextBox";
        public const string ElementSelectButton = "SelectButton";
        public const string ElementOpenButton = "OpenButton";

        private readonly IDictionary<string, object> metaField;

        private FrameworkElement root;
        private Button selectButton;
        private Button openButton;
        private TextBox textBox;

        public ManyToOneFieldControl(object metaField)
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];

            DefaultStyleKey = typeof(ManyToOneFieldControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            this.selectButton = this.GetTemplateChild(ElementSelectButton) as Button;
            this.openButton = this.GetTemplateChild(ElementOpenButton) as Button;
            this.textBox = this.GetTemplateChild(ElementTextBox) as TextBox;

            if (this.selectButton != null)
            {
                this.selectButton.Click += new RoutedEventHandler(this.SelectButtonClicked);
            }

            if (this.openButton != null)
            {
                this.openButton.Click += new RoutedEventHandler(this.OpenButtonClicked);
            }
        }

        public string FieldName { get; private set; }

        private object[] fieldValue = null;
        public object Value
        {
            get
            {
                return fieldValue != null ? fieldValue.First() : null;
            }
            set
            {
                var tuple = value as object[];
                if (tuple != null)
                {
                    this.fieldValue = tuple;
                    if (this.textBox != null)
                    {
                        this.textBox.Text = (string)tuple[1];
                    }
                }
                else if (value is long)
                {
                    this.Value = new object[] { value, String.Empty }; //TODO 读取名称
                }
            }
        }

        public void Empty()
        {
            this.Value = null;
        }

        public void SelectButtonClicked(object sender, RoutedEventArgs args)
        {
            Debug.Assert(this.metaField != null);
            var relatedModel = this.metaField["relation"] as string;
            Debug.Assert(!string.IsNullOrEmpty(relatedModel));

            var dlg = new Controls.SelectionDialog(relatedModel);
            dlg.RecordsSelected += new EventHandler<Controls.RecordsSelectedEventArgs>(this.OnIDsSelected);
            dlg.ShowDialog();
        }

        public void OnIDsSelected(object sender, Controls.RecordsSelectedEventArgs args)
        {
            if (args.SelectedIDs.Length != 1)
            {
                return;
            }

            this.Value = args.SelectedIDs.First();
        }

        public void OpenButtonClicked(object sender, RoutedEventArgs args)
        {
            Debug.Assert(this.metaField != null);
            var relatedModel = this.metaField["relation"] as string;
            Debug.Assert(!string.IsNullOrEmpty(relatedModel));

            if (this.Value != null)
            {
                var dlg = new Agos.Windows.FormView.FormDialog(relatedModel, (long)this.Value);
                dlg.ShowDialog();
            }
        }
    }
}
