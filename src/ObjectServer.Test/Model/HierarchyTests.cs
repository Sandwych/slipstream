using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using NUnit.Framework;

using ObjectServer.Sql;
using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class HierarchyTests : LocalTestCase
    {
        private static readonly string[] AssertNodeFields = { "name", "_left", "_right", "children", "descendants" };

        private void AssertNode(long id, long left, long right)
        {
            var model = (IModel)this.ServiceScope.GetResource("test.category");
            var records = model.ReadInternal(this.ServiceScope, new long[] { id }, AssertNodeFields);
            Assert.AreEqual(1, records.Length);
            var record = records[0];
            Assert.AreEqual(left, (long)record["_left"]);
            Assert.AreEqual(right, (long)record["_right"]);
        }

        [Test]
        public void Test_create_nodes()
        {
            dynamic data = this.PrepareTestingData();
            var model = (IModel)this.ServiceScope.GetResource("test.category");

            //确认 _left 与 _right 设置正确
            var fields = new string[] { "name", "_left", "_right", "children", "descendants" };

            AssertNode(data.id1, 1, 2);

            var record2 = model.ReadInternal(this.ServiceScope, new long[] { data.id2 }, fields)[0];
            Assert.AreEqual((long)3, record2["_left"]);
            Assert.AreEqual((long)10, record2["_right"]);
            var children2 = (long[])record2["children"];
            Array.Sort(children2);
            Assert.AreEqual(2, children2.Length);
            Assert.AreEqual(data.id3, children2[0]);
            Assert.AreEqual(data.id4, children2[1]);
            var descendants2 = (long[])record2["descendants"];
            Array.Sort(descendants2);
            Assert.AreEqual(3, descendants2.Length);

            AssertNode(data.id3, 4, 7);
            AssertNode(data.id4, 8, 9);
            AssertNode(data.id5, 5, 6);
        }

        [Test]
        public void Test_delete_nodes()
        {
            dynamic data = this.PrepareTestingData();
            var model = (IModel)this.ServiceScope.GetResource("test.category");

            model.DeleteInternal(this.ServiceScope, new long[] { data.id3, data.id1 });

            var ids = model.SearchInternal(this.ServiceScope);
            Assert.AreEqual(2, ids.Length);
            Array.Sort<long>(ids);
            Assert.AreEqual(data.id2, ids[0]);
            Assert.AreEqual(data.id4, ids[1]);

            AssertNode(data.id2, 1, 4);
            AssertNode(data.id4, 2, 3);
        }

        [Test]
        public void Test_change_parent()
        {
            dynamic data = this.PrepareTestingData();
            var model = (IModel)this.ServiceScope.GetResource("test.category");

            //把node5 的父节点改成 node2
            dynamic record = new ExpandoObject();
            record.parent = data.id2;
            model.WriteInternal(this.ServiceScope, data.id5, record);

            var ids = model.SearchInternal(this.ServiceScope);
            Assert.AreEqual(5, ids.Length);
            //TODO 测试
        }

        [Test]
        public void Test_childof()
        {
            dynamic data = this.PrepareTestingData();

            var model = (IModel)this.ServiceScope.GetResource("test.category");

            var domain1 = new object[][] 
            { 
                new object[] { "_id", "childof", data.id2 }
            };

            var ids1 = model.SearchInternal(
                this.ServiceScope, domain1,
                new OrderExpression[] { new OrderExpression("_id", SortDirection.Asc) });

            Assert.AreEqual(3, ids1.Length);
            Assert.AreEqual(data.id3, ids1[0]);
            Assert.AreEqual(data.id4, ids1[1]);
            Assert.AreEqual(data.id5, ids1[2]);

            var domain2 = new object[][] 
            { 
                new object[] { "_id", "childof", data.id3 }
            };

            var ids2 = model.SearchInternal(
                this.ServiceScope, domain2,
                new OrderExpression[] { new OrderExpression("_id", SortDirection.Asc) });

            Assert.AreEqual(1, ids2.Length);
            Assert.AreEqual(data.id5, ids2[0]);
        }


        private dynamic PrepareTestingData()
        {
            this.ClearModel(this.ServiceScope, "test.category");
            var model = (IModel)this.ServiceScope.GetResource("test.category");

            dynamic data = new ExpandoObject();
            //1
            //
            //2
            //|....3
            //      |....5
            //
            //|....4
            //插入4个根节点，1，2作为根节点，3,4是2的子节点，5 是3的子节点
            data.root1 = new ExpandoObject();
            data.root1.name = "root1";
            data.id1 = this.Service.CreateModel(this.SessionId, "test.category", data.root1);

            data.root2 = new ExpandoObject();
            data.root2.name = "root2";
            data.id2 = this.Service.CreateModel(this.SessionId, "test.category", data.root2);

            //插入节点3的时候节点2还是叶子
            data.root3 = new ExpandoObject();
            data.root3.name = "root3";
            data.root3.parent = data.id2;
            data.id3 = this.Service.CreateModel(this.SessionId, "test.category", data.root3);

            data.root4 = new ExpandoObject();
            data.root4.name = "root4";
            data.root4.parent = data.id2;
            data.id4 = this.Service.CreateModel(this.SessionId, "test.category", data.root4);

            data.root5 = new ExpandoObject();
            data.root5.name = "root5";
            data.root5.parent = data.id3;
            data.id5 = this.Service.CreateModel(this.SessionId, "test.category", data.root5);

            return data;
        }

    }
}
