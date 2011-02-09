using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    //[ServiceObject] 
    public class Field : ModelBase
    {

        public Field()
            : base()
        {
            this.Automatic = false;
            this.Name = "Core.Field";
            this.TableName = "core_field";
        }


    }
}
