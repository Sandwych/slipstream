using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface IObjectPool
    {
        string Database { get; }

        void AddServiceObject(IServiceObject obj);

        IServiceObject this[string objName] { get; }

        bool Contains(string objName);
    }
}
