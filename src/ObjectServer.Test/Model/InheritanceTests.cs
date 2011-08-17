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
        private const string InitName = "animal_name";
        private const string InitDogfood = "bone";

        private long PrepareTestingData()
        {
            dynamic dog = new ExpandoObject();
            dog.name = InitName;
            dog.dogfood = InitDogfood;

            var dogModel = (IModel)this.ServiceScope.GetResource("test.dog");

            return dogModel.CreateInternal(this.ServiceScope, dog);
        }

        [SetUp]
        public void ClearData()
        {
            this.ClearModel(this.ServiceScope, "test.dog");
            this.ClearModel(this.ServiceScope, "test.animal");
        }

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
            this.ClearData();

            dynamic dog = new ExpandoObject();
            dog.name = "oldblue";
            dog.dogfood = "pear";

            var dogModel = (IModel)this.ServiceScope.GetResource("test.dog");
            long id = -1;
            Assert.DoesNotThrow(() =>
            {
                id = dogModel.CreateInternal(this.ServiceScope, dog);
            });
            var ids = dogModel.SearchInternal(this.ServiceScope);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(id, ids[0]);
            Assert.AreEqual(1, this.Service.CountModel(this.SessionId, "test.animal"));
        }

        [Test]
        public void Test_multitable_creation_and_reading()
        {
            var dogModel = (IModel)this.ServiceScope.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);

            var dog = dogModel.ReadInternal(this.ServiceScope, new long[] { id })[0];
            Assert.AreEqual(InitName, (string)dog["name"]);
            Assert.AreEqual(InitDogfood, (string)dog["dogfood"]);
        }

        [Test]
        public void Test_multitable_creation_and_browsing()
        {
            var dogModel = (IModel)this.ServiceScope.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);

            var dog = dogModel.Browse(this.ServiceScope, id);
            Assert.AreEqual(InitName, dog.name);
            Assert.AreEqual(InitDogfood, dog.dogfood);
        }


        [Test]
        public void Test_multitable_deletion()
        {
            var dogModel = (IModel)this.ServiceScope.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);
            Assert.DoesNotThrow(() =>
            {
                dogModel.DeleteInternal(this.ServiceScope, new long[] { id });
            });
            Assert.AreEqual(0, this.Service.CountModel(this.SessionId, "test.animal"));
            Assert.AreEqual(0, this.Service.CountModel(this.SessionId, "test.dog"));
        }

        [Test]
        public void Test_multitable_writing()
        {
            var dogModel = (IModel)this.ServiceScope.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);

            dynamic fieldValues = new ExpandoObject();
            fieldValues.name = "oldyellow";
            fieldValues.dogfood = "apple";

            Assert.DoesNotThrow(() =>
                {
                    dogModel.WriteInternal(this.ServiceScope, id, fieldValues);
                });

            var dog = dogModel.ReadInternal(this.ServiceScope, new long[] { id })[0];

            Assert.AreEqual("oldyellow", (string)dog["name"]);
            Assert.AreEqual("apple", (string)dog["dogfood"]);
        }

        [Test]
        public void Test_multitable_searching()
        {
            var animalModel = (IModel)this.ServiceScope.GetResource("test.animal");
            var dogModel = (IModel)this.ServiceScope.GetResource("test.dog");

            this.ClearData();
            var id = this.PrepareTestingData();

            Assert.That(id > 0);

            var constraints = new object[][]
            { 
                new object[] { "name", "=", InitName } 
            };

            var animalIds = animalModel.SearchInternal(this.ServiceScope, constraints);
            var dogIds = dogModel.SearchInternal(this.ServiceScope, constraints);

            Assert.AreEqual(1, animalIds.Length);
            Assert.AreEqual(1, dogIds.Length);
        }

    }
}
