using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using NUnit.Framework;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class XmlDataImporterTests : LocalTestCase
    {
        const string XmlResourcePath = "ObjectServer.Test.Model.data.xml";

        [Test]
        public void Test_simple_importing()
        {
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(XmlResourcePath))
            {
                //删除所有记录
                dynamic testObjectModel = this.ResourceScope.DatabaseProfile.GetResource("test.test_model");
                this.ClearTestModelTable();

                var importer = new XmlDataImporter(this.ResourceScope, "test");

                importer.Import(xmlStream);

                var ids = testObjectModel.SearchInternal(this.ResourceScope, null, 0, 0);
                Assert.AreEqual(3, ids.Length);

                var testObjectRecords = testObjectModel.ReadInternal(this.ResourceScope, ids, null);
                Assert.AreEqual("name1", testObjectRecords[0]["name"]);
            }
        }


        [Test]
        public void Test_many2one_field_importing()
        {
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(XmlResourcePath))
            {
                //删除所有记录
                this.ClearMasterAndChildTable();
                dynamic childModel = this.ResourceScope.DatabaseProfile.GetResource("test.child");
                dynamic masterModel = this.ResourceScope.DatabaseProfile.GetResource("test.master");

                var importer = new XmlDataImporter(this.ResourceScope, "test");

                importer.Import(xmlStream);

                object[][] domain;

                domain = new object[][] { new object[] { "name", "=", "master1" } };
                var ids = masterModel.Search(this.ResourceScope, domain, 0, 0);
                Assert.AreEqual(1, ids.Length);
                dynamic master1 = masterModel.Browse(this.ResourceScope, ids[0]);
                Assert.AreEqual(2, master1.children.Length);
            }
        }

    }
}
