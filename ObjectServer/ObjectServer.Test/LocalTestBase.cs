using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer
{
    public abstract class LocalTestBase
    {
        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            if (!ObjectServerStarter.Initialized)
            {
                ObjectServerStarter.Initialize();
            }

            var service = ObjectServerStarter.ExportedService;

            this.SessionId = service.LogOn("objectserver", "root", "root");
        }

        [TestFixtureTearDown]
        public virtual void DisposeFramework()
        {
            var service = ObjectServerStarter.ExportedService;
            service.LogOff(this.SessionId);
        }

        public string SessionId { get; private set; }

    }
}
