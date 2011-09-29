using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Dynamic;

namespace ObjectServer.Client.Agos.Models
{
    public sealed class FormModel : DynamicObject, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, object> record =
            new Dictionary<string, object>();

        private readonly Dictionary<string, List<object>> errors =
            new Dictionary<string, List<object>>();

        public FormModel()
        {
        }

        public FormModel(IDictionary<string, object> record)
        {
        }

        public object this[string property]
        {
            get
            {
                return this.record[property];
            }
            set
            {
                // The validation code of which you speak here.
                this.record[property] = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }


        #region INotifyDataErrorInfo Members

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return this.errors[propertyName];
        }

        public bool HasErrors
        {
            get { return this.errors.Count > 0; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.record.TryGetValue(binder.Name, out result))
            {
                return base.TryGetMember(binder, out result);
            }
            else
            {
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (!this.record.ContainsKey(binder.Name))
            {
                return false;
            }

            this.record[binder.Name] = value;
            this.NotifyPropertyChanged(binder.Name);

            return base.TrySetMember(binder, value);
        }
    }
}
