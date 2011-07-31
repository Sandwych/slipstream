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
        [Test]
        public void Test_create_nodes()
        {
            dynamic data = this.PrepareTestingData();
            var model = (IModel)this.ServiceScope.GetResource("test.category");

            //确认 _left 与 _right 设置正确
            var fields = new string[] { "name", "_left", "_right", "children", "descendants" };

            var record1 = model.ReadInternal(this.ServiceScope, new long[] { data.id1 }, fields)[0];
            Assert.AreEqual((long)1, record1["_left"]);
            Assert.AreEqual((long)2, record1["_right"]);

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


            var record3 = model.ReadInternal(this.ServiceScope, new long[] { data.id3 }, fields)[0];
            Assert.AreEqual((long)4, record3["_left"]);
            Assert.AreEqual((long)7, record3["_right"]);

            var record4 = model.ReadInternal(this.ServiceScope, new long[] { data.id4 }, fields)[0];
            Assert.AreEqual((long)8, record4["_left"]);
            Assert.AreEqual((long)9, record4["_right"]);

            var record5 = model.ReadInternal(this.ServiceScope, new long[] { data.id5 }, fields)[0];
            Assert.AreEqual((long)5, record5["_left"]);
            Assert.AreEqual((long)6, record5["_right"]);
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
            //2
            //|....3
            //     |....5
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
