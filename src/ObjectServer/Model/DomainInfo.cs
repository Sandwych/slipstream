using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    [Serializable]
    public struct DomainInfo
    {
        public DomainInfo(string field, string opr, object value)
            : this()
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("field");
            }

            //TODO 检查是否是合法的 opr
            if (string.IsNullOrEmpty(opr))
            {
                throw new ArgumentNullException("opr");
            }

            this.Field = field;
            this.Operator = opr;
            this.Value = value;
        }

        public string Field { get; private set; }
        public string Operator { get; private set; }
        public object Value { get; private set; }

        public static DomainInfo FromTuple(object[] domain)
        {
            if (domain == null)
            {
                throw new ArgumentNullException("tuple");
            }
            if (domain.Length != 3)
            {
                throw new ArgumentException("tuple");
            }

            return new DomainInfo((string)domain[0], (string)domain[1], domain[2]);
        }

    }
}
