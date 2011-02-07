using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using Npgsql;
using NpgsqlTypes;

using ObjectServer.Utility;


namespace ObjectServer.Model
{
    [ServiceObject]
    public class Model : ModelBase
    {
        public Model() : base()
        {
            this.Name = "Core.Model";
            this.TableName = "core_model";
        }
    }
}
