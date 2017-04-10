using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;

namespace SlipStream.Test.Web
{
    public class JsonSerializedCallingTests : RpcTestBase
    {
        public void CrudShouldBeSuccessfully()
        {
            var constraints = new object[][] 
            { 
                new object[] { "name", "=", "core.meta_entity"}
            };

            var result = JsonRpc("Execute", this.SessionToken, "core.meta_entity", "Search", constraints, 0, 0);
            var ids = (object[])result["result"];

            Assert.AreEqual(1, ids.Length);
        }
    }
}
