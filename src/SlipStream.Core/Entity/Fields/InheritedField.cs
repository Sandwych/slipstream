using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SlipStream.Entity
{
    internal sealed class InheritedField : AbstractField
    {
        private readonly IField _inheritedField;

        public InheritedField(
            IEntity entity, IField inheritedField)
            : base(entity, inheritedField.Name, inheritedField.Type)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (inheritedField == null)
            {
                throw new ArgumentNullException(nameof(inheritedField));
            }

            this._inheritedField = inheritedField;
        }

        protected override Dictionary<long, object> OnGetFieldValues(
            ICollection<Dictionary<string, object>> rawRecords)
        {
            return this._inheritedField.GetFieldValues(rawRecords);
        }

        protected override object OnSetFieldValue(object value)
        {
            return this._inheritedField.SetFieldValue(value);
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            return this._inheritedField.BrowseField(record);
        }

        public IField BaseField { get { return this._inheritedField; } }

        public override bool IsRequired
        {
            get
            {
                return this._inheritedField.IsRequired;
            }
        }

        public override bool IsReadonly
        {
            get
            {
                return this._inheritedField.IsReadonly;
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
                return this._inheritedField.IsScalar;
            }
        }

        public override int Size
        {
            get
            {
                return this._inheritedField.Size;
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
                return this._inheritedField.OnDeleteAction;
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
                return this._inheritedField.Options;
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
