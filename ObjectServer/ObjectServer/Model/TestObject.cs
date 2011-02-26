using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;


namespace ObjectServer.Model
{
    [ServiceObject]
    public class TestObject : TableModel
    {

        public TestObject()
            : base("test.test_object")
        {

            Fields.Chars("name").SetLabel("姓名").SetSize(64).SetRequired();
            Fields.Chars("address").SetLabel("地址").SetSize(200).SetRequired();
            Fields.Integer("field1").SetLabel("数1");
            Fields.Integer("field2").SetLabel("数2");
            Fields.Integer("field3").SetLabel("数3").SetGetter(this.GetField3);
            Fields.BigInteger("big_int_field").SetLabel("Bit Int Field");
            Fields.Boolean("boolean_field")
                .SetLabel("Boolean Field")
                .SetRequired()
                .SetDefaultProc(s => true);
            Fields.Text("text_field").SetLabel("Text Field");
            //Fields.Float("float_field").SetLabel("Float Field");
            Fields.Money("money_field").SetLabel("Money Field");
        }

        [ServiceMethod]
        public virtual int GetSum(IContext callingContext)
        {
            return 1 + 1;
        }

        public Dictionary<long, object> GetField3(IContext callingContext, object[] ids)
        {
            var fieldNames = new object[] { "field1", "field2" };
            var values = base.Read(callingContext, ids, fieldNames);
            var rows = new Dictionary<long, object>(ids.Count());
            foreach (var r in values)
            {
                var id = (long)r["id"];
                var field1 = (int)r["field1"];
                var field2 = (int)r["field2"];
                rows[id] = field1 + field2;
            }
            return rows;
        }
    }
}
