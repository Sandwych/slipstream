using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ScalarField : AbstractField
    {
        public ScalarField(IMetaModel model, string name, FieldType ft)
            : base(model, name, ft)
        {
        }


        protected override Dictionary<long, object> OnGetFieldValues(
            IServiceScope session, ICollection<Dictionary<string, object>> records)
        {
            return records.ExtractFieldValues(this.Name);
        }


        protected override object OnSetFieldValue(IServiceScope scope, object value)
        {
            return value;
        }

        public override object BrowseField(IServiceScope scope, IDictionary<string, object> record)
        {
            return record[this.Name];
        }

        public override bool IsColumn()
        {
            return !this.IsFunctional;
        }

        public override bool IsScalar
        {
            get { return false; }
        }

        public override ObjectServer.Model.OnDeleteAction OnDeleteAction
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
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

    }
}
