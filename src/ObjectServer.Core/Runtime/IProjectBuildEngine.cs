using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Runtime
{
    public interface IProjectBuildEngine
    {
        bool Build(string projPath, string outDir);
    }
}
