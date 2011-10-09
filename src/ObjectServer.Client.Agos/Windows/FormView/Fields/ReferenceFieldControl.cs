using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{

    [TemplatePart(Name = ReferenceFieldControl.ElementRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ReferenceFieldControl.ElementTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = ReferenceFieldControl.ElementComboBox, Type = typeof(ComboBox))]
    [TemplatePart(Name = ReferenceFieldControl.ElementSelectButton, Type = typeof(Button))]
    [TemplatePart(Name = ReferenceFieldControl.ElementOpenButton, Type = typeof(Button))]
    [TemplatePart(Name = ReferenceFieldControl.ElementClearButton, Type = typeof(Button))]
    public sealed class ReferenceFieldControl : Control, IFieldWidget
    {
        public const string ElementRoot = "Root";
        public const string ElementSelectButton = "SelectButton";
        public const string ElementOpenButton = "OpenButton";
        public const string ElementClearButton = "ClearButton";
        public const string ElementComboBox = "ComboBox";
        public const string ElementTextBox = "TextBox";

        private FrameworkElement root;
        private TextBox textBox;
        private ComboBox comboBox;
        private Button openButton;
        private Button selectButton;
        private Button clearButton;

        private readonly IDictionary<string, object> metaField;

        public ReferenceFieldControl(object metaField)
        {
            this.metaField = (IDictionary<string, object>)metaField;
            this.FieldName = (string)this.metaField["name"];
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            this.selectButton = this.GetTemplateChild(ElementSelectButton) as Button;
            this.openButton = this.GetTemplateChild(ElementOpenButton) as Button;
            this.clearButton = this.GetTemplateChild(ElementClearButton) as Button;
            this.textBox = this.GetTemplateChild(ElementTextBox) as TextBox;
            this.comboBox = this.GetTemplateChild(ElementComboBox) as ComboBox;

            var isReadonly = (bool)this.metaField["readonly"];

            if (this.comboBox != null)
            {
                var options = (IEnumerable)this.metaField["options"];
                this.comboBox.SelectedValuePath = "Key";
                this.comboBox.DisplayMemberPath = "Value";
                this.comboBox.ItemsSource = options;

                this.comboBox.IsEnabled = !isReadonly;
            }

            if (this.textBox != null)
            {
                this.textBox.IsReadOnly = isReadonly;
            }

            if (this.selectButton != null)
            {
                this.selectButton.IsEnabled = !isReadonly;
            }

            if (this.clearButton != null)
            {
                this.clearButton.IsEnabled = !isReadonly;
            }

        }

        public string FieldName { get; private set; }

        private object[] fieldValue = null;
        public object Value
        {
            get
            {
                if (this.fieldValue != null)
                {
                    return new object[] { this.fieldValue[0], this.fieldValue[1] };
                }
                else
                {
                    return null;
                }
            }
            set
            {
                var tuple = value as object[];
                if (tuple != null)
                {
                    this.fieldValue = tuple;
                    if (this.comboBox != null && this.textBox != null)
                    {
                        this.comboBox.SelectedValue = tuple[0];
                        this.textBox.Text = (string)tuple[2];
                    }
                }
            }
        }

        public void Empty()
        {
            if (this.textBox != null)
            {
                this.textBox.Text = String.Empty;
            }

            if (this.comboBox != null)
            {
                this.comboBox.SelectedIndex = -1;
            }
        }
    }
}
