using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Model
{
    public sealed class InheritanceDescriptor
    {
        public InheritanceDescriptor(string baseModel, string relatedField)
        {
            if (string.IsNullOrEmpty(baseModel))
            {
                throw new ArgumentNullException("baseModel");
            }

            if (string.IsNullOrEmpty(relatedField))
            {
                throw new ArgumentNullException("baseModel");
            }

            this.BaseModel = baseModel;
            this.RelatedField = relatedField;
        }

        public string BaseModel { get; private set; }
        public string RelatedField { get; private set; }
    }
}
