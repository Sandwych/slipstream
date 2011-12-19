using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


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

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!NamingRule.IsValidFieldName(name))
            {
                throw new ArgumentOutOfRangeException("name", "Invalid field name: " + name);
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
            this.CriterionConverter = null;
        }

        public AbstractField(IModel model, string name, FieldType type)
            : this(model, name)
        {
            this.Type = type;

            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!NamingRule.IsValidFieldName(name))
            {
                throw new ArgumentOutOfRangeException("name", "Invalid field name: " + name);
            }
        }

        private void SetName(string name)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));
            Debug.Assert(name.Trim().Length != 0);

            this.Name = name;
            this.Internal = AbstractSqlModel.SystemReadonlyFields.Contains(name);
        }


        #region IField

        public IModel Model { get; private set; }

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

        public Dictionary<long, object> GetFieldValues(
            IServiceContext scope, ICollection<Dictionary<string, object>> records)
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

        public object SetFieldValue(IServiceContext scope, object value)
        {
            if (this.IsFunctional)
            {
                return value;
            }
            else
            {
                return this.OnSetFieldValue(scope, value);
            }
        }

        protected abstract Dictionary<long, object> OnGetFieldValues(
            IServiceContext scope, ICollection<Dictionary<string, object>> records);

        protected abstract object OnSetFieldValue(IServiceContext scope, object value);

        public abstract object BrowseField(IDictionary<string, object> record);

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
            GetPropertyValues(IServiceContext session, IEnumerable<long> ids)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Dictionary<long, object> GetFieldValuesFunctional(
            IServiceContext ctx, ICollection<Dictionary<string, object>> records)
        {
            Debug.Assert(ctx != null);
            Debug.Assert(records != null);

            var ids = records.Select(p => (long)p[AbstractModel.IdFieldName]).ToArray();

            var result = this.ValueGetter(ctx, ids);

            return result;
        }


        #region Fluent interface for options

        public IField SetLabel(string label)
        {
            if (String.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException("label");
            }

            this.Label = label;
            return this;
        }

        public virtual IField Required()
        {
            this.IsRequired = true;
            return this;
        }

        public virtual IField NotRequired()
        {
            this.IsRequired = false;
            return this;
        }

        public IField SetValueGetter(FieldValueGetter fieldGetter)
        {
            if (fieldGetter == null)
            {
                throw new ArgumentNullException("fieldGetter");
            }

            this.ValueGetter = fieldGetter;
            return this;
        }

        public IField SetDefaultValueGetter(FieldDefaultValueGetter defaultProc)
        {
            if (defaultProc == null)
            {
                throw new ArgumentNullException("defaultProc");
            }

            this.DefaultProc = defaultProc;
            return this;
        }

        public IField SetCriterionConverter(CriterionConverter convProc)
        {
            if (convProc == null)
            {
                throw new ArgumentNullException("convProc");
            }

            this.CriterionConverter = convProc;
            return this;
        }

        public IField SetSize(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

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

        public IField Unique()
        {
            this.IsUnique = true;
            return this;
        }

        public IField NotUnique()
        {
            this.IsUnique = false;
            return this;
        }

        #endregion

        public override string ToString()
        {
            return this.Model.Name + "." + this.Name;
        }

        protected static Criterion[] ThroughCriterionConverter(
            IServiceContext ctx, Criterion criterion)
        {
            return new Criterion[] { criterion };
        }
    }
}
