using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectServer.Client.Model
{
    public class ViewModel : AbstractModel, INotifyPropertyChanged
    {
        private static readonly Dictionary<string, PropertyInfo> props =
            new Dictionary<string, PropertyInfo>();

        static ViewModel()
        {
            var selfType = typeof(ViewModel);
            RegisterAllFields(props, selfType);
        }


        public ViewModel(IDictionary<string, object> record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            SetPropertiesByRecord(this, props, record);
        }

        public ViewModel()
        {
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private string name;
        [Field("name")]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

        private string model;
        [Field("model")]
        public string Model
        {
            get
            {
                return this.model;
            }
            set
            {
                this.model = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Model"));
            }
        }

        private string kind;
        [Field("kind")]
        public string Kind
        {
            get
            {
                return this.kind;
            }
            set
            {
                this.kind = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Kind"));
            }
        }

        private string layout;
        [Field("layout")]
        public string Layout
        {
            get
            {
                return this.layout;
            }
            set
            {
                this.layout = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Layout"));
            }
        }
    }
}
