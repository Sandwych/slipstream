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

            //插入3个根节点，1，2作为根节点，3是2的子节点
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


            //确认 _left 与 _right 设置正确
            var fields = new string[] { "name", "_left", "_right" };

            var record1 = model.ReadInternal(this.ServiceScope, new long[] { id1 }, fields)[0];
            Assert.AreEqual((long)0, record1["_left"]);
            Assert.AreEqual((long)1, record1["_right"]);

            var record2 = model.ReadInternal(this.ServiceScope, new long[] { id2 }, fields)[0];
            Assert.AreEqual((long)2, record2["_left"]);
            Assert.AreEqual((long)5, record2["_right"]);

            var record3 = model.ReadInternal(this.ServiceScope, new long[] { id3 }, fields)[0];
            Assert.AreEqual((long)3, record3["_left"]);
            Assert.AreEqual((long)4, record3["_right"]);
        }


    }
}
