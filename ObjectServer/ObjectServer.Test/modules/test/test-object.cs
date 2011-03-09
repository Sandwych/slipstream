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
    public sealed class ChildObject : TableModel
    {
        public ChildObject()
            : base("test.child")
        {
            Fields.Chars("name").SetLabel("Name").Required().SetSize(64);
            Fields.ManyToOne("master", "test.master").SetLabel("Master");
        }
    }


    //主表
    [Resource]
    public sealed class MasterObject : TableModel
    {
        public MasterObject()
            : base("test.master")
        {
            Fields.Chars("name").SetLabel("Name").SetSize(64);
            Fields.OneToMany("children", "test.child", "master").SetLabel("Children");
        }
    }


    [Resource]
    public class TestObject : TableModel
    {

        public TestObject()
            : base("test.test_object")
        {

            Fields.Chars("name").SetLabel("姓名").SetSize(64).Required();
            Fields.Chars("address").SetLabel("地址").SetSize(200).Required();
            Fields.Integer("field1").SetLabel("数1");
            Fields.Integer("field2").SetLabel("数2");
            Fields.Integer("field3").SetLabel("数3").SetGetter(this.GetField3);
            Fields.BigInteger("big_int_field").SetLabel("Bit Int Field");
            Fields.Boolean("boolean_field")
                .SetLabel("Boolean Field")
                .Required()
                .SetDefaultProc(s => true);
            Fields.Text("text_field").SetLabel("Text Field");
            Fields.Float("float_field").SetLabel("Float Field");
            Fields.Money("money_field").SetLabel("Money Field");
            Fields.Enumeration("enum_field",
                new Dictionary<string, string>() { { "state1", "State 1" }, { "state2", "State2" } })
                .SetLabel("Money Field");

        }

        [ServiceMethod]
        public static int GetSum(dynamic self, IContext ctx)
        {
            return 1 + 1;
        }

        public Dictionary<long, object> GetField3(IContext ctx, object[] ids)
        {
            var fieldNames = new object[] { "field1", "field2" };
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

}
