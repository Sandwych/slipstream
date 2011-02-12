using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;


namespace ObjectServer.Model
{
    [ServiceObject]
    public class TestObject : ModelBase
    {

        public TestObject()
        {
            this.Name = "test.test_object";

            this.DefineField("name", "姓名", "varchar", 64);
            this.DefineField("address", "地址", "varchar", 200);
        }

        [ServiceMethod]
        public int GetSum(IDbConnection conn)        
        {
            return 1 + 1;
        }


    }
}
