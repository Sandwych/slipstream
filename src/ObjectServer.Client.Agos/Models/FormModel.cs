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

        private readonly Dictionary<string, List<string>> errors =
            new Dictionary<string, List<string>>();

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

            //TODO 验证字段
            //绑定语法 Text="{Binding [Title], Mode=TwoWay}"


            this.record[binder.Name] = value;
            this.NotifyPropertyChanged(binder.Name);

            return base.TrySetMember(binder, value);
        }

        // Adds the specified error to the errors collection if it is not 
        // already present, inserting it in the first position if isWarning is 
        // false. Raises the ErrorsChanged event if the collection changes. 
        public void AddError(string propertyName, string error, bool isWarning)
        {
            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = new List<string>();
            }

            if (!errors[propertyName].Contains(error))
            {
                if (isWarning)
                {
                    errors[propertyName].Add(error);
                }
                else
                {
                    errors[propertyName].Insert(0, error);
                }
                RaiseErrorsChanged(propertyName);
            }
        }

        // Removes the specified error from the errors collection if it is
        // present. Raises the ErrorsChanged event if the collection changes.
        public void RemoveError(string propertyName, string error)
        {
            if (errors.ContainsKey(propertyName) &&
                errors[propertyName].Contains(error))
            {
                errors[propertyName].Remove(error);
                if (errors[propertyName].Count == 0)
                {
                    errors.Remove(propertyName);
                }
                this.RaiseErrorsChanged(propertyName);
            }
        }

        public void RaiseErrorsChanged(string propertyName)
        {
            if (this.ErrorsChanged != null)
            {
                this.ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

    }
}
