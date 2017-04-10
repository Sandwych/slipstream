using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using NUnit.Framework;

namespace SlipStream.Entity.Test
{
    [TestFixture(Category = "DataImporter")]
    public class XmlDataImporterTests : ServiceContextTestCaseBase
    {
        const string TestModelXmlResourcePath = "SlipStream.Test.XmlFiles.test-model-data.xml";
        const string MasterChildXmlResourcePath = "SlipStream.Test.XmlFiles.master-child-data.xml";
        const string ReferenceFieldXmlResourcePath = "SlipStream.Test.XmlFiles.reference-field-data.xml";

        [Test]
        public void CanImportSimpleData()
        {
            this.ClearAllModelData();
            this.ClearTestEntityTable();

            //删除所有记录
            dynamic testObjectModel = this.GetResource("test.test_entity");

            var constraints = new object[][] { new object[] { "entity", "=", "test.test_entity" } };

            //第一遍更新，应该是插入3条，更新一条，总记录数 3 条
            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(TestModelXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");

                importer.Import(xmlStream);

                var ids = testObjectModel.Search(null, null, 0, 0);
                Assert.AreEqual(3, ids.Length);

                var domain1 = new object[][] { new object[] { "name", "=", "name_changed" } };
                ids = testObjectModel.Search(domain1, null, 0, 0);
                Assert.AreEqual(1, ids.Length);
            }

            //再倒入一遍
            using (var xmlStream = Assembly.GetExecutingAssembly()
             .GetManifestResourceStream(TestModelXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");
                importer.Import(xmlStream);
            }

            var ids2 = testObjectModel.Search(null, null, 0, 0);
            Assert.AreEqual(3, ids2.Length);
        }


        [Test]
        public void CanImportManyToOneField()
        {
            this.ClearMasterAndChildTable();
            this.ClearAllModelData();
            //删除所有导入记录

            dynamic childModel = this.GetResource("test.child");
            dynamic masterModel = this.GetResource("test.master");

            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(MasterChildXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");

                importer.Import(xmlStream);

                object[][] constraints;

                constraints = new object[][] { new object[] { "name", "=", "master1" } };
                var ids = masterModel.Search(constraints, null, 0, 0);
                Assert.AreEqual(1, ids.Length);
                dynamic master1 = masterModel.Browse(ids[0]);
                Assert.AreEqual(2, master1.children.Length);
            }
        }


        [Test]
        public void CanImportReferenceField()
        {
            this.ClearMasterAndChildTable();
            this.ClearAllModelData();
            //删除所有导入记录

            dynamic testModel = this.GetResource("test.test_entity");
            dynamic masterModel = this.GetResource("test.master");

            using (var xmlStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(ReferenceFieldXmlResourcePath))
            {
                var importer = new XmlDataImporter(this.Context, "test");

                importer.Import(xmlStream);

                var masterIds = masterModel.Search(null, null, 0, 0);
                Assert.AreEqual(1, masterIds.Length);

                var testModelIds = testModel.Search(null, null, 0, 0);
                Assert.AreEqual(1, testModelIds.Length);

                dynamic testModelRecord = testModel.Browse(testModelIds[0]);
                Assert.AreEqual("master1", testModelRecord.reference_field.name);
            }
        }

        private void ClearAllModelData()
        {
            var model = (IEntity)this.GetResource("core.entity_data");

            ClearAllModelData(model, "test.master");
            ClearAllModelData(model, "test.child");
            ClearAllModelData(model, "test.test_entity");

            ClearEntity("test.child");
            ClearEntity("test.master");
            ClearEntity("test.test_entity");
        }

        private void ClearAllModelData(dynamic model, string modelName)
        {
            var constraints = new object[][] { new object[] { "entity", "=", modelName } };
            var ids = model.Search(constraints, null, 0, 0);
            if (ids != null && ids.Length > 0)
            {
                model.Delete(ids);
            }
        }

    }
}
