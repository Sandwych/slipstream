using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TransactionMethodAttribute : Attribute
    {
        public TransactionMethodAttribute(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
