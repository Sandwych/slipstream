using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IFieldInfo
    {
        string Name { get; }
        string Label { get; }
        string SqlType { get; }
        string Type { get; }
        int Size { get; }
    }
}
