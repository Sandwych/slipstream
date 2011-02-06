using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ObjectServer
{
    public interface IServiceObject
    {

        string Name { get; }

        MethodInfo GetServiceMethod(string name);

    }
}
