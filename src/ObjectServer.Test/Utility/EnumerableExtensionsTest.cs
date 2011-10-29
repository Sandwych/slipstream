using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Utility.Test
{
    [TestFixture]
    public class EnumerableExtensionsTest
    {

        [Test]
        public void Test_ToHex_Method()
        {
            var bytes = new byte[] { 0x33, 0x44, 0x45, 0x67, 0x2A, 0x96 };
            Assert.AreEqual("334445672A96", bytes.ToHex());

            var bytes1 = new byte[] { 0x33 };
            Assert.AreEqual("33", bytes1.ToHex());

            var bytes0 = new byte[] { };
            Assert.AreEqual("", bytes0.ToHex());
        }

        [Test]
        public void Test_ToCommaList_Method()
        {
            var ints = new int[] { 11, 22, 33, 44, 55 };
            Assert.AreEqual("11,22,33,44,55", ints.ToCsv());

            var ints1 = new int[] { 11 };
            Assert.AreEqual("11", ints1.ToCsv());

            var ints0 = new int[] { };
            Assert.AreEqual("", ints0.ToCsv());
        }
    }
}
