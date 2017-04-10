using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace SlipStream.Entity
{
    internal abstract class AbstractField : IField
    {
        private bool _isProperty = false;

        public AbstractField(IEntity entity, string name)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!NamingRule.IsValidFieldName(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), $"Invalid field name: {name}");
            }

            if (entity.Fields.ContainsKey(name))
            {
                var msg = string.Format("Duplicate field name '{0}'", name);
                throw new ArgumentOutOfRangeException(nameof(name), msg);
            }

            this.SetName(name);
            this.Entity = entity;

            this.Label = string.Empty;
            this.IsRequired = false;
            this.Lazy = false;
            this.CriterionConverter = null;
        }

        public AbstractField(IEntity entity, string name, FieldType type)
            : this(entity, name)
        {
            this.Type = type;

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!NamingRule.IsValidFieldName(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), "Invalid field name: " + name);
            }
        }

        private void SetName(string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));
            Debug.Assert(name.Trim().Length != 0);

            this.Name = name;
            this.Internal = AbstractSqlEntity.SystemReadonlyFields.Contains(name);
        }


        #region IField

        public IEntity Entity { get; private set; }

        public string Name { get; private set; }

        public string Label { get; private set; }

        public string Help { get; private set; }

        public bool IsFunctional
        {
            get { return this.ValueGetter != null; }
        }

        public FieldValueGetter ValueGetter { get; private set; }

        public FieldDefaultValueGetter DefaultProc { get; private set; }

        public CriterionConverter CriterionConverter { get; private set; }

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

        public bool IsUnique { get; private set; }

        public abstract OnDeleteAction OnDeleteAction { get; set; }

        public abstract IDictionary<string, string> Options { get; set; }

        public abstract bool IsColumn { get; }

        public virtual bool Selectable
        {
            get { return this.IsColumn || this.CriterionConverter != null; }
        }

        public virtual void VerifyDefinition()
        {
            //基础的验证
            if (this.DefaultProc != null && this.ValueGetter != null)
            {
                throw new Exceptions.ResourceException(
                    string.Format("Function field [{0}] can not work with DefaultProc property", this));
            }
        }

        public Dictionary<long, object> GetFieldValues(ICollection<Dictionary<string, object>> records)
        {
            if (this.IsFunctional)
            {
                return this.GetFieldValuesFunctional(records);
            }
            else
            {
                return this.OnGetFieldValues(records);
            }
        }

        public object SetFieldValue(object value)
        {
            if (this.IsFunctional)
            {
                return value;
            }
            else
            {
                return this.OnSetFieldValue(value);
            }
        }

        protected abstract Dictionary<long, object> OnGetFieldValues(ICollection<Dictionary<string, object>> records);

        protected abstract object OnSetFieldValue(object value);

        public abstract object BrowseField(IDictionary<string, object> record);

        public bool IsProperty
        {
            get
            {
                return this._isProperty;
            }
            set
            {
                if (value && this.IsFunctional)
                {
                    throw new NotSupportedException("A functional field cannot be a property field");
                }

                if (value)
                {
                    this._isProperty = true;
                }
            }
        }

        private static Dictionary<long, object>
            GetPropertyValues(IEnumerable<long> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Dictionary<long, object> GetFieldValuesFunctional(ICollection<Dictionary<string, object>> records)
        {
            Debug.Assert(records != null);

            var ids = records.Select(p => (long)p[AbstractEntity.IdFieldName]).ToArray();

            var result = this.ValueGetter(this.Entity.DbDomain.CurrentSession, ids);

            return result;
        }


        #region Fluent interface for options

        public IField WithLabel(string label)
        {
            if (String.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.Label = label;
            return this;
        }

        public virtual IField WithRequired()
        {
            this.IsRequired = true;
            return this;
        }

        public virtual IField WithNotRequired()
        {
            this.IsRequired = false;
            return this;
        }

        public IField WithValueGetter(FieldValueGetter fieldGetter)
        {
            if (fieldGetter == null)
            {
                throw new ArgumentNullException("fieldGetter");
            }

            this.ValueGetter = fieldGetter;
            return this;
        }

        public IField WithDefaultValueGetter(FieldDefaultValueGetter defaultProc)
        {
            if (defaultProc == null)
            {
                throw new ArgumentNullException("defaultProc");
            }

            this.DefaultProc = defaultProc;
            return this;
        }

        public IField WithCriterionConverter(CriterionConverter convProc)
        {
            if (convProc == null)
            {
                throw new ArgumentNullException("convProc");
            }

            this.CriterionConverter = convProc;
            return this;
        }

        public IField WithSize(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            this.Size = size;
            return this;
        }

        public IField WithHelp(string help)
        {
            this.Help = help;
            return this;
        }

        public IField WithReadonly()
        {
            this.IsReadonly = true;
            return this;
        }

        public IField WithNotReadonly()
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

        public IField WithOptions(IDictionary<string, string> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            this.Options = options;
            return this;
        }

        public IField WithUnique()
        {
            this.IsUnique = true;
            return this;
        }

        public IField WithNotUnique()
        {
            this.IsUnique = false;
            return this;
        }

        #endregion

        public override string ToString()
        {
            return this.Entity.Name + "." + this.Name;
        }

        protected static Criterion[] ThroughCriterionConverter(
            IServiceContext ctx, Criterion criterion)
        {
            return new Criterion[] { criterion };
        }
    }
}
