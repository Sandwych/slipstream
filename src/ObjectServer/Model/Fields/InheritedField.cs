using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class InheritedField : AbstractField
    {
        private IMetaField inheritedField;

        public InheritedField(
            IMetaModel model, IMetaField inheritedField)
            : base(model, inheritedField.Name, inheritedField.Type)
        {
            this.inheritedField = inheritedField;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
           IServiceScope scope, ICollection<Dictionary<string, object>> rawRecords)
        {
            return this.inheritedField.GetFieldValues(scope, rawRecords);
        }

        protected override object OnSetFieldValue(IServiceScope scope, object value)
        {
            return this.inheritedField.SetFieldValue(scope, value);
        }

        public override object BrowseField(IServiceScope scope, IDictionary<string, object> record)
        {
            return this.inheritedField.BrowseField(scope, record);
        }

        public override bool IsRequired
        {
            get
            {
                return this.inheritedField.IsRequired;
            }
        }

        public override bool IsReadonly
        {
            get
            {
                return this.inheritedField.IsReadonly;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override bool IsColumn()
        {
            return false;
        }

        public override bool IsScalar
        {
            get
            {
                return this.inheritedField.IsScalar;
            }
        }

        public override int Size
        {
            get
            {
                return this.inheritedField.Size;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override OnDeleteAction OnDeleteAction
        {
            get
            {
                return this.inheritedField.OnDeleteAction;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override IDictionary<string, string> Options
        {
            get
            {
                return this.inheritedField.Options;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}
