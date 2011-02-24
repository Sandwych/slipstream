using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer
{
    public abstract class TestBase
    {
        [TestFixtureSetUp()]
        public virtual void InitFramework()
        {
            if (!ObjectServerStarter.Initialized)
            {
                ObjectServerStarter.Initialize();
            }

            var proxy = new LocalService();

            this.SessionId = proxy.LogOn("objectserver", "root", "root");
        }

        public string SessionId { get; private set; }

    }
}
