using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

using Xunit;

namespace ObjectServer.Test
{
    public class ConfigurationTest
    {

        [Fact]
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

            var cfg = JsonConvert.DeserializeObject<Configuration>(cfgText);
            Assert.NotNull(cfg);

        }
    }
}
