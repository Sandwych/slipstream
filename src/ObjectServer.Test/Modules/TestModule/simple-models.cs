using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Test
{

    //我们故意反转依赖顺序进行声明，看系统能否处理

    //子表
    [Resource]
    public sealed class ChildModel : AbstractTableModel
    {
        public ChildModel()
            : base("test.child")
        {
            Fields.Chars("name").SetLabel("Name").Required().SetSize(64);
            Fields.ManyToOne("master", "test.master").SetLabel("Master");
        }
    }


    //主表
    [Resource]
    public sealed class MasterModel : AbstractTableModel
    {
        public MasterModel()
            : base("test.master")
        {
            Fields.Chars("name").SetLabel("Name").SetSize(64);
            Fields.OneToMany("children", "test.child", "master").SetLabel("Children");
        }
    }


    [Resource]
    public class TestModel : AbstractTableModel
    {

        public TestModel()
            : base("test.test_model")
        {

            Fields.Chars("name").SetLabel("姓名").SetSize(64).Required();
            Fields.Chars("address").SetLabel("地址").SetSize(200).Required();
            Fields.Integer("field1").SetLabel("数1");
            Fields.Integer("field2").SetLabel("数2");
            Fields.Integer("field3").SetLabel("数3").ValueGetter(this.GetField3);
            Fields.BigInteger("big_int_field").SetLabel("Bit Int Field");
            Fields.Boolean("boolean_field").SetLabel("Boolean Field").Required().DefaultValueGetter(s => true);
            Fields.Text("text_field").SetLabel("Text Field");
            Fields.Float("float_field").SetLabel("Float Field");
            Fields.Money("money_field").SetLabel("Money Field");

            Fields.Enumeration("enum_field",
                new Dictionary<string, string>() { { "state1", "State 1" }, { "state2", "State2" } })
                .SetLabel("Money Field");

            Fields.Binary("binary_field").SetLabel("Binary Field");

            Fields.Reference("reference_field").SetLabel("Reference Field").SetOptions(
                new Dictionary<string, string>()
                {
                    { "test.master", "Master Model" },
                    { "test.child", "Child Model" },
                });
        }

        [ServiceMethod]
        public static int GetNumberPlusResult(IModel self, IServiceScope scope, int x, int y)
        {
            return x + y;
        }

        public Dictionary<long, object> GetField3(IServiceScope ctx, long[] ids)
        {
            var fieldNames = new string[] { "field1", "field2" };
            var values = base.ReadInternal(ctx, ids, fieldNames);
            var rows = new Dictionary<long, object>(ids.Count());
            foreach (var r in values)
            {
                var id = (long)r["id"];
                var field1 = r["field1"];
                var field2 = r["field2"];

                if (field1 is DBNull || field2 is DBNull)
                {
                    rows[id] = DBNull.Value;
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
    public sealed class ProductModel : AbstractTableModel
    {
        public ProductModel()
            : base("test.product")
        {
            Fields.Chars("name").SetLabel("Name");
            Fields.Float("quantity").BeProperty().SetLabel("Quantity");
        }
    }

}
