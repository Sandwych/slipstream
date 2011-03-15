using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public sealed class InheritanceInfo
    {
        public InheritanceInfo(string baseModel, string field)
        {
            if (string.IsNullOrEmpty(baseModel))
            {
                throw new ArgumentNullException("baseModel");
            }

            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentNullException("baseModel");
            }

            this.BaseModel = baseModel;
            this.Field = field;
        }

        public string BaseModel { get; private set; }
        public string Field { get; private set; }
    }
}
