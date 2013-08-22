using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Test
{

    //////////////////// 继承单表测试的表 ///////////////////
    [Resource]
    public sealed class SingleTableBaseModel : AbstractSqlModel
    {
        public SingleTableBaseModel()
            : base("test.single_table")
        {
            IsVersioned = false;
            Fields.Chars("name").SetLabel("Name").Required().SetSize(64);
        }
    }


    [Resource]
    public sealed class SingleTableInheritedModel : AbstractExtendedModel
    {
        public SingleTableInheritedModel()
            : base("test.single_table")
        {
            Fields.Integer("age").SetLabel("Age");
        }


        [ServiceMethod("Create")]
        public static long Create(IModel model, IDictionary<string, object> propertyBag)
        {
            var record = new Dictionary<string, object>(propertyBag);
            record["age"] = 33;
            return model.CreateInternal(record);
        }
    }


    ////////////////////// 测试多表继承的表 /////////////////

    [Resource]
    public sealed class TestAnimalModel : AbstractSqlModel
    {
        public TestAnimalModel()
            : base("test.animal")
        {
            IsVersioned = false;
            Fields.Chars("name").SetLabel("Name");
        }
    }

    /// <summary>
    /// dog 继承自 animal
    /// </summary>
    [Resource]
    public sealed class TestDogModel : AbstractSqlModel
    {
        public TestDogModel()
            : base("test.dog")
        {
            IsVersioned = false;
            Inherit("test.animal", "animal");

            Fields.ManyToOne("animal", "test.dog").Required().OnDelete(OnDeleteAction.Cascade)
                .SetLabel("Base Animal Model");
            Fields.Chars("dogfood").SetLabel("Favorite Dogfood");
        }
    }

    /// <summary>
    /// cocker dog 继承自 dog
    /// </summary>
    [Resource]
    public sealed class TestCockerModel : AbstractSqlModel
    {
        public TestCockerModel()
            : base("test.cocker")
        {
            IsVersioned = false;
            Inherit("test.dog", "dog");

            Fields.ManyToOne("dog", "test.dog").Required().OnDelete(OnDeleteAction.Cascade)
                .SetLabel("Base Animal Model");
            Fields.Chars("color").SetLabel("Color");
        }
    }

    /************* 演示多继承 ***************/

    [Resource]
    public sealed class TestFlyableModel : AbstractSqlModel
    {
        public TestFlyableModel()
            : base("test.flyable")
        {
            IsVersioned = false;
            Fields.Integer("wings").SetLabel("Wings");
        }
    }

    [Resource]
    public sealed class TestBatModel : AbstractSqlModel
    {
        public TestBatModel()
            : base("test.bat")
        {
            IsVersioned = false;
            Inherit("test.animal", "animal");
            Inherit("test.flyable", "flyable");

            Fields.ManyToOne("animal", "test.animal").Required()
                .OnDelete(OnDeleteAction.Cascade).SetLabel("Base Animal Model");
            Fields.ManyToOne("flyable", "test.flyable").Required()
                .OnDelete(OnDeleteAction.Cascade).SetLabel("Base Flyable Model");
            Fields.Boolean("sucker").SetLabel("Is Sucker?");
        }
    }

    [Resource]
    public sealed class TestBatManModel : AbstractSqlModel
    {
        public TestBatManModel()
            : base("test.batman")
        {
            IsVersioned = false;
            Inherit("test.bat", "bat");

            Fields.ManyToOne("bat", "test.bat").Required()
                .OnDelete(OnDeleteAction.Cascade).SetLabel("Base Bat Model");
            Fields.Chars("real_name");
        }
    }


}
