using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ObjectServer.Model
{
    internal abstract class AbstractField : IField
    {
        private bool isProperty = false;

        public AbstractField(IModel model, string name)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (!NamingRule.IsValidFieldName(name))
            {
                throw new ArgumentException("name", "Invalid field name");
            }

            if (model.Fields.ContainsKey(name))
            {
                var msg = string.Format("Duplicate field name '{0}'", name);
                throw new ArgumentOutOfRangeException("name", msg);
            }

            this.SetName(name);
            this.Model = model;

            this.Label = string.Empty;
            this.IsRequired = false;
            this.Lazy = false;
        }

        public AbstractField(IModel model, string name, FieldType type)
            : this(model, name)
        {
            this.Type = type;

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


        #region IMetaField

        public IModel Model { get; private set; }

        public string Name { get; private set; }

        public string Label { get; private set; }

        public string Help { get; private set; }

        public bool IsFunctional
        {
            get { return this.Getter != null; }
        }

        public FieldValueGetter Getter
        {
            get;
            set;
        }

        public FieldDefaultValueGetter DefaultProc { get; set; }

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

        public abstract IDictionary<string, string> Options { get; set; }

        public abstract bool IsColumn();

        public void Validate()
        {
            if (this.DefaultProc != null && this.Getter != null)
            {
                throw new ArgumentException("Function field cannot have the DefaultProc property");
            }
        }

        public Dictionary<long, object> GetFieldValues(
            IServiceScope scope, ICollection<Dictionary<string, object>> records)
        {
            if (this.IsFunctional)
            {
                return this.GetFieldValuesFunctional(scope, records);
            }
            else
            {
                return this.OnGetFieldValues(scope, records);
            }
        }

        public object SetFieldValue(IServiceScope scope, object value)
        {
            if (this.IsFunctional)
            {
                throw new NotSupportedException();
            }
            else
            {
                return this.OnSetFieldValue(scope, value);
            }
        }

        protected abstract Dictionary<long, object> OnGetFieldValues(
            IServiceScope scope, ICollection<Dictionary<string, object>> records);

        protected abstract object OnSetFieldValue(IServiceScope scope, object value);

        public abstract object BrowseField(IServiceScope scope, IDictionary<string, object> record);

        public bool IsProperty
        {
            get
            {
                return this.isProperty;
            }
            set
            {
                if (value && this.IsFunctional)
                {
                    throw new NotSupportedException("A functional field cannot be a property field");
                }

                if (value)
                {
                    this.isProperty = true;
                }
            }
        }

        private static Dictionary<long, object>
            GetPropertyValues(IServiceScope session, IEnumerable<long> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Dictionary<long, object> GetFieldValuesFunctional(
            IServiceScope ctx, ICollection<Dictionary<string, object>> records)
        {
            var ids = records.Select(p => (long)p["id"]).ToArray();

            var result = this.Getter(ctx, ids);

            return result;
        }


        #region Fluent interface for options

        public IField SetLabel(string label)
        {
            this.Label = label;
            return this;
        }

        public IField Required()
        {
            this.IsRequired = true;
            return this;
        }

        public IField NotRequired()
        {
            this.IsRequired = false;
            return this;
        }

        public IField ValueGetter(FieldValueGetter fieldGetter)
        {
            this.Getter = fieldGetter;
            return this;
        }

        public IField DefaultValueGetter(FieldDefaultValueGetter defaultProc)
        {
            this.DefaultProc = defaultProc;
            return this;
        }

        public IField SetSize(int size)
        {
            this.Size = size;
            return this;
        }

        public IField SetHelp(string help)
        {
            this.Help = help;
            return this;
        }

        public IField Readonly()
        {
            this.IsReadonly = true;
            return this;
        }

        public IField NotReadonly()
        {
            this.IsReadonly = false;
            return this;
        }

        public IField OnDelete(OnDeleteAction act)
        {
            this.OnDeleteAction = act;
            return this;
        }

        public IField BeProperty()
        {
            this.IsProperty = true;
            return this;
        }

        public IField SetOptions(IDictionary<string, string> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            this.Options = options;
            return this;
        }

        #endregion
    }
}
