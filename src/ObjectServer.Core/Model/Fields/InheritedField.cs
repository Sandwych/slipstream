using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer.Model
{
    internal sealed class InheritedField : AbstractField
    {
        private readonly IField inheritedField;

        public InheritedField(
            IModel model, IField inheritedField)
            : base(model, inheritedField.Name, inheritedField.Type)
        {
            Debug.Assert(model != null);
            Debug.Assert(inheritedField != null);

            this.inheritedField = inheritedField;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
            ICollection<Dictionary<string, object>> rawRecords)
        {
            return this.inheritedField.GetFieldValues(rawRecords);
        }

        protected override object OnSetFieldValue(object value)
        {
            return this.inheritedField.SetFieldValue(value);
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            return this.inheritedField.BrowseField(record);
        }

        public IField BaseField { get { return this.inheritedField; } }

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

        public override bool IsColumn { get { return false; } }

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

        public override bool Selectable
        {
            get
            {
                return this.BaseField.Selectable;
            }
        }
    }
}
