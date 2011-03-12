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
            IResourceScope session, List<Dictionary<string, object>> records)
        {
            return records.ExtractFieldValues(this.Name);
        }


        protected override Dictionary<long, object> OnSetFieldValues(
            IResourceScope scope, IList<Dictionary<string, object>> records)
        {
            return records.ExtractFieldValues(this.Name);
        }


        private Dictionary<long, object> ExtractFieldValues(List<Dictionary<string, object>> records)
        {
            var result = new Dictionary<long, object>(records.Count);
            foreach (var r in records)
            {
                result[(long)r["id"]] = r[this.Name];
            }
            return result;
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
