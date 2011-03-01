using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal abstract class MetaField : IMetaField
    {
        public MetaField()
        {
            this.Label = string.Empty;
            this.Required = false;
        }

        public MetaField(string name, FieldType type)
            : this()
        {
            if (string.IsNullOrEmpty(name) || name.Trim().Length == 0)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
            this.Type = type;

            this.Internal = name == ModelBase.VersionFieldName
                || name == ModelBase.IdFieldName
                || name == ModelBase.ActiveFieldName;
        }

        #region IField 成员

        public string Name { get; set; }

        public string Label { get; set; }

        public bool Functional
        {
            get { return this.Getter != null; }
        }

        public FieldGetter Getter
        {
            get;
            set;
        }

        public FieldDefaultProc DefaultProc { get; set; }

        public FieldType Type { get; set; }

        public int Size { get; set; }

        public virtual bool Required { get; set; }

        public virtual string Relation { get; set; }

        public virtual string RelatedField { get; set; }

        public virtual string OriginField { get; set; }

        public virtual bool Internal { get; set; }

        public abstract bool IsStorable();

        public abstract ReferentialAction ReferentialAction { get; set; }

        public void Validate()
        {
            if (this.DefaultProc != null && this.Getter != null)
            {
                throw new ArgumentException("Function field cannot have the DefaultProc property");
            }
        }

        public Dictionary<long, object> GetFieldValues(
            IContext callingContext, List<Dictionary<string, object>> records)
        {
            if (this.Functional)
            {
                return this.GetFieldValuesFunctional(callingContext, records);
            }
            else
            {
                return this.OnGetFieldValues(callingContext, records);
            }
        }


        protected abstract Dictionary<long, object> OnGetFieldValues(
            IContext callingContext, List<Dictionary<string, object>> records);

        #endregion

        private Dictionary<long, object> GetFieldValuesFunctional(
            IContext callingContext, List<Dictionary<string, object>> records)
        {
            var ids = records.Select(p => p["id"]).ToArray();

            var result = this.Getter(callingContext, ids);

            return result;
        }
    }
}
