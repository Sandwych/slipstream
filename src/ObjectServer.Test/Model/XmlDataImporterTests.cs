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
        const string ReferenceFieldXmlResourcePath = "ObjectServer.Test.XmlFiles.reference-field-data.xml";

        [Test]
        public void Test_simple_importing()
        {
            this.ClearAllModelData();
            this.ClearTestModelTable();

            //删除所有记录
            dynamic testObjectModel = this.TransactionContext.GetResource("test.test_model");

            var constraints = new object[][] { new object[] { "model", "=", "test.test_model" } };

            //第一遍更新，应该是插入3条，更新一条，总记录数 3 条
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(TestModelXmlResourcePath))
            using (var ctx = new TransactionContext(TestingDatabaseName, this.SessionId))
            {
                var importer = new XmlDataImporter(ctx, "test");

                importer.Import(xmlStream);

                var ids = testObjectModel.SearchInternal(this.TransactionContext);
                Assert.AreEqual(3, ids.Length);

                var domain1 = new object[][] { new object[] { "name", "=", "name_changed" } };
                ids = testObjectModel.SearchInternal(this.TransactionContext, domain1);
                Assert.AreEqual(1, ids.Length);
            }

            //再导入一遍，现在应该是更新3条，插入一条,因此 test_test_model 里的记录总数是4条
            using (var xmlStream = Assembly.GetExecutingAssembly()
             .GetManifestResourceStream(TestModelXmlResourcePath))
            using (var ctx = new TransactionContext(TestingDatabaseName, this.SessionId))
            {
                var importer = new XmlDataImporter(ctx, "test");
                importer.Import(xmlStream);
            }

            var ids2 = testObjectModel.SearchInternal(this.TransactionContext);
            Assert.AreEqual(4, ids2.Length);
        }


        [Test]
        public void Test_many2one_field_importing()
        {
            this.ClearMasterAndChildTable();
            this.ClearAllModelData();
            //删除所有导入记录

            dynamic childModel = this.TransactionContext.GetResource("test.child");
            dynamic masterModel = this.TransactionContext.GetResource("test.master");

            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(MasterChildXmlResourcePath))
            using (var ctx = new TransactionContext(TestingDatabaseName, this.SessionId))
            {
                var importer = new XmlDataImporter(ctx, "test");

                importer.Import(xmlStream);

                object[][] constraints;

                constraints = new object[][] { new object[] { "name", "=", "master1" } };
                var ids = masterModel.Search(this.TransactionContext, constraints, null, 0, 0);
                Assert.AreEqual(1, ids.Length);
                dynamic master1 = masterModel.Browse(this.TransactionContext, ids[0]);
                Assert.AreEqual(2, master1.children.Length);
            }
        }


        [Test]
        public void Test_reference_field_importing()
        {
            this.ClearMasterAndChildTable();
            this.ClearAllModelData();
            //删除所有导入记录

            dynamic testModel = this.TransactionContext.GetResource("test.test_model");
            dynamic masterModel = this.TransactionContext.GetResource("test.master");

            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(ReferenceFieldXmlResourcePath))
            using (var ctx = new TransactionContext(TestingDatabaseName, this.SessionId))
            {
                var importer = new XmlDataImporter(ctx, "test");

                importer.Import(xmlStream);

                var masterIds = masterModel.Search(this.TransactionContext, null, null, 0, 0);
                Assert.AreEqual(1, masterIds.Length);

                var testModelIds = testModel.Search(this.TransactionContext, null, null, 0, 0);
                Assert.AreEqual(1, testModelIds.Length);

                dynamic testModelRecord = testModel.Browse(this.TransactionContext, testModelIds[0]);
                Assert.AreEqual("master1", testModelRecord.reference_field.name);
            }
        }

        private void ClearAllModelData()
        {
            var model = (IModel)this.TransactionContext.GetResource("core.model_data");

            ClearAllModelData(model, "test.master");
            ClearAllModelData(model, "test.child");
            ClearAllModelData(model, "test.test_model");

            ClearModel("test.child");
            ClearModel("test.master");
            ClearModel("test.test_model");
        }

        private void ClearAllModelData(IModel model, string modelName)
        {
            var constraints = new object[][] { new object[] { "model", "=", modelName } };
            var ids = model.SearchInternal(this.TransactionContext, constraints);
            if (ids != null && ids.Length > 0)
            {
                model.DeleteInternal(this.TransactionContext, ids);
            }
        }

    }
}
