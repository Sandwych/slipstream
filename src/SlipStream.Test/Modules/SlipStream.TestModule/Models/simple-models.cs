using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Test
{

    //我们故意反转依赖顺序进行声明，看系统能否处理

    //子表
    [Resource]
    public sealed class ChildModel : AbstractSqlModel
    {
        public ChildModel()
            : base("test.child")
        {
            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(64);
            Fields.ManyToOne("master", "test.master").WithLabel("Master")
                .WithNotRequired().OnDelete(OnDeleteAction.SetNull);
        }
    }


    //主表
    [Resource]
    public sealed class MasterModel : AbstractSqlModel
    {
        public MasterModel()
            : base("test.master")
        {
            Fields.Chars("name").WithLabel("Name").WithSize(64);
            Fields.OneToMany("children", "test.child", "master").WithLabel("Children");
        }
    }


    [Resource]
    public class TestModel : AbstractSqlModel
    {

        public TestModel()
            : base("test.test_model")
        {
            IsVersioned = false;

            Fields.Chars("name").WithLabel("姓名").WithSize(64).WithRequired();
            Fields.Chars("address").WithLabel("地址").WithSize(200).WithRequired();
            Fields.Integer("field1").WithLabel("数1");
            Fields.Integer("field2").WithLabel("数2");
            Fields.Integer("field3").WithLabel("数3").SetValueGetter(this.GetField3);
            Fields.BigInteger("big_int_field").WithLabel("Bit Int Field");
            Fields.Boolean("boolean_field").WithLabel("Boolean Field").WithRequired().WithDefaultValueGetter(s => true);
            Fields.Text("text_field").WithLabel("Text Field");
            Fields.Xml("xml_field").WithLabel("XML Field");
            Fields.Double("double_field").WithLabel("Double Field");
            Fields.Decimal("money_field").WithLabel("Decimal Field");
            Fields.Date("date_field").WithLabel("Date Field");
            Fields.Time("time_field").WithLabel("Time Field");
            Fields.DateTime("datetime_field").WithLabel("DateTime Field");

            Fields.Enumeration("enum_field",
                new Dictionary<string, string>() { { "state1", "State 1" }, { "state2", "State2" } })
                .WithLabel("Enumeration Field");

            Fields.Binary("binary_field").WithLabel("Binary Field");

            Fields.Reference("reference_field").WithLabel("Reference Field").SetOptions(
                new Dictionary<string, string>()
                {
                    { "test.master", "Master Model" },
                    { "test.child", "Child Model" },
                });
        }

        [ServiceMethod("GetNumberPlusResult")]
        public static int GetNumberPlusResult(IModel self, int x, int y)
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
            Fields.Chars("name").WithLabel("Name");
            Fields.Double("quantity").BeProperty().WithLabel("Quantity");
        }
    }

    [Resource]
    public sealed class SalesOrderModel : AbstractSqlModel
    {
        public SalesOrderModel()
            : base("test.sales_order")
        {
            Fields.Chars("name").WithLabel("Code");
            Fields.DateTime("order_date").WithLabel("Date");
            Fields.ManyToOne("organization", "core.organization").WithLabel("Organization").WithNotRequired();
        }
    }


    [Resource]
    public sealed class ValidatorModel : AbstractSqlModel
    {
        public ValidatorModel()
            : base("test.validator")
        {
            Fields.Chars("required_field").WithLabel("Required Field").WithRequired();
            Fields.Chars("readonly_field").WithLabel("Readonly Field").Readonly();
        }
    }

    [Resource]
    public sealed class PersonModel : AbstractSqlModel
    {
        public PersonModel()
            : base("test.person")
        {
            Fields.Chars("name").WithRequired();
            Fields.Enumeration("gender", new Dictionary<string, string>() { 
                { "male", "Male" }, { "female", "Female" }, { "unknown", "Unknown" } });
        }
    }

}
