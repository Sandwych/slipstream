using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using SlipStream.Model;

namespace SlipStream.Model.Fields
{
    [TestFixture]
    public class FunctionalFieldTests : ServiceContextTestCaseBase
    {
        private const string ModelName = "test.functional_field_object";

        private dynamic PrepareTestData()
        {
            dynamic data = new ExpandoObject();
            dynamic model = this.GetResource(ModelName);

            data.record1 = new ExpandoObject();
            data.record1.name = "test1";
            data.record1.field1 = 1;
            data.record1.field2 = 2;
            data.record1_id = model.Create(data.record1);

            data.record2 = new ExpandoObject();
            data.record2.name = "test2";
            data.record2.field1 = 5;
            data.record2.field2 = 4;
            data.record2_id = model.Create(data.record2);

            return data;
        }

        [SetUp]
        public void ClearTestData()
        {
            dynamic model = this.GetResource(ModelName);
            long[] ids = (long[])model.Search(null, null, 0, 0);
            var idsToDel = ids.Select(o => (object)o).ToArray();
            model.Delete(idsToDel);
        }

        [Test]
        public void CanUseFunctionalManyToOneField()
        {
            var constraints = new object[][] { new object[] { "login", "=", "root" } };
            dynamic userModel = this.GetResource("core.user");
            dynamic model = this.GetResource(ModelName);
            dynamic ids = userModel.Search(constraints, null, 0, 0);

            dynamic data = PrepareTestData();

            dynamic records = model.Read(new object[] { data.record1_id }, null);

            var userField0 = (object[])records[0]["user"];
            Assert.AreEqual(ids[0], userField0[0]);
        }

        [Test]
        public void CanFunctionalFieldAsConstraint()
        {
            dynamic model = this.GetResource(ModelName);
            var constraint = new object[][] { new object[] { "sum_field", "=", 9 } };
            dynamic data = PrepareTestData();

            dynamic n = model.Count(constraint);
            Assert.AreEqual(1, n);
        }
    }
}
