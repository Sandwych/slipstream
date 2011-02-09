using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

namespace ObjectServer
{
    public interface IServiceObject
    {
        void Initialize(Backend.Database db);

        string Name { get; }

        MethodInfo GetServiceMethod(string name);

    }
}
