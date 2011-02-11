using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using Npgsql;
using NpgsqlTypes;

using ObjectServer.Utility;
using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject]
    public class Model : ModelBase
    {
        public Model() : base()
        {
            this.Automatic = false;
            this.Name = "Core.Model";
            this.TableName = "core_model";
        }
    }
}
