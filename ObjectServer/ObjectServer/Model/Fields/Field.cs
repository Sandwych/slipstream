using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Fields
{
    internal class Field : IField
    {
        public Field(string name, string type)
        {
            this.Name = name;
            this.Type = type;
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

        public virtual string SqlType
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

        public bool IsFunctionField
        {
            get { return this.Getter != null; }
        }

        public FieldGetter Getter
        {
            get;
            internal set;
        }

        public string Type
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

        #endregion
    }
}
