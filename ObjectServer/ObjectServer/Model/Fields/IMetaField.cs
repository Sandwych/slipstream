using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IMetaField
    {
        string Name { get; }
        string Label { get; set; }
        bool Functional { get; }
        FieldGetter Getter { get; set; }
        FieldDefaultProc DefaultProc { get; set; }
        FieldType Type { get; }
        int Size { get; set; }
        bool Required { get; set; }
        string Relation { get; set; }
        string OriginField { get; set; }
        string RelatedField { get; set; }
        bool Internal { get; }

        IDictionary<string, string> Options { get; }

        bool IsStorable();

        void Validate();

        Dictionary<long, object> GetFieldValues(
            IContext callingContext, List<Dictionary<string, object>> records);
    }
}
