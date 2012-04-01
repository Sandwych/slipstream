using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using NUnit.Framework;

namespace ObjectServer.Runtime
{
    [TestFixture]
    public class DynamicProjectBuilderTests
    {


        [Test]
        public void TestParseProjectFile()
        {
            var modulesBaseDir = Environment.CurrentDirectory;
            var projFile = Path.Combine(modulesBaseDir, "DemoModule.csproj.test");
            Assert.IsTrue(File.Exists(projFile));

            var parser = new DefaultProjectFileParser();
            var pfd = parser.Parse(projFile);
            foreach (var x in pfd.References)
            {
                Console.WriteLine("Full={0},Path={1}, ReferenceType={2}, simple={3}",
                    x.FullName, x.Path, x.ReferenceType, x.SimpleName);
                Console.WriteLine(Path.Combine(modulesBaseDir, x.SimpleName + ".dll"));
            }

        }
    }
}
