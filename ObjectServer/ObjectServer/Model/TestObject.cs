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
        {
            this.Name = "test.test_object";

            this.CharsField("name", "姓名", 64, true, null, null);
            this.CharsField("address", "地址", 200, true, null, null);
            this.IntegerField("field1", "数1", true, null, null);
            this.IntegerField("field2", "数2", true, null, null);
            this.IntegerField("field3", "数3", true, this.GetField3, null);
            this.BitIntegerField("big_int_field", "Bit Int Field", false, null, null);
            this.BooleanField("boolean_field", "Boolean Field", true, null, s => true);
            this.TextField("text_field", "Text Field", false, null, null);
        }

        [ServiceMethod]
        public int GetSum(ICallingContext callingContext)
        {
            return 1 + 1;
        }

        public Dictionary<long, object> GetField3(ICallingContext callingContext, object[] ids)
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
