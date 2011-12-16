using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Malt.Utility;
using Malt;

namespace ObjectServer.Utility.Test
{
    public sealed class Model
    {
        public string Name { get; set; }
        public string[] Depend { get; set; }
    }


    [TestFixture]
    public class DependencySorterTest
    {
        [Datapoint]
        public static readonly Model[] TestData =
            new Model[]
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

        [Datapoint]
        public static readonly Model[] EmptyModelSet = new Model[] { };

        /// <summary>
        /// 测试依赖排序类
        /// </summary>
        [Theory]
        public void Test_dependency_sorter(Model[] models)
        {
            Assume.That(models != null);

            models.DependencySort(m => m.Name, m => m.Depend);

            if (models.Length > 0)
            {
                Assert.AreEqual("user", models[0].Name);
                Assert.AreEqual("group", models[1].Name);
                Assert.AreEqual("menu", models[2].Name);
                Assert.AreEqual("action", models[3].Name);
            }
        }

    }
}
