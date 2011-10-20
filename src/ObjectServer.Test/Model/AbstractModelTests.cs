using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;
using System.Data.Common;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{

    [TestFixture]
    public class AbstractModelTests : TransactionContextTestCaseBase
    {
        [Test]
        public void ShouldHandleWithBadConstraints()
        {
            dynamic model = this.GetResource("core.model");
            var constraints = new object[] {
                new object[] { "kk", "=", 13 },
            };

            Assert.Throws<ArgumentException>(delegate
            {
                var ids = model.Search(this.TransactionContext, constraints, null, 0, 0);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                var ids = model.Count(this.TransactionContext, constraints);
            });

        }

        [Test]
        public void CanGetFields()
        {
            var modelName = "core.user";
            dynamic userModel = this.GetResource(modelName);
            var result = userModel.GetFields(this.TransactionContext, null);
            var records = ((object[])result).Select(i => (Dictionary<string, object>)i);

            Assert.IsTrue(records.Any());
        }

        [Test]
        public void CheckVersionIncrement()
        {
            dynamic masterModel = this.GetResource("test.master");
            this.ClearModel("test.master");

            dynamic m1 = new ExpandoObject();
            m1.name = "master1";

            //创建第一笔记录
            var id1 = masterModel.Create(this.TransactionContext, m1);
            var record1 = this.GetMasterRecordByName("master1");

            Assert.That(record1.ContainsKey(AbstractModel.VersionFieldName));
            Assert.AreEqual(0, record1[AbstractModel.VersionFieldName]);

            //修改
            var name11 = "master1'1";
            record1["name"] = name11;
            masterModel.Write(this.TransactionContext, record1[AbstractModel.IdFieldName], record1);
            record1 = this.GetMasterRecordByName(name11);

            Assert.That(record1.ContainsKey(AbstractModel.VersionFieldName));
            Assert.AreEqual(1, record1[AbstractModel.VersionFieldName]);

            //错误的版本号
            long ver = (long)record1[AbstractModel.VersionFieldName];
            ver--;

            record1[AbstractModel.VersionFieldName] = ver;
            Assert.Throws<Exceptions.ConcurrencyException>(delegate
            {
                masterModel.Write(this.TransactionContext, record1[AbstractModel.IdFieldName], record1);
            });
        }

        [Test]
        public void CanHandleCreationWithUnexistedColumn()
        {
            dynamic masterModel = this.GetResource("test.master");
            this.ClearModel("test.master");

            dynamic m1 = new ExpandoObject();
            m1.name = "master1";
            m1.age = 1;

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                masterModel.Create(this.TransactionContext, m1);
            });
        }

        [Test]
        public void CanHandleWritingWithUnexistedColumn()
        {
            dynamic masterModel = this.GetResource("test.master");
            this.ClearModel("test.master");

            dynamic m1 = new ExpandoObject();
            m1.name = "master1";
            long id = (long)masterModel.Create(this.TransactionContext, m1);
            dynamic records = masterModel.Read(this.TransactionContext, new long[] { id }, null);

            dynamic m2 = new ExpandoObject();
            m2._version = records[0]["_version"];
            m2.age = 12;

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                masterModel.Write(this.TransactionContext, id, m2);
            });
        }

        [Test]
        public void CheckGetFieldDefaultValues()
        {
            dynamic testModel = this.GetResource("test.test_model");
            var booleanFieldName = "boolean_field";
            var fields = new string[] { booleanFieldName };

            var defaultValues = testModel.GetFieldDefaultValues(this.TransactionContext, fields);

            Assert.AreEqual(1, defaultValues.Count);
            Assert.AreEqual(true, defaultValues[booleanFieldName]);
        }

        private Dictionary<string, object> GetMasterRecordByName(string name)
        {
            var fields = new string[] { "name", AbstractModel.VersionFieldName };
            dynamic masterModel = this.GetResource("test.master");
            var constraint = new object[][] {
                new object[] { "name", "=", name }
            };
            long[] ids = (long[])masterModel.Search(this.TransactionContext, constraint, null, 0, 0);
            var records = (Dictionary<string, object>[])masterModel.Read(this.TransactionContext, ids, fields);
            return records.First();
        }
    }
}
