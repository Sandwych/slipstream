using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;



namespace ObjectServer
{
    [ModelObject]
    public class TestObject : ModelBase
    {

        public TestObject()
            : base()
        {
            this.Name = "Test.TestObject";
            this.TableName = "test_test_object";
        }

        [ServiceMethod]
        public int GetSum(IDbConnection conn)        
        {
            return 1 + 1;
        }


    }
}
