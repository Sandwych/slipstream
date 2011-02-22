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
        }

    }
}
