using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    public sealed class InheritanceDescriptor
    {
        public InheritanceDescriptor(string baseEntity, string relatedField)
        {
            if (string.IsNullOrEmpty(baseEntity))
            {
                throw new ArgumentNullException(nameof(baseEntity));
            }

            if (string.IsNullOrEmpty(relatedField))
            {
                throw new ArgumentNullException(nameof(relatedField));
            }

            this.BaseEntity = baseEntity;
            this.RelatedField = relatedField;
        }

        public string BaseEntity { get; private set; }
        public string RelatedField { get; private set; }
    }
}
