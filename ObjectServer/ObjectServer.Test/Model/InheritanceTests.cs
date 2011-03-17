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

            var adminUserModel = (IMetaModel)this.ResourceScope
                .DatabaseProfile.GetResource("test.admin_user");
            Assert.DoesNotThrow(() =>
                {
                    adminUserModel.CreateInternal(this.ResourceScope, adminUserRecord);
                });
        }

        [Test]
        public void Test_multitable_creation_and_reading()
        {
            var adminUserModel = (IMetaModel)this.ResourceScope
                .DatabaseProfile.GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.IsTrue(id > 0);

            var adminUser = adminUserModel.ReadInternal(this.ResourceScope, new long[] { id })[0];
            Assert.AreEqual("admin_user_name", (string)adminUser["name"]);
            Assert.AreEqual("admin_user_info", (string)adminUser["admin_info"]);
        }

        [Test]
        public void Test_multitable_creation_and_browsing()
        {
            var adminUserModel = (IMetaModel)this.ResourceScope
                .DatabaseProfile.GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.IsTrue(id > 0);

            var adminUser = adminUserModel.Browse(this.ResourceScope, id);
            Assert.AreEqual("admin_user_name", adminUser.name);
            Assert.AreEqual("admin_user_info", adminUser.admin_info);
        }


        [Test]
        public void Test_multitable_deletion()
        {
            var adminUserModel = (IMetaModel)this.ResourceScope
                .DatabaseProfile.GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.IsTrue(id > 0);
            Assert.DoesNotThrow(() =>
            {
                adminUserModel.DeleteInternal(this.ResourceScope, new long[] { id });
            });

        }

        [Test]
        public void Test_multitable_writing()
        {
            var adminUserModel = (IMetaModel)this.ResourceScope
                .DatabaseProfile.GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.IsTrue(id > 0);

            dynamic fieldValues = new ExpandoObject();
            fieldValues.name = "changed_base_name";
            fieldValues.admin_info = "changed_admin_info";

            Assert.DoesNotThrow(() =>
                {
                    adminUserModel.WriteInternal(this.ResourceScope, id, fieldValues);
                });

            var adminUserRecord = adminUserModel.ReadInternal(this.ResourceScope, new long[] { id })[0];

            Assert.AreEqual("changed_base_name", (string)adminUserRecord["name"]);
            Assert.AreEqual("changed_admin_info", (string)adminUserRecord["admin_info"]);
        }

        [Test]
        public void Test_multitable_searching()
        {
            var adminUserModel = (IMetaModel)this.ResourceScope
                .DatabaseProfile.GetResource("test.admin_user");
            this.ClearMultiTable();
            var id = this.AddMultiTableTestData();
            Assert.IsTrue(id > 0);

            var domain = new object[][]
            { 
                new object[] { "name", "=", "admin_user_name" } 
            };

            var ids = adminUserModel.SearchInternal(this.ResourceScope, domain);

            Assert.AreEqual(1, ids.Length);
        }

        private long AddMultiTableTestData()
        {
            dynamic adminUserRecord = new ExpandoObject();
            adminUserRecord.name = "admin_user_name";
            adminUserRecord.admin_info = "admin_user_info";

            var adminUserModel = (IMetaModel)this.ResourceScope
                .DatabaseProfile.GetResource("test.admin_user");

            return adminUserModel.CreateInternal(this.ResourceScope, adminUserRecord);
        }

        private void ClearMultiTable()
        {
            this.ClearModel(this.ResourceScope, "test.admin_user");
            this.ClearModel(this.ResourceScope, "test.user");
        }

    }
}
