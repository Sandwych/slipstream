using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface IService
    {
        object Execute(
            string dbName, string objectName, string name, params object[] args);
    }
}
