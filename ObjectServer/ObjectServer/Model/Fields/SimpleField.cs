using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Fields
{
    public class SimpleField : IFieldInfo
    {

        #region IFieldInfo 成员

        public string Name
        {
            get;
            internal set;
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
                return string.Format("{0}({1})",
                    this.Type, this.Size);
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

        #endregion
    }
}
