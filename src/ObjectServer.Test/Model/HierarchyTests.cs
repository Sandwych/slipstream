using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics;

using NUnit.Framework;

using ObjectServer.Model;

namespace ObjectServer.Model.Test
{
    [TestFixture]
    public class HierarchyTests : TransactionContextTestCaseBase
    {
        private static readonly string[] AssertNodeFields = { "name", "_left", "_right", "_children", "_descendants", "parent" };

        private IDictionary<string, object> ReadNode(long id)
        {
            var catModel = this.GetResource("test.category");
            dynamic records = catModel.Read(this.TransactionContext, new object[] { id }, AssertNodeFields);
            Assert.AreEqual(1, records.Length);
            var record = records[0];
            return (IDictionary<string, object>)record;
        }

        private void AssertChildren(long id, params long[] childIds)
        {
            var record = this.ReadNode(id);
            var ids = (long[])record["_children"];
            Assert.AreEqual(childIds.Length, ids.Length);
            var hash1 = new List<long>(childIds);
            hash1.Sort();
            var hash2 = new List<long>(ids);
            hash2.Sort();
            Assert.That(hash1.SequenceEqual(hash2));
        }

        private void AssertDescendants(long id, params long[] descendantIds)
        {
            var record = this.ReadNode(id);
            var ids = (long[])record["_descendants"];
            Assert.AreEqual(descendantIds.Length, ids.Length);
            var hash1 = new List<long>(descendantIds);
            hash1.Sort();
            var hash2 = new List<long>(ids);
            hash2.Sort();
            Assert.That(hash1.SequenceEqual(hash2));
        }


        [Test]
        public void CanCreateNodes()
        {
            dynamic data = this.PrepareTestingData();
            dynamic catModel = this.GetResource("test.category");

            //确认 _left 与 _right 设置正确
            var fields = AssertNodeFields;
            this.AssertChildren(data.id2, data.id3, data.id4);
            this.AssertChildren(data.id3, data.id5);
            this.AssertDescendants(data.id2, data.id3, data.id4, data.id5);
        }

        [Test]
        public void CanCreateNodesWithNullParent()
        {
            dynamic data = this.PrepareTestingData();
            dynamic catModel = this.GetResource("test.category");

            dynamic node1 = new ExpandoObject();
            node1.name = "a node with null parent";
            node1.parent = null;
            var id1 = (long)catModel.Create(this.TransactionContext, node1);
            var record1 = this.ReadNode(id1);
            Assert.That((long)record1["_left"] == (long)record1["_right"] - 1);
            Assert.IsNull(record1["parent"]);

            dynamic node2 = new ExpandoObject();
            node2.name = "a node without parent field";
            var id2 = catModel.Create(this.TransactionContext, node2);
            var record2 = this.ReadNode(id2);
            Assert.That((long)record2["_left"] == (long)record2["_right"] - 1);
            Assert.IsNull(record2["parent"]);
        }

        [Test]
        public void CanDeleteNodes()
        {
            dynamic data = this.PrepareTestingData();
            dynamic catModel = this.GetResource("test.category");

            //删掉 3 和 1
            catModel.Delete(this.TransactionContext, new long[] { data.id3, data.id1 });

            this.AssertChildren(data.id2, data.id4);
            this.AssertDescendants(data.id2, data.id4);
        }

        [Test]
        public void CanChangeParent1()
        {
            dynamic data = this.PrepareTestingData();
            dynamic catModel = this.GetResource("test.category");

            //把node5 的父节点改成 node2
            dynamic record = new ExpandoObject();
            record.parent = data.id2;
            catModel.Write(this.TransactionContext, data.id5, record);

            AssertChildren(data.id2, data.id3, data.id4, data.id5);
        }

        [Test]
        public void CanChangeParent2()
        {
            dynamic data = this.PrepareTestingData();
            dynamic catModel = this.GetResource("test.category");

            //把node3 的父节点改成 node1
            dynamic record = new ExpandoObject();
            record.parent = data.id1;
            catModel.Write(this.TransactionContext, data.id3, record);

            dynamic ids = catModel.Search(this.TransactionContext, null, null, 0, 0);
            Assert.AreEqual(5, ids.Length);

            this.AssertChildren(data.id1, data.id3);
            this.AssertChildren(data.id3, data.id5);
            this.AssertDescendants(data.id1, data.id3, data.id5);
            this.AssertChildren(data.id2, data.id4);
            this.AssertDescendants(data.id2, data.id4);
        }

        [Test]
        public void CanChangeParentToNull()
        {
            dynamic data = this.PrepareTestingData();
            dynamic catModel = this.GetResource("test.category");

            //把节点3的上级设成 null
            dynamic node3 = new ExpandoObject();
            node3.parent = null;
            catModel.Write(this.TransactionContext, data.id3, node3);

            this.AssertChildren(data.id2, data.id4);
            this.AssertDescendants(data.id2, data.id4);
            this.AssertChildren(data.id3, data.id5);
        }

        [Test]
        public void CanChangeFromNull()
        {
            dynamic data = this.PrepareTestingData();
            dynamic catModel = this.GetResource("test.category");

            data.data6 = new ExpandoObject();
            data.data6.name = "node6";
            data.data6.parent = null;
            data.id6 = catModel.Create(this.TransactionContext, data.data6);

            Assert.That(data.id6 > 0);

            //把6 的上级改成 5
            data.data6.parent = data.id5;
            catModel.Write(this.TransactionContext, data.id6, data.data6);

            this.AssertChildren(data.id5, data.id6);
            this.AssertDescendants(data.id5, data.id6);
        }

        [Test]
        public void CanUseChildOfOperator()
        {
            dynamic catModel = this.GetResource("test.category");
            dynamic data = this.PrepareTestingData();

            var domain1 = new object[][] 
            { 
                new object[] { "_id", "childof", data.id2 }
            };

            var orders = new object[] { new object[] { "_id", "ASC" } };
            dynamic ids1 = catModel.Search(this.TransactionContext, domain1, orders, 0, 0);

            Assert.AreEqual(3, ids1.Length);
            Assert.AreEqual(data.id3, ids1[0]);
            Assert.AreEqual(data.id4, ids1[1]);
            Assert.AreEqual(data.id5, ids1[2]);

            var domain2 = new object[][] 
            { 
                new object[] { "_id", "childof", data.id3 }
            };

            dynamic ids2 = catModel.Search(this.TransactionContext, domain2, orders, 0, 0);

            Assert.AreEqual(1, ids2.Length);
            Assert.AreEqual(data.id5, ids2[0]);
        }

        [Test]
        public void CanUseNotChildOfOperator()
        {
            dynamic catModel = this.GetResource("test.category");
            dynamic data = this.PrepareTestingData();

            var domain1 = new object[][] 
            { 
                new object[] { "_id", "!childof", data.id2 }
            };

            var orders = new object[] { new object[] { "_id", "ASC" } };
            var ids1 = (long[])catModel.Search(this.TransactionContext, domain1, orders, 0, 0);

            Assert.AreEqual(2, ids1.Length);
            Assert.AreEqual(data.id1, ids1[0]);
            Assert.AreEqual(data.id2, ids1[1]);
        }

        [Test]
        public void ShouldThrowsWhenWritingRecursived()
        {
            dynamic catModel = this.GetResource("test.category");
            dynamic data = this.PrepareTestingData();

            dynamic record = new ExpandoObject();
            record.parent = data.id3;
            Assert.Throws<Exceptions.DataException>(delegate
            {
                catModel.Write(this.TransactionContext, data.id2, record);
            });

            record.parent = data.id2;
            Assert.Throws<Exceptions.DataException>(delegate
            {
                catModel.Write(this.TransactionContext, data.id2, record);
            });

            record.parent = data.id5;
            Assert.Throws<Exceptions.DataException>(delegate
            {
                catModel.Write(this.TransactionContext, data.id2, record);
            });
        }


        private dynamic PrepareTestingData()
        {
            dynamic catModel = this.GetResource("test.category");
            this.ClearModel("test.category");

            dynamic data = new ExpandoObject();
            //|1
            //|
            //|+2
            //  |--3
            //     |--5
            //
            //  |....4
            //插入4个根节点，1，2作为根节点，3,4是2的子节点，5 是3的子节点
            data.node1 = new ExpandoObject();
            data.node1.name = "node1";
            data.id1 = catModel.Create(this.TransactionContext, data.node1);

            data.node2 = new ExpandoObject();
            data.node2.name = "node2";
            data.id2 = catModel.Create(this.TransactionContext, data.node2);

            //插入节点3的时候节点2还是叶子
            data.node3 = new ExpandoObject();
            data.node3.name = "node3";
            data.node3.parent = data.id2;
            data.id3 = catModel.Create(this.TransactionContext, data.node3);

            data.node4 = new ExpandoObject();
            data.node4.name = "node4";
            data.node4.parent = data.id2;
            data.id4 = catModel.Create(this.TransactionContext, data.node4);

            data.node5 = new ExpandoObject();
            data.node5.name = "node5";
            data.node5.parent = data.id3;
            data.id5 = catModel.Create(this.TransactionContext, data.node5);

            return data;
        }

    }
}
