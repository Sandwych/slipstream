using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using NUnit.Framework;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class XmlDataImporterTests : LocalTestBase
    {
        const string XmlResourcePath = "ObjectServer.Test.Model.data.xml";

        [Test]
        public void Test_simple_importing()
        {
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(XmlResourcePath))
            using (var context = new ContextScope(new Guid(this.SessionId)))
            {
                //删除所有记录
                dynamic testObjectModel = context.Database["test.test_object"];
                this.ClearModelDataTable(context);
                this.ClearTestObjectTable(context, testObjectModel);

                var importer = new XmlDataImporter(context, "test");

                importer.Import(xmlStream);

                var ids = testObjectModel.Search(context, null, 0, 0);
                Assert.AreEqual(3, ids.Length);

                var testObjectRecords = testObjectModel.Read(context, ids, null);
                Assert.AreEqual("name1", testObjectRecords[0]["name"]);
            }
        }


        [Test]
        public void Test_many2one_field_importing()
        {
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(XmlResourcePath))
            using (var context = new ContextScope(new Guid(this.SessionId)))
            {
                //删除所有记录
                this.ClearModelDataTable(context);
                this.ClearMasterAndChildTable(context);
                dynamic childModel = context.Database["test.child"];
                dynamic masterModel = context.Database["test.master"];

                var importer = new XmlDataImporter(context, "test");

                importer.Import(xmlStream);

                object[][] domain;

                domain = new object[][] { new object[] { "name", "=", "master1" } };
                var ids = masterModel.Search(context, domain, 0, 0);
                Assert.AreEqual(1, ids.Length);
                dynamic master1 = masterModel.Browse(context, ids[0]);
                Assert.AreEqual(2, master1.children.Length);
            }
        }


       


    }
}
