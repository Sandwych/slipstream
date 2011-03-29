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
            dynamic inheritedModel = this.ServiceScope.GetResource("test.single_table");
            Assert.True(inheritedModel.Fields.ContainsKey("age"));

            var propBag = new Dictionary<string, object>()
                {
                    { "name", "inherited" },
                    { "age", 44},
                };

            object id = inheritedModel.Create(this.ServiceScope, propBag);

            var record = inheritedModel.Read(this.ServiceScope, new object[] { id }, null)[0];

            Assert.AreEqual(33, record["age"]);
        }

        [Test]
        public void Test_multitable_create()
        {
            this.ClearMultiTable();

            dynamic adminUserRecord = new ExpandoObject();
            adminUserRecord.name = "admin_user_name";
            adminUserRecord.admin_info = "admin_user_info";

            var adminUserModel = (IModel)this.ServiceScope
                .GetResource("test.admin_user");
            long id = -1;
            Assert.DoesNotThrow(() =>
            {
                id = adminUserModel.CreateInternal(this.ServiceScope, adminUserRecord);
            });
            var ids = adminUserModel.SearchInternal(this.ServiceScope);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(id, ids[0]);
        }

        [Test]
        public void Test_multitable_creation_and_reading()
        {
            var adminUserModel = (IModel)this.ServiceScope
                .GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.That(id > 0);

            var adminUser = adminUserModel.ReadInternal(this.ServiceScope, new long[] { id })[0];
            Assert.AreEqual("admin_user_name", (string)adminUser["name"]);
            Assert.AreEqual("admin_user_info", (string)adminUser["admin_info"]);
        }

        [Test]
        public void Test_multitable_creation_and_browsing()
        {
            var adminUserModel = (IModel)this.ServiceScope
                .GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.That(id > 0);

            var adminUser = adminUserModel.Browse(this.ServiceScope, id);
            Assert.AreEqual("admin_user_name", adminUser.name);
            Assert.AreEqual("admin_user_info", adminUser.admin_info);
        }


        [Test]
        public void Test_multitable_deletion()
        {
            var adminUserModel = (IModel)this.ServiceScope
                .GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.That(id > 0);
            Assert.DoesNotThrow(() =>
            {
                adminUserModel.DeleteInternal(this.ServiceScope, new long[] { id });
            });

        }

        [Test]
        public void Test_multitable_writing()
        {
            var adminUserModel = (IModel)this.ServiceScope
                .GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.That(id > 0);

            dynamic fieldValues = new ExpandoObject();
            fieldValues.name = "changed_base_name";
            fieldValues.admin_info = "changed_admin_info";

            Assert.DoesNotThrow(() =>
                {
                    adminUserModel.WriteInternal(this.ServiceScope, id, fieldValues);
                });

            var adminUserRecord = adminUserModel.ReadInternal(this.ServiceScope, new long[] { id })[0];

            Assert.AreEqual("changed_base_name", (string)adminUserRecord["name"]);
            Assert.AreEqual("changed_admin_info", (string)adminUserRecord["admin_info"]);
        }

        [Test]
        public void Test_multitable_searching()
        {
            var adminUserModel = (IModel)this.ServiceScope
                .GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.That(id > 0);

            var domain = new object[][]
            { 
                new object[] { "name", "=", "admin_user_name" } 
            };

            var ids = adminUserModel.SearchInternal(this.ServiceScope, domain);

            Assert.AreEqual(1, ids.Length);
        }

        private long AddMultiTableTestData()
        {
            dynamic adminUserRecord = new ExpandoObject();
            adminUserRecord.name = "admin_user_name";
            adminUserRecord.admin_info = "admin_user_info";

            var adminUserModel = (IModel)this.ServiceScope
                .GetResource("test.admin_user");

            return adminUserModel.CreateInternal(this.ServiceScope, adminUserRecord);
        }

        private void ClearMultiTable()
        {
            this.ClearModel(this.ServiceScope, "test.admin_user");
            this.ClearModel(this.ServiceScope, "test.user");
        }

    }
}
