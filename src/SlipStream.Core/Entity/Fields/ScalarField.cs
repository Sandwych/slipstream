using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Exceptions;

namespace SlipStream.Entity
{
    internal sealed class ScalarField : AbstractField
    {
        private static readonly IReadOnlyCollection<FieldType> ScalarFieldTypes =
            new HashSet<FieldType>()
            {
                FieldType.Identifier,
                FieldType.Integer,
                FieldType.BigInteger,
                FieldType.Double,
                FieldType.DateTime,
                FieldType.Date,
                FieldType.Time,
                FieldType.Boolean,
                FieldType.Decimal,
                FieldType.Chars,
                FieldType.Text,
                FieldType.Binary,
            };

        public ScalarField(IEntity entity, string name, FieldType ft)
            : base(entity, name, ft)
        {
            if (!ScalarFieldTypes.Contains(ft))
            {
                throw new ResourceException("提供的字段类型不是标量字段");
            }
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
