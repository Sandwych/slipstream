using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject] 
    public class FieldModel : ModelBase
    {

        public FieldModel()
            : base()
        {
            this.Name = "core.field";
            this.Versioned = false;
        }


    }
}
