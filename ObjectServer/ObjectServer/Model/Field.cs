using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [ModelObject] 
    public class Field : Model
    {

        public Field()
            : base()
        {
            this.Name = "Core.MetaField";
            this.TableName = "core_metafield";

        }


    }
}
