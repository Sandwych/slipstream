using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    internal class FieldBase
    {

        public FieldBase(string name, string relation, string label, string type, string help)
        {
            this.Name = name;
            this.Relation = relation;
            this.Label = label;
            this.Type = type;
            this.Help = help;
        }

        public string Name { get; private set; }

        public string Relation { get; private set; }

        public string Label { get; private set; }

        public string Type { get; private set; }

        public string Help { get; private set; }
    }
}
