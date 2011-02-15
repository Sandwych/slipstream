using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Fields
{
    public class SimpleField : IField
    {

        public SimpleField(string name)
        {
            this.Name = name;
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

        public string SqlType
        {
            get
            {
                var sqlType = this.Type;
                var sqlSize = string.Empty;
                if (this.Size > 1)
                {
                    sqlSize = "(" + this.Size.ToString() + ")";
                }
                var notNull = string.Empty;
                if (this.Required)
                {
                    notNull = "NOT NULL";
                }

                return string.Format("{0}{1} {2}", sqlType, sqlSize, notNull);
            }
        }

        public string Type
        {
            get;
            internal set;
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

        #endregion
    }
}
