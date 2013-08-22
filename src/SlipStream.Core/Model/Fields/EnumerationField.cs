using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Sandwych;

namespace SlipStream.Model
{
    internal sealed class EnumerationField : AbstractField
    {
        private Dictionary<string, string> options = new Dictionary<string, string>();

        public const int MaxSize = 60;
        public const int DefaultSize = 16;

        public EnumerationField(IModel model, string name, IDictionary<string, string> options)
            : base(model, name, FieldType.Enumeration)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (options == null || options.Count() <= 0)
            {
                throw new ArgumentNullException("options");
            }

            foreach (var p in options)
            {
                if (string.IsNullOrEmpty(p.Key) || string.IsNullOrEmpty(p.Value))
                {
                    throw new ArgumentOutOfRangeException("options");
                }

                this.options.Add(p.Key, p.Value);
            }

            var maxLength = options.Max(p => p.Key.Length);
            this.Size = Math.Max(maxLength, DefaultSize);
        }


        protected override Dictionary<long, object> OnGetFieldValues(ICollection<Dictionary<string, object>> records)
        {
            if (records == null)
            {
                throw new ArgumentNullException("records");
            }

            var result = new Dictionary<long, object>(records.Count);
            foreach (var r in records)
            {
                var id = (long)r[AbstractModel.IdFieldName];
                var colValue = r[this.Name];
                if (!colValue.IsNull())
                {
                    var enumKey = (string)colValue;
                    string value;
                    if (this.Options.TryGetValue(enumKey, out value))
                    {
                        var fieldValue = new string[] { enumKey, value };
                        result.Add(id, fieldValue);
                    }
                    else
                    {
                        throw new SlipStream.Exceptions.DataException("Invalid enumeration item: " + enumKey);
                    }
                }
                else
                {
                    result.Add(id, null);
                }
            }

            return result;
        }

        protected override object OnSetFieldValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            //TODO 检查是否在范围内

            return value;
        }

        public override object BrowseField(IDictionary<string, object> record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            return record[this.Name];
        }

        public override bool IsColumn { get { return !this.IsFunctional; } }

        public override bool IsScalar
        {
            get { return true; }
        }

        public override SlipStream.Model.OnDeleteAction OnDeleteAction
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
                return this.options;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override void VerifyDefinition()
        {
            base.VerifyDefinition();

            //检查用户提供的枚举选项
            foreach (var p in this.options)
            {
                if (string.IsNullOrEmpty(p.Key) || p.Key.Length > MaxSize || string.IsNullOrEmpty(p.Value))
                {
                    throw new Exceptions.ResourceException(
                        string.Format("Bad enumeration options in field {0}", this));
                }
            }
        }
    }
}
