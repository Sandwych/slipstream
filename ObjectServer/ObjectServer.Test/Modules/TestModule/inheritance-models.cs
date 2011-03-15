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

        [ServiceMethod]
        public static long Create(
            dynamic model, IResourceScope ctx, IDictionary<string, object> propertyBag)
        {
            var record = new Dictionary<string, object>(propertyBag);
            record["age"] = 33;
            return model.CreateInternal(ctx, record);
        }
    }


    ////////////////////// 测试多表继承的表 /////////////////

    [Resource]
    public sealed class TestUserModel : AbstractTableModel
    {
        public TestUserModel()
            : base("test.user")
        {
            Fields.Chars("name").SetLabel("Name");
        }
    }

    [Resource]
    public sealed class TestAdminUserModel : AbstractTableModel
    {
        public TestAdminUserModel()
            : base("test.admin_user")
        {
            Inherit("test.user", "user");

            Fields.ManyToOne("user", "test.user").SetLabel("Base User Model");
            Fields.Chars("admin_info").SetLabel("Administration Info");
        }
    }


}
