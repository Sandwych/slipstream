using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public sealed class ManyToManyModel : TableModel
    {
        public ManyToManyModel(string leftField, string rightField)
        {
            
        }

        public IMetaField LeftField { get; private set; }
        public IMetaField RightField { get; private set; }
    }
}
