using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IServiceObject
    {
        void Initialize(IDatabase db);

        string Name { get; }

        MethodInfo GetServiceMethod(string name);

        bool DatabaseRequired { get; }

    }
}
