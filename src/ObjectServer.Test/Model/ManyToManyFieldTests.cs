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
    public class ManyToManyFieldTests : LocalTestCase
    {
        class IdCollection
        {
            public long eid1;
            public long eid2;
            public long eid3;
            public long eid4;
            public long eid5;

            public long did1;
            public long did2;
            public long did3;
            public long did4;
            public long did5;
        }



        [Test]
        public void Can_browse_m2m_field()
        {
            this.ClearManyToManyModels();
            var ids = this.GenerateTestData();

            dynamic employeeModel = this.ResourceScope.GetResource("test.employee");
            dynamic e1 = employeeModel.Browse(this.ResourceScope, ids.eid1);

            Assert.AreEqual(3, e1.departments.Length);
            Assert.AreEqual(e1.departments[0].id, ids.did2);
            Assert.AreEqual(e1.departments[1].id, ids.did3);
            Assert.AreEqual(e1.departments[2].id, ids.did4);

            dynamic e2 = employeeModel.Browse(this.ResourceScope, ids.eid2);
            Assert.AreEqual(2, e2.departments.Length);
            Assert.AreEqual(e2.departments[0].id, ids.did3);
            Assert.AreEqual(e2.departments[1].id, ids.did4);

        }

        [Test]
        public void Test_CRUD_m2m_field()
        {
            this.ClearManyToManyModels();
            var ids = this.GenerateTestData();

            var employees = this.Service.ReadModel(this.SessionId, "test.employee",
                new object[] { ids.eid1, ids.eid2 }, new object[] { "name", "departments" });

            Assert.AreEqual(2, employees.Length);
            var employee1 = employees[0];
            var employee2 = employees[1];

            Assert.IsInstanceOf<object[]>(employee1["departments"]);
            var departments1 = (object[])employee1["departments"];
            Assert.AreEqual(3, departments1.Length);
            Assert.AreEqual(ids.did2, ((object[])departments1[0])[0]);                
            Assert.AreEqual(ids.did3, ((object[])departments1[1])[0]);                
            Assert.AreEqual(ids.did4, ((object[])departments1[2])[0]);                
        }

        private IdCollection GenerateTestData()
        {
            var ids = new IdCollection();
            dynamic e = new ExpandoObject();
            e.name = "employee";
            ids.eid1 = this.Service.CreateModel(this.SessionId, "test.employee", e);
            ids.eid2 = this.Service.CreateModel(this.SessionId, "test.employee", e);
            ids.eid3 = this.Service.CreateModel(this.SessionId, "test.employee", e);
            ids.eid4 = this.Service.CreateModel(this.SessionId, "test.employee", e);
            ids.eid5 = this.Service.CreateModel(this.SessionId, "test.employee", e);

            dynamic dept = new ExpandoObject();
            dept.name = "department";
            ids.did1 = this.Service.CreateModel(this.SessionId, "test.department", dept);
            ids.did2 = this.Service.CreateModel(this.SessionId, "test.department", dept);
            ids.did3 = this.Service.CreateModel(this.SessionId, "test.department", dept);
            ids.did4 = this.Service.CreateModel(this.SessionId, "test.department", dept);
            ids.did5 = this.Service.CreateModel(this.SessionId, "test.department", dept);

            //设置e1 对应 did2, did3, did4
            this.Service.CreateModel(this.SessionId, "test.department_employee",
                new Dictionary<string, object>() { { "eid", ids.eid1 }, { "did", ids.did2 }, });
            this.Service.CreateModel(this.SessionId, "test.department_employee",
                new Dictionary<string, object>() { { "eid", ids.eid1 }, { "did", ids.did3 }, });
            this.Service.CreateModel(this.SessionId, "test.department_employee",
                new Dictionary<string, object>() { { "eid", ids.eid1 }, { "did", ids.did4 }, });

            //设置 e2  对应 did3 did4
            this.Service.CreateModel(this.SessionId, "test.department_employee",
                new Dictionary<string, object>() { { "eid", ids.eid2 }, { "did", ids.did3 }, });
            this.Service.CreateModel(this.SessionId, "test.department_employee",
                new Dictionary<string, object>() { { "eid", ids.eid2 }, { "did", ids.did4 }, });

            return ids;
        }
    }
}
