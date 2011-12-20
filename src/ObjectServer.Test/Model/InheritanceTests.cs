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
    public class InheritanceTests : ServiceContextTestCaseBase
    {
        private const string InitName = "animal_name";
        private const string InitDogfood = "bone";

        private long PrepareTestingData()
        {
            var dogModel = this.GetResource("test.dog");

            dynamic dog = new ExpandoObject();
            dog.name = InitName;
            dog.dogfood = InitDogfood;
            return dogModel.Create(dog);
        }

        [SetUp]
        public void ClearData()
        {
            this.ClearModel("test.dog");
            this.ClearModel("test.animal");
        }

        [Test]
        public void CheckMultiTablesInheritance()
        {
            dynamic batModel = this.GetResource("test.bat");
            Assert.AreEqual(2, batModel.Inheritances.Count);
        }

        [Test]
        public void CanOverrideSingleTableTransaction()
        {
            dynamic inheritedModel = this.GetResource("test.single_table");
            Assert.True(inheritedModel.Fields.ContainsKey("age"));

            var propBag = new Dictionary<string, object>()
                {
                    { "name", "inherited" },
                    { "age", 44},
                };

            object id = inheritedModel.Create(propBag);

            var record = inheritedModel.Read(new object[] { id }, null)[0];
            Assert.AreEqual(33, record["age"]);
        }

        [Test]
        public void CanCreateMultiTable()
        {
            this.ClearData();

            dynamic dog = new ExpandoObject();
            dog.name = "oldblue";
            dog.dogfood = "pear";

            var animalModel = this.GetResource("test.animal");
            var dogModel = this.GetResource("test.dog");
            long id = dogModel.Create(dog);
            var ids = dogModel.Search(null, null, 0, 0);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(id, ids[0]);
            Assert.AreEqual(1, animalModel.Count(null));

            var dogObj = dogModel.Browse(id);
            var animal = animalModel.Browse(dogObj.animal._id);
            Assert.AreEqual(dog.name, animal.name);
        }

        [Test]
        public void CanCreateAndReadMultiTable()
        {
            var dogModel = this.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);

            var dog = dogModel.Read(new long[] { id }, null)[0];
            Assert.AreEqual(InitName, (string)dog["name"]);
            Assert.AreEqual(InitDogfood, (string)dog["dogfood"]);
        }

        [Test]
        public void CanCreateAndBrowseMultiTable()
        {
            var dogModel = this.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);

            var dog = dogModel.Browse(id);
            Assert.AreEqual(InitName, dog.name);
            Assert.AreEqual(InitDogfood, dog.dogfood);
        }


        [Test]
        public void CanDeleteMultiTable()
        {
            var animalModel = this.GetResource("test.animal");
            var dogModel = this.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);
            dogModel.Delete(new long[] { id });
            Assert.AreEqual(0, animalModel.Count(null));
            Assert.AreEqual(0, dogModel.Count(null));
        }

        [Test]
        public void CanWriteMultiTable()
        {
            var dogModel = this.GetResource("test.dog");
            this.ClearData();
            var id = this.PrepareTestingData();
            Assert.That(id > 0);

            dynamic fieldValues = new ExpandoObject();
            fieldValues.name = "oldyellow";
            fieldValues.dogfood = "apple";
            dogModel.Write(id, fieldValues);

            var dog = dogModel.Read(new long[] { id }, null)[0];
            Assert.AreEqual("oldyellow", (string)dog["name"]);
            Assert.AreEqual("apple", (string)dog["dogfood"]);
        }

        [Test]
        public void CanSearchMultiTable()
        {
            var animalModel = this.GetResource("test.animal");
            var dogModel = this.GetResource("test.dog");

            this.ClearData();
            var id = this.PrepareTestingData();

            Assert.That(id > 0);

            var constraints = new object[][]
            { 
                new object[] { "name", "=", InitName } 
            };
            var animalIds = animalModel.Search(constraints, null, 0, 0);
            var dogIds = dogModel.Search(constraints, null, 0, 0);

            Assert.AreEqual(1, animalIds.Length);
            Assert.AreEqual(1, dogIds.Length);
        }

        [Test]
        public void CheckThreeLevelMultiInheritances()
        {
            var batman = this.GetResource("test.batman");

            //是否有父表的字段
            Assert.That(batman.Fields.ContainsKey("sucker")); //test.bat            

            //是否有祖辈表的字段
            Assert.That(batman.Fields.ContainsKey("wings")); //test.flyable
            Assert.That(batman.Fields.ContainsKey("name")); //test.animal
        }

    }
}
