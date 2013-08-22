using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Data;

namespace SlipStream
{
    public interface IDbDomain : IDisposable, IResourceContainer
    {

        void Initialize(bool isUpdate);
        IServiceContext OpenSession(string sessionToken);
        IServiceContext OpenSystemSession();
        IServiceContext CurrentSession { get; }
        IDataProvider DataProvider { get; }
    }
}
