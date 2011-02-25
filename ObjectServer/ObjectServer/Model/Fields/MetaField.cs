using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal abstract class MetaField : IMetaField
    {
        public MetaField(string name, FieldType type)
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

        public FieldGetter Getter { get; set; }

        public FieldDefaultProc DefaultProc { get; set; }

        public FieldType Type { get; set; }

        public int Size { get; set; }

        public bool Required { get; set; }

        public string Relation { get; set; }

        public string RelatedField { get; set; }

        public string OriginField { get; set; }

        public bool Internal { get; set; }

        public bool IsStorable()
        {
            if (this.Functional)
            {
                return false;
            }

            if (this.Type == FieldType.ManyToMany
                || this.Type == FieldType.OneToMany)
            {
                return false;
            }

            return true;
        }

        public void Validate()
        {
            if (this.DefaultProc != null && this.Getter != null)
            {
                throw new ArgumentException("Function field cannot have the DefaultProc property");
            }
        }

        public abstract Dictionary<long, object> GetFieldValues(
            ICallingContext session, List<Dictionary<string, object>> records);

        #endregion
    }
}
