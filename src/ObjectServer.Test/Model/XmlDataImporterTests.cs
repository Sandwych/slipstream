using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using NUnit.Framework;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class XmlDataImporterTests : ServiceContextTestCaseBase
    {
        const string TestModelXmlResourcePath = "ObjectServer.Test.XmlFiles.test-model-data.xml";
        const string MasterChildXmlResourcePath = "ObjectServer.Test.XmlFiles.master-child-data.xml";
        const string ReferenceFieldXmlResourcePath = "ObjectServer.Test.XmlFiles.reference-field-data.xml";

        [Test]
        public void CanImportSimpleData()
        {
            this.ClearAllModelData();
            this.ClearTestModelTable();

            //删除所有记录
            dynamic testObjectModel = this.Context.GetResource("test.test_model");

            var constraints = new object[][] { new object[] { "model", "=", "test.test_model" } };

            //第一遍更新，应该是插入3条，更新一条，总记录数 3 条
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(TestModelXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");

                importer.Import(xmlStream);

                var ids = testObjectModel.Search(this.Context, null, null, 0, 0);
                Assert.AreEqual(3, ids.Length);

                var domain1 = new object[][] { new object[] { "name", "=", "name_changed" } };
                ids = testObjectModel.Search(this.Context, domain1, null, 0, 0);
                Assert.AreEqual(1, ids.Length);
            }

            //再倒入一遍
            using (var xmlStream = Assembly.GetExecutingAssembly()
             .GetManifestResourceStream(TestModelXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");
                importer.Import(xmlStream);
            }

            var ids2 = testObjectModel.Search(this.Context, null, null, 0, 0);
            Assert.AreEqual(3, ids2.Length);
        }


        [Test]
        public void CanImportManyToOneField()
        {
            this.ClearMasterAndChildTable();
            this.ClearAllModelData();
            //删除所有导入记录

            dynamic childModel = this.Context.GetResource("test.child");
            dynamic masterModel = this.Context.GetResource("test.master");

            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(MasterChildXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");

                importer.Import(xmlStream);

                object[][] constraints;

                constraints = new object[][] { new object[] { "name", "=", "master1" } };
                var ids = masterModel.Search(this.Context, constraints, null, 0, 0);
                Assert.AreEqual(1, ids.Length);
                dynamic master1 = masterModel.Browse(this.Context, ids[0]);
                Assert.AreEqual(2, master1.children.Length);
            }
        }


        [Test]
        public void CanImportReferenceField()
        {
            this.ClearMasterAndChildTable();
            this.ClearAllModelData();
            //删除所有导入记录

            dynamic testModel = this.Context.GetResource("test.test_model");
            dynamic masterModel = this.Context.GetResource("test.master");

            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(ReferenceFieldXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");

                importer.Import(xmlStream);

                var masterIds = masterModel.Search(this.Context, null, null, 0, 0);
                Assert.AreEqual(1, masterIds.Length);

                var testModelIds = testModel.Search(this.Context, null, null, 0, 0);
                Assert.AreEqual(1, testModelIds.Length);

                dynamic testModelRecord = testModel.Browse(this.Context, testModelIds[0]);
                Assert.AreEqual("master1", testModelRecord.reference_field.name);
            }
        }

        private void ClearAllModelData()
        {
            var model = (IModel)this.Context.GetResource("core.model_data");

            ClearAllModelData(model, "test.master");
            ClearAllModelData(model, "test.child");
            ClearAllModelData(model, "test.test_model");

            ClearModel("test.child");
            ClearModel("test.master");
            ClearModel("test.test_model");
        }

        private void ClearAllModelData(dynamic model, string modelName)
        {
            var constraints = new object[][] { new object[] { "model", "=", modelName } };
            var ids = model.Search(this.Context, constraints, null, 0, 0);
            if (ids != null && ids.Length > 0)
            {
                model.Delete(this.Context, ids);
            }
        }

    }
}
