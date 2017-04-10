using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Test
{

    //////////////////// 继承单表测试的表 ///////////////////
    [Resource]
    public sealed class SingleTableBaseEntity : AbstractSqlEntity
    {
        public SingleTableBaseEntity()
            : base("test.single_table")
        {
            IsVersioned = false;
            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(64);
        }
    }


    [Resource]
    public sealed class SingleTableInheritedEntity : AbstractExtendedEntity
    {
        public SingleTableInheritedEntity()
            : base("test.single_table")
        {
            Fields.Integer("age").WithLabel("Age");
        }


        [ServiceMethod("Create")]
        public static long Create(IEntity entity, IDictionary<string, object> propertyBag)
        {
            var record = new Dictionary<string, object>(propertyBag);
            record["age"] = 33;
            return entity.CreateInternal(record);
        }
    }


    ////////////////////// 测试多表继承的表 /////////////////

    [Resource]
    public sealed class TestAnimalEntity : AbstractSqlEntity
    {
        public TestAnimalEntity()
            : base("test.animal")
        {
            IsVersioned = false;
            Fields.Chars("name").WithLabel("Name");
        }
    }

    /// <summary>
    /// dog 继承自 animal
    /// </summary>
    [Resource]
    public sealed class TestDogEntity : AbstractSqlEntity
    {
        public TestDogEntity()
            : base("test.dog")
        {
            IsVersioned = false;
            Inherit("test.animal", "animal");

            Fields.ManyToOne("animal", "test.dog").WithRequired().OnDelete(OnDeleteAction.Cascade)
                .WithLabel("Base Animal Entity");
            Fields.Chars("dogfood").WithLabel("Favorite Dogfood");
        }
    }

    /// <summary>
    /// cocker dog 继承自 dog
    /// </summary>
    [Resource]
    public sealed class TestCockerEntity : AbstractSqlEntity
    {
        public TestCockerEntity()
            : base("test.cocker")
        {
            IsVersioned = false;
            Inherit("test.dog", "dog");

            Fields.ManyToOne("dog", "test.dog").WithRequired().OnDelete(OnDeleteAction.Cascade)
                .WithLabel("Base Animal Entity");
            Fields.Chars("color").WithLabel("Color");
        }
    }

    /************* 演示多继承 ***************/

    [Resource]
    public sealed class TestFlyableEntity : AbstractSqlEntity
    {
        public TestFlyableEntity()
            : base("test.flyable")
        {
            IsVersioned = false;
            Fields.Integer("wings").WithLabel("Wings");
        }
    }

    [Resource]
    public sealed class TestBatEntity : AbstractSqlEntity
    {
        public TestBatEntity()
            : base("test.bat")
        {
            IsVersioned = false;
            Inherit("test.animal", "animal");
            Inherit("test.flyable", "flyable");

            Fields.ManyToOne("animal", "test.animal").WithRequired()
                .OnDelete(OnDeleteAction.Cascade).WithLabel("Base Animal Entity");
            Fields.ManyToOne("flyable", "test.flyable").WithRequired()
                .OnDelete(OnDeleteAction.Cascade).WithLabel("Base Flyable Entity");
            Fields.Boolean("sucker").WithLabel("Is Sucker?");
        }
    }

    [Resource]
    public sealed class TestBatManEntity : AbstractSqlEntity
    {
        public TestBatManEntity()
            : base("test.batman")
        {
            IsVersioned = false;
            Inherit("test.bat", "bat");

            Fields.ManyToOne("bat", "test.bat").WithRequired()
                .OnDelete(OnDeleteAction.Cascade).WithLabel("Base Bat Entity");
            Fields.Chars("real_name");
        }
    }


}
