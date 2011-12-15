using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace ObjectServer.Test.Web
{
    [Ignore]
    public class JsonSerializedCallingTests : RpcTestBase
    {
        [Ignore]
        public void CrudShouldBeSuccessfully()
        {
            var constraints = new object[][] 
            { 
                new object[] { "name", "=", "core.model"}
            };

            var result = JsonRpc("Execute", this.SessionToken, "core.model", "Search", constraints, 0, 0);
            var ids = (object[])result["result"];

            Assert.AreEqual(1, ids.Length);
        }
    }
}
