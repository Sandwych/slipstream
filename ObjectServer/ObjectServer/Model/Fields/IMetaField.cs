using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IMetaField
    {
        string Name { get; }
        string Label { get; }
        bool Functional { get; }
        FieldGetter Getter { get; }
        FieldDefaultProc DefaultProc { get; }
        FieldType Type { get; }
        int Size { get; }
        bool Required { get; }
        string Relation { get; }
        string OriginField { get; }
        string RelatedField { get; }
        bool Internal { get; }

        bool IsStorable();

        void Validate();
    }
}
