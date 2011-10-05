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

using ObjectServer.Client.Agos.Models;

namespace ObjectServer.Client.Agos.Windows.FormView
{

    [TemplatePart(Name = HLine.ElementRoot, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = HLine.ElementHorizontalLine, Type = typeof(Rectangle))]
    [TemplatePart(Name = HLine.ElementLabel, Type = typeof(Label))]
    public class HLine : Control
    {
        public const string ElementRoot = "Root";
        public const string ElementHorizontalLine = "HorizontalLine";
        public const string ElementLabel = "Label";

        FrameworkElement root;
        Rectangle border;
        Label label;
        private readonly string text;

        public HLine(string text)
        {
            this.text = text;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.root = this.GetTemplateChild(ElementRoot) as FrameworkElement;
            this.border = this.GetTemplateChild(ElementHorizontalLine) as Rectangle;
            this.label = this.GetTemplateChild(ElementLabel) as Label;

            if (this.label != null && !string.IsNullOrEmpty(this.text))
            {
                this.label.Content = this.text;
            }
        }

        public string Text
        {
            get
            {
                if (this.label != null)
                {
                    return (string)this.label.Content;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (this.label != null)
                {
                    this.label.Content = value ?? string.Empty;
                }
            }
        }
    }
}
