using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ObjectServer.Model
{
    internal abstract class AbstractMetaField : IMetaField
    {
        public AbstractMetaField(string name)
        {
            this.SetName(name);

            this.Label = string.Empty;
            this.IsRequired = false;
            this.Lazy = false;
        }

        private void SetName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Trim().Length == 0)
            {
                throw new ArgumentNullException("name");
            }

            this.Name = name;
            this.Internal = name == AbstractModel.VersionFieldName
                || name == AbstractModel.IdFieldName
                || name == AbstractModel.ActiveFieldName;
        }

        public AbstractMetaField(string name, FieldType type)
            : this(name)
        {
            this.Type = type;

        }

        #region IField 成员

        public string Name { get; private set; }

        public string Label { get; private set; }

        public bool IsFunctional
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

        public virtual int Size { get; set; }

        public virtual bool IsRequired { get; private set; }

        public virtual string Relation { get; set; }

        public virtual string RelatedField { get; set; }

        public virtual string OriginField { get; set; }

        public virtual bool Internal { get; set; }

        public virtual bool IsReadonly { get; set; }

        public virtual bool Lazy { get; set; }

        public abstract bool IsScalar { get; }

        public abstract OnDeleteAction OnDeleteAction { get; set; }

        public virtual IDictionary<string, string> Options
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public abstract bool IsColumn();

        public void Validate()
        {
            if (this.DefaultProc != null && this.Getter != null)
            {
                throw new ArgumentException("Function field cannot have the DefaultProc property");
            }
        }

        public Dictionary<long, object> GetFieldValues(
            IResourceScope ctx, List<Dictionary<string, object>> records)
        {
            if (this.IsFunctional)
            {
                return this.GetFieldValuesFunctional(ctx, records);
            }
            else
            {
                return this.OnGetFieldValues(ctx, records);
            }
        }


        protected abstract Dictionary<long, object> OnGetFieldValues(
            IResourceScope ctx, List<Dictionary<string, object>> records);

        #endregion

        private Dictionary<long, object> GetFieldValuesFunctional(
            IResourceScope ctx, List<Dictionary<string, object>> records)
        {
            var ids = records.Select(p => (long)p["id"]);

            var result = this.Getter(ctx, ids);

            return result;
        }


        #region Fluent interface for options
        public IMetaField SetLabel(string label)
        {
            this.Label = label;
            return this;
        }

        public IMetaField Required()
        {
            this.IsRequired = true;
            return this;
        }

        public IMetaField NotRequired()
        {
            this.IsRequired = false;
            return this;
        }

        public IMetaField SetGetter(FieldGetter fieldGetter)
        {
            this.Getter = fieldGetter;
            return this;
        }

        public IMetaField SetDefaultProc(FieldDefaultProc defaultProc)
        {
            this.DefaultProc = defaultProc;
            return this;
        }

        public IMetaField SetSize(int size)
        {
            this.Size = size;
            return this;
        }

        public IMetaField Readonly()
        {
            this.IsReadonly = true;
            return this;
        }

        public IMetaField NotReadonly()
        {
            this.IsReadonly = false;
            return this;
        }

        public IMetaField OnDelete(OnDeleteAction act)
        {
            this.OnDeleteAction = act;
            return this;
        }
        #endregion
    }
}
