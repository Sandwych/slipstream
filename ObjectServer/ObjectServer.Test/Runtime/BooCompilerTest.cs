using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Xunit;

using ObjectServer.Runtime;

namespace ObjectServer.Test.Runtime
{
    public class BooCompilerTest
    {

        [Fact]
        public void TestBooCompiler()
        {

            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var booFile = Path.Combine(dir, "Test.boo");

            var a = Assembly.GetExecutingAssembly();
            using (var ins = a.GetManifestResourceStream("Test.boo"))
            using (var outs = File.Create(booFile))
            {
                int b = 0;
                while ((b = ins.ReadByte()) >= 0)
                {
                    outs.WriteByte((byte)b);
                }
            }

            var booCompiler = new BooCompiler();

            var assembly = booCompiler.Compile(new string[] { booFile });
            Assert.NotNull(assembly);
        }
    }
}
