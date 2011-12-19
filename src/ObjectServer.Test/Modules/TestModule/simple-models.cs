using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Malt;
using ObjectServer.Model;

namespace ObjectServer.Test
{

    //我们故意反转依赖顺序进行声明，看系统能否处理

    //子表
    [Resource]
    public sealed class ChildModel : AbstractSqlModel
    {
        public ChildModel()
            : base("test.child")
        {
            Fields.Chars("name").SetLabel("Name").Required().SetSize(64);
            Fields.ManyToOne("master", "test.master").SetLabel("Master")
                .NotRequired().OnDelete(OnDeleteAction.SetNull);
        }
    }


    //主表
    [Resource]
    public sealed class MasterModel : AbstractSqlModel
    {
        public MasterModel()
            : base("test.master")
        {
            Fields.Chars("name").SetLabel("Name").SetSize(64);
            Fields.OneToMany("children", "test.child", "master").SetLabel("Children");
        }
    }


    [Resource]
    public class TestModel : AbstractSqlModel
    {

        public TestModel()
            : base("test.test_model")
        {
            IsVersioned = false;

            Fields.Chars("name").SetLabel("姓名").SetSize(64).Required();
            Fields.Chars("address").SetLabel("地址").SetSize(200).Required();
            Fields.Integer("field1").SetLabel("数1");
            Fields.Integer("field2").SetLabel("数2");
            Fields.Integer("field3").SetLabel("数3").SetValueGetter(this.GetField3);
            Fields.BigInteger("big_int_field").SetLabel("Bit Int Field");
            Fields.Boolean("boolean_field").SetLabel("Boolean Field").Required().SetDefaultValueGetter(s => true);
            Fields.Text("text_field").SetLabel("Text Field");
            Fields.Double("double_field").SetLabel("Double Field");
            Fields.Decimal("money_field").SetLabel("Decimal Field");

            Fields.Enumeration("enum_field",
                new Dictionary<string, string>() { { "state1", "State 1" }, { "state2", "State2" } })
                .SetLabel("Enumeration Field");

            Fields.Binary("binary_field").SetLabel("Binary Field");

            Fields.Reference("reference_field").SetLabel("Reference Field").SetOptions(
                new Dictionary<string, string>()
                {
                    { "test.master", "Master Model" },
                    { "test.child", "Child Model" },
                });
        }

        [ServiceMethod("GetNumberPlusResult")]
        public static int GetNumberPlusResult(IModel self, IServiceContext scope, int x, int y)
        {
            return x + y;
        }

        public Dictionary<long, object> GetField3(IServiceContext ctx, long[] ids)
        {
            var fieldNames = new string[] { "field1", "field2" };
            var values = base.ReadInternal(ids, fieldNames);
            var rows = new Dictionary<long, object>(ids.Count());
            foreach (var r in values)
            {
                var id = (long)r["_id"];
                var field1 = r["field1"];
                var field2 = r["field2"];

                if (field1.IsNull() || field2.IsNull())
                {
                    rows[id] = null;
                }
                else
                {
                    rows[id] = (int)field1 + (int)field2;
                }
            }
            return rows;
        }
    }



    [Resource]
    public sealed class ProductModel : AbstractSqlModel
    {
        public ProductModel()
            : base("test.product")
        {
            Fields.Chars("name").SetLabel("Name");
            Fields.Double("quantity").BeProperty().SetLabel("Quantity");
        }
    }

    [Resource]
    public sealed class SalesOrderModel : AbstractSqlModel
    {
        public SalesOrderModel()
            : base("test.sales_order")
        {
            Fields.Chars("name").SetLabel("Code");
            Fields.DateTime("order_date").SetLabel("Date");
            Fields.ManyToOne("organization", "core.organization").SetLabel("Organization").NotRequired();
        }
    }


    [Resource]
    public sealed class ValidatorModel : AbstractSqlModel
    {
        public ValidatorModel()
            : base("test.validator")
        {
            Fields.Chars("required_field").SetLabel("Required Field").Required();
            Fields.Chars("readonly_field").SetLabel("Readonly Field").Readonly();
        }
    }

    [Resource]
    public sealed class PersonModel : AbstractSqlModel
    {
        public PersonModel()
            : base("test.person")
        {
            Fields.Chars("name").Required();
            Fields.Enumeration("gender", new Dictionary<string, string>() { 
                { "male", "Male" }, { "female", "Female" }, { "unknown", "Unknown" } });
        }
    }

}
