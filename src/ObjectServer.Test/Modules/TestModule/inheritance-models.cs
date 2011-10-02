using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Test
{

    //////////////////// 继承单表测试的表 ///////////////////
    [Resource]
    public sealed class SingleTableBaseModel : AbstractTableModel
    {
        public SingleTableBaseModel()
            : base("test.single_table")
        {
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
        public static long Create(IModel model, ITransactionContext ctx, IDictionary<string, object> propertyBag)
        {
            var record = new Dictionary<string, object>(propertyBag);
            record["age"] = 33;
            return model.CreateInternal(ctx, record);
        }
    }


    ////////////////////// 测试多表继承的表 /////////////////

    [Resource]
    public sealed class TestAnimalModel : AbstractTableModel
    {
        public TestAnimalModel()
            : base("test.animal")
        {
            Fields.Chars("name").SetLabel("Name");
        }
    }

    [Resource]
    public sealed class TestDogModel : AbstractTableModel
    {
        public TestDogModel()
            : base("test.dog")
        {
            Inherit("test.animal", "animal");

            Fields.ManyToOne("animal", "test.dog").Required().OnDelete(OnDeleteAction.Cascade)
                .SetLabel("Base Animal Model");
            Fields.Chars("dogfood").SetLabel("Favorite Dogfood");
        }
    }


}
