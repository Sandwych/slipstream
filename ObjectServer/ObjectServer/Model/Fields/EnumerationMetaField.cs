using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal sealed class EnumerationMetaField : MetaField
    {
        private Dictionary<string, string> options = new Dictionary<string, string>();

        public const int DefaultSize = 16;

        public EnumerationMetaField(string name, IEnumerable<KeyValuePair<string, string>> options)
            : base(name, FieldType.Enumeration)
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
                this.options.Add(p.Key, p.Value);
            }

            var maxLength = options.Max(p => p.Key.Length);
            this.Size = Math.Max(maxLength, DefaultSize);
        }


        protected override Dictionary<long, object> OnGetFieldValues(
            IContext session, List<Dictionary<string, object>> records)
        {
            var result = new Dictionary<long, object>(records.Count());

            foreach (var r in records)
            {
                result[(long)r["id"]] = r[this.Name];
            }

            return result;
        }

        public override bool IsColumn()
        {
            return !this.Functional;
        }

        public override ObjectServer.Model.ReferentialAction ReferentialAction
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
        }
    }
}
