using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace SlipStream.Client.Model
{
    public sealed class MetaField
    {
        public MetaField(IDictionary<string, object> record)
        {
            this.Name = (string)record["name"];
            this.IsRequired = (bool)record["required"];
            this.IsReadonly = (bool)record["readonly"];
            this.Relation = (string)record["relation"];
            this.Type = (string)record["type"];
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsRequired { get; set; }
        public string Relation { get; set; }

    }
}
