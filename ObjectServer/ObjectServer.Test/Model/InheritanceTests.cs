using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class InheritanceTests : LocalTestCase
    {
        [Test]
        public void Test_single_table()
        {
            dynamic inheritedModel = this.ResourceScope.DatabaseProfile.GetResource("test.single_table");
            Assert.True(inheritedModel.Fields.ContainsKey("age"));

            var propBag = new Dictionary<string, object>()
                {
                    { "name", "inherited" },
                    { "age", 44},
                };

            object id = inheritedModel.Create(this.ResourceScope, propBag);

            var record = inheritedModel.Read(this.ResourceScope, new object[] { id }, null)[0];

            Assert.AreEqual(33, record["age"]);
        }

        [Test]
        public void Test_multitable_create()
        {
            this.ClearMultiTable();

            dynamic adminUserRecord = new ExpandoObject();
            adminUserRecord.name = "admin_user_name";
            adminUserRecord.admin_info = "admin_user_info";

            var adminUserModel = (IMetaModel)this.ResourceScope.DatabaseProfile.GetResource("test.admin_user");
            Assert.DoesNotThrow(() =>
            {
                adminUserModel.CreateInternal(this.ResourceScope, adminUserRecord);
            });
        }

        private void AddMultiTableTestData()
        {
        }

        private void ClearMultiTable()
        {
            this.ClearModel(this.ResourceScope, "test.admin_user");
            this.ClearModel(this.ResourceScope, "test.user");
        }

    }
}
