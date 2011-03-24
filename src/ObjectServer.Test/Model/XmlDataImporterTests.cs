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
        const string TestModelXmlResourcePath = "ObjectServer.Test.XmlFiles.test-model-data.xml";
        const string MasterChildXmlResourcePath = "ObjectServer.Test.XmlFiles.master-child-data.xml";

        [Test]
        public void Test_simple_importing()
        {
            this.ClearAllModelData();
            this.ClearTestModelTable();

            //删除所有记录
            dynamic testObjectModel = this.ResourceScope.GetResource("test.test_model");

            var domain = new object[][] { new object[] { "model", "=", "test.test_model" } };

            //第一遍更新，应该是插入3条，更新一条，总记录数 3 条
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(TestModelXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.ResourceScope, "test");

                importer.Import(xmlStream);

                var ids = testObjectModel.SearchInternal(this.ResourceScope);
                Assert.AreEqual(3, ids.Length);

                var domain1 = new object[][] { new object[] { "name", "=", "name_changed" } };
                ids = testObjectModel.SearchInternal(this.ResourceScope, domain1);
                Assert.AreEqual(1, ids.Length);
            }

            //再导入一遍，现在应该是更新3条，插入一条,因此 test_test_model 里的记录总数是4条
            using (var xmlStream = Assembly.GetExecutingAssembly()
             .GetManifestResourceStream(TestModelXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.ResourceScope, "test");
                importer.Import(xmlStream);
            }

            var ids2 = testObjectModel.SearchInternal(this.ResourceScope);
            Assert.AreEqual(4, ids2.Length);
        }


        [Test]
        public void Test_many2one_field_importing()
        {
            this.ClearMasterAndChildTable();
            this.ClearAllModelData();
            //删除所有导入记录

            dynamic childModel = this.ResourceScope.GetResource("test.child");
            dynamic masterModel = this.ResourceScope.GetResource("test.master");

            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(MasterChildXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.ResourceScope, "test");

                importer.Import(xmlStream);

                object[][] domain;

                domain = new object[][] { new object[] { "name", "=", "master1" } };
                var ids = masterModel.Search(this.ResourceScope, domain, null, 0, 0);
                Assert.AreEqual(1, ids.Length);
                dynamic master1 = masterModel.Browse(this.ResourceScope, ids[0]);
                Assert.AreEqual(2, master1.children.Length);
            }
        }

        private void ClearAllModelData()
        {
            var model = (IModel)this.ResourceScope.GetResource("core.model_data");

            ClearAllModelData(model, "test.master");
            ClearAllModelData(model, "test.child");
            ClearAllModelData(model, "test.test_model");
        }

        private void ClearAllModelData(IModel model, string modelName)
        {
            var domain = new object[][] { new object[] { "model", "=", modelName } };
            var ids = model.SearchInternal(this.ResourceScope, domain);
            if (ids != null && ids.Length > 0)
            {
                model.DeleteInternal(this.ResourceScope, ids);
            }
        }

    }
}
