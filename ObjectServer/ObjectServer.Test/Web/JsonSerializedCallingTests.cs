using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;

using NUnit.Framework;

namespace ObjectServer.Test.Web
{
    [Ignore]
    public class JsonSerializedCallingTests : WebTestBase
    {
        [Ignore]
        public void Test_json_serialized_CRUD()
        {
            var domain = new object[][] 
            { 
                new object[] { "name", "=", "core.model"}
            };

            var result = JsonRpc("Execute", this.SessionId, "core.model", "Search", domain, 0, 0);
            var ids = (object[])result["result"];

            Assert.AreEqual(1, ids.Length);
        }
    }
}
