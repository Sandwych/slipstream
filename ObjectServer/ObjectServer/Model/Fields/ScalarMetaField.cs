using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class ScalarMetaField : AbstractMetaField
    {
        public ScalarMetaField(IMetaModel model, string name, FieldType ft)
            : base(model, name, ft)
        {
        }


        protected override Dictionary<long, object> OnGetFieldValues(
            IResourceScope session, ICollection<Dictionary<string, object>> records)
        {
            return records.ExtractFieldValues(this.Name);
        }


        protected override object OnSetFieldValue(IResourceScope scope, object value)
        {
            return value;
        }

        public override object BrowseField(IResourceScope scope, IDictionary<string, object> record)
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

    }
}
