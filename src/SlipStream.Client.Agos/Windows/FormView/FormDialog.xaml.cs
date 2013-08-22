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

namespace SlipStream.Client.Agos.Windows.FormView
{
    public partial class FormDialog : FloatableWindow
    {
        private readonly long recordID;
        private readonly FormView formView;
        private readonly string model;

        public event EventHandler Saved;

        public FormDialog(string model, long recordID)
        {
            this.recordID = recordID;
            this.model = model;

            InitializeComponent();

            this.formView = new FormView(model, recordID);
            this.ScrollContent.Child = formView;

        }

        public bool IsNew
        {
            get { return this.recordID <= 0; }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            var app = (App)App.Current;

            //执行保存
            var record = formView.GetFieldValues();

            if (this.IsNew)
            {
                //执行创建
                this.CreateModel(app, record);
            }
            else
            {
                this.WriteModel(app, record);
            }
        }

        private void CreateModel(App app, IDictionary<string, object> record)
        {
            var args = new object[] { record };
            app.ClientService.Execute(this.model, "Create", args, (result, error) =>
            {
                this.Saved(this, new EventArgs());

                this.DialogResult = true;
            });
        }

        private void WriteModel(App app, IDictionary<string, object> record)
        {
            var args = new object[] { this.recordID, record };
            app.ClientService.Execute(this.model, "Write", args, (result, error) =>
            {
                this.Saved(this, new EventArgs());

                this.DialogResult = true;
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

