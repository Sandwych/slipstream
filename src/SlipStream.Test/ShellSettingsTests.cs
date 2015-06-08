using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Mono.Options;

using NUnit.Framework;
using SlipStream.Core;
using SlipStream;

namespace SlipStream.Test
{
    [TestFixture]
    public class ShellSettingsTests
    {
        [Test]
        public void TestDefaultModulesPath()
        {
            var ss = new ShellSettings();
            Assert.IsTrue(ss.ModulePath.EndsWith("Modules"));
            System.Diagnostics.Debug.WriteLine(ss.ModulePath);
        }
    }
}
