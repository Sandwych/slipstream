using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ObjectServer.Runtime
{
    internal interface ICompiler
    {
        Assembly Compile(IEnumerable<string> sourceFiles);
    }
}
