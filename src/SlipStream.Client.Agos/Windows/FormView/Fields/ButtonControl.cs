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

namespace SlipStream.Client.Agos.Windows.FormView
{
    public sealed class ButtonControl : Button
    {
        private readonly string target;
        private readonly string model;

        public ButtonControl(Sandwych.Layout.Models.Button buttonInfo, string model)
            : base()
        {
            this.model = model;
            this.Text = buttonInfo.Text;
            this.ButtonName = buttonInfo.Name;
            this.target = buttonInfo.Target;
        }

        public string Text
        {
            get
            {
                return (string)base.Content;
            }
            set
            {
                base.Content = value;
            }
        }

        public string ButtonName { get; private set; }
    }
}
