using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using Mono.Options;

using NUnit.Framework;

namespace ObjectServer.Test
{
    [TestFixture]
    public class MonoOptionsTests
    {
        [Test]
        public void CanIgnoreUnknownOptions()
        {
            var unknownArg1 = "-c db_port=1234";
            var unknownArg2 = "-c db_user=demo";
            var args = new string[] {
                "--version", 
                "-h", 
                unknownArg1,
                unknownArg2,
            };
            var p = new OptionSet() {
                { "h|?|help", v => {} },
                { "version", v => {} },
            };

            var extras = p.Parse(args);
            Assert.AreEqual(2, extras.Count);
            Assert.AreEqual(unknownArg1, extras[0]);
            Assert.AreEqual(unknownArg2, extras[1]);
        }

        [Test]
        public void CanParseConfigurationOptions()
        {
            var unknownArg1 = "-c db_port=1234";
            var unknownArg2 = "-c db_user=demo";
            var args = new string[] {
                "--version", 
                "-h", 
                unknownArg1,
                unknownArg2,
            };
            var cfgItems = new List<string>();
            var p = new OptionSet() {
                { "h|?|help",  v => {} },
                { "version",   v => {} },
                { "c|cfg=",   v => { cfgItems.Add(v.Trim()); } },
            };

            var extras = p.Parse(args);
            Assert.AreEqual(0, extras.Count);
            Assert.AreEqual(unknownArg1.Substring(3), cfgItems[0]);
            Assert.AreEqual(unknownArg2.Substring(3), cfgItems[1]);
        }

    }
}
