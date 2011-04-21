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
    public class HierarchyTests : LocalTestCase
    {
        [Test]
        public void Test_create_nodes()
        {
            this.ClearModel(this.ServiceScope, "test.category");

            var model = (IModel)this.ServiceScope.GetResource("test.category");

            //1
            //2
            //|....3
            //     |....5
            //|....4
            //插入4个根节点，1，2作为根节点，3,4是2的子节点，5 是3的子节点
            dynamic root1 = new ExpandoObject();
            root1.name = "root1";
            long id1 = model.CreateInternal(this.ServiceScope, root1);

            dynamic root2 = new ExpandoObject();
            root2.name = "root2";
            long id2 = model.CreateInternal(this.ServiceScope, root2);

            //插入节点3的时候节点2还是叶子
            dynamic root3 = new ExpandoObject();
            root3.name = "root3";
            root3.parent = id2;
            long id3 = model.CreateInternal(this.ServiceScope, root3);

            dynamic root4 = new ExpandoObject();
            root4.name = "root4";
            root4.parent = id2;
            long id4 = model.CreateInternal(this.ServiceScope, root4);

            dynamic root5 = new ExpandoObject();
            root5.name = "root5";
            root5.parent = id3;
            long id5 = model.CreateInternal(this.ServiceScope, root5);


            //确认 _left 与 _right 设置正确
            var fields = new string[] { "name", "_left", "_right", "children", "descendants" };

            var record1 = model.ReadInternal(this.ServiceScope, new long[] { id1 }, fields)[0];
            Assert.AreEqual((long)1, record1["_left"]);
            Assert.AreEqual((long)2, record1["_right"]);

            var record2 = model.ReadInternal(this.ServiceScope, new long[] { id2 }, fields)[0];
            Assert.AreEqual((long)3, record2["_left"]);
            Assert.AreEqual((long)10, record2["_right"]);
            var children2 = (long[])record2["children"];
            Array.Sort(children2);
            Assert.AreEqual(2, children2.Length);
            Assert.AreEqual(id3, children2[0]);
            Assert.AreEqual(id4, children2[1]);
            var descendants2 = (long[])record2["descendants"];
            Array.Sort(descendants2);
            Assert.AreEqual(3, descendants2.Length);


            var record3 = model.ReadInternal(this.ServiceScope, new long[] { id3 }, fields)[0];
            Assert.AreEqual((long)4, record3["_left"]);
            Assert.AreEqual((long)7, record3["_right"]);

            var record4 = model.ReadInternal(this.ServiceScope, new long[] { id4 }, fields)[0];
            Assert.AreEqual((long)8, record4["_left"]);
            Assert.AreEqual((long)9, record4["_right"]);

            var record5 = model.ReadInternal(this.ServiceScope, new long[] { id5 }, fields)[0];
            Assert.AreEqual((long)5, record5["_left"]);
            Assert.AreEqual((long)6, record5["_right"]);
        }

        [Test]
        public void Test_childof()
        {
            this.ClearModel(this.ServiceScope, "test.category");
            var model = (IModel)this.ServiceScope.GetResource("test.category");
            //1
            //2
            //|....3
            //     |....5
            //|....4
            //插入4个根节点，1，2作为根节点，3,4是2的子节点，5 是3的子节点
            dynamic root1 = new ExpandoObject();
            root1.name = "root1";
            long id1 = model.CreateInternal(this.ServiceScope, root1);

            dynamic root2 = new ExpandoObject();
            root2.name = "root2";
            long id2 = model.CreateInternal(this.ServiceScope, root2);

            //插入节点3的时候节点2还是叶子
            dynamic root3 = new ExpandoObject();
            root3.name = "root3";
            root3.parent = id2;
            long id3 = model.CreateInternal(this.ServiceScope, root3);

            dynamic root4 = new ExpandoObject();
            root4.name = "root4";
            root4.parent = id2;
            long id4 = model.CreateInternal(this.ServiceScope, root4);

            dynamic root5 = new ExpandoObject();
            root5.name = "root5";
            root5.parent = id3;
            long id5 = model.CreateInternal(this.ServiceScope, root5);

            var domain1 = new object[][] 
            { 
                new object[] { "id", "childof", id2 }
            };

            var ids1 = model.SearchInternal(
                this.ServiceScope, domain1,
                new OrderInfo[] { new OrderInfo("id", SearchOrder.Asc) });

            Assert.AreEqual(3, ids1.Length);
            Assert.AreEqual(id3, ids1[0]);
            Assert.AreEqual(id4, ids1[1]);
            Assert.AreEqual(id5, ids1[2]);

            var domain2 = new object[][] 
            { 
                new object[] { "id", "childof", id3 }
            };

            var ids2 = model.SearchInternal(
                this.ServiceScope, domain2,
                new OrderInfo[] { new OrderInfo("id", SearchOrder.Asc) });

            Assert.AreEqual(1, ids2.Length);
            Assert.AreEqual(id5, ids2[0]);
        }

    }
}
