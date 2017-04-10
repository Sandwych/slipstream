using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.DemoModule
{
    [Resource]
    public sealed class EmployeeEntity : AbstractSqlEntity
    {
        public EmployeeEntity() : base("demo.employee")
        {
            Fields.Chars("name").WithLabel("姓名").WithRequired();
            Fields.Chars("address").WithLabel("地址");
            Fields.Double("salary").WithLabel("月薪");
            Fields.Date("birthdate").WithLabel("出生日期");
        }
    }
}
