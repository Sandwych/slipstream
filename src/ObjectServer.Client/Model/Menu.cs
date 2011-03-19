using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;

namespace ObjectServer.Client.Model
{
    public class Menu
    {
        public Menu()
        {
        }

        public Menu(IDictionary<string, object> record)
        {
            this.Id = (long)record["id"];
            this.Name = (string)record["name"];

            var parentField = record["parent"];
            if (parentField != null)
            {
                var parentInfo = (object[])parentField;
                this.ParentId = (long)parentInfo[0];
                this.ParentName = (string)parentInfo[1];
            }

            if (record.ContainsKey("ordinal"))
            {
                var x = record["ordinal"];
                this.Ordinal = (long)record["ordinal"];
            }
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public long? ParentId { get; set; }

        public string ParentName { get; set; }

        public long Ordinal { get; set; }
    }
}
