using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IField
    {
        string Name { get; }
        string Label { get; }
        string SqlType { get; }
        string Type { get; }
        int Size { get; }
        bool Required { get; }
    }
}
