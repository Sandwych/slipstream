using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.DemoModule
{
    [Resource]
    public sealed class EmployeeModel : AbstractTableModel
    {
        public EmployeeModel()
            : base("demo.employee")
        {
            Fields.Chars("name").SetLabel("姓名").Required();
            Fields.Chars("address").SetLabel("地址");
            Fields.Float("salary").BeProperty().SetLabel("月薪");
        }
    }
}
