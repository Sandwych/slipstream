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
                dynamic testObjectModel = context.Database.Resources["test.test_object"];
                var ids = testObjectModel.Search(context, null, 0, 0);
                if (ids.Length > 0)
                {
                    testObjectModel.Delete(context, ids);
                }

                var importer = new XmlDataImporter(context, "test");

                importer.Import(xmlStream);

                ids = testObjectModel.Search(context, null, 0, 0);
                Assert.AreEqual(3, ids.Length);

                var testObjectRecords = testObjectModel.Read(context, ids, null);
                Assert.AreEqual("name1", testObjectRecords[0]["name"]);
            }
        }

    }
}
