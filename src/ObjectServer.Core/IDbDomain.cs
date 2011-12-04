using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Data;

namespace ObjectServer
{
    public interface IDbDomain : IDisposable, IResourceContainer
    {

        void Initialize(bool isUpdate);

    }
}
