using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using NUnit.Framework;

namespace ObjectServer.Test
{
    [TestFixture]
    public class ConfigurationTest
    {

        [Test]
        public void Deserialize_configuration()
        {
            var cfgText = @"
{
    ""db-type"": ""Postgresql"",
    ""db-host"": ""localhost"",
    ""db-port"": 5432,
    ""db-user"": ""objectserver"",
    ""db-password"": ""objectserver"",
    ""module-path"": ""c:\\objectserver-modules"",
}
";

            var cfg = JsonConvert.DeserializeObject<Config>(cfgText);
            Assert.NotNull(cfg);
            Assert.AreEqual(ObjectServer.Backend.DatabaseType.Postgresql, cfg.DbType);

        }
    }
}
