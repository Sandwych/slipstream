using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace ObjectServer.Utility.Test
{
    class Model
    {
        public string Name { get; set; }
        public string[] Depend { get; set; }
    }


    [TestFixture]
    public class DependencySorterTest
    {

        /// <summary>
        /// 测试依赖排序类
        /// </summary>
        [Test]
        public void Test_dependency_sorter()
        {
            var models = new Model[]
            {
                new Model()
                {
                    Name = "menu",
                    Depend = new string[] { "user", "group" }
                },
                new Model()
                {
                    Name = "user",
                    Depend = new string[] { }
                },
                new Model()
                {
                    Name = "action",
                    Depend = new string[] { "menu" }
                },
                new Model()
                {
                    Name = "group",
                    Depend = new string[] { "user" }
                },              
            };
            var sortedModels = DependencySorter<Model, string>.DependencySort(models, m => m.Name, m => m.Depend);

            Assert.AreEqual("user", sortedModels[0].Name);
            Assert.AreEqual("group", sortedModels[1].Name);
            Assert.AreEqual("menu", sortedModels[2].Name);
            Assert.AreEqual("action", sortedModels[3].Name);

        }

    }
}
