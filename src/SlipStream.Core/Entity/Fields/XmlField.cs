using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Exceptions;

namespace SlipStream.Entity
{
    internal sealed class XmlField : AbstractField
    {
        public XmlField(IEntity model, string name)
            : base(model, name, FieldType.Xml)
        {
        }

        protected override Dictionary<long, object> OnGetFieldValues(
            ICollection<Dictionary<string, object>> records)
        {
            return records.ExtractFieldValues(this.Name);
        }

        protected override object OnSetFieldValue(object value)
        {
            return value;
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            if (record == null || record.Count == 0)
            {
                throw new ArgumentNullException("record");
            }

            return record[this.Name];
        }

        public override bool IsColumn { get { return !this.IsFunctional; } }

        public override bool IsScalar
        {
            get { return true; }
        }

        public override SlipStream.Entity.OnDeleteAction OnDeleteAction
        {
            get
            {
                throw new NotSupportedException();
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
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void VerifyDefinition()
        {
            base.VerifyDefinition();
        }

    }
}
