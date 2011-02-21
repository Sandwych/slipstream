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

        public string Name
        {
            get;
            private set;
        }

        public string Label
        {
            get;
            internal set;
        }


        public bool Functional
        {
            get { return this.Getter != null; }
        }

        public FieldGetter Getter
        {
            get;
            internal set;
        }

        public FieldDefaultProc DefaultProc
        {
            get;
            internal set;
        }

        public FieldType Type
        {
            get;
            private set;
        }

        public int Size
        {
            get;
            internal set;
        }

        public bool Required
        {
            get;
            internal set;
        }

        public string Relation
        {
            get;
            internal set;
        }

        public string RelatedField
        {
            get;
            internal set;
        }

        public string OriginField
        {
            get;
            internal set;
        }

        public bool Internal
        {
            get;
            internal set;
        }

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
            ISession session, List<Dictionary<string, object>> records);

        #endregion
    }
}
