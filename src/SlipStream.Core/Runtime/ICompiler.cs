using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SlipStream.Runtime
{
    public interface ICompiler
    {
        Assembly BuildProject(string projectFile);
    }
}
