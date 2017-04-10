using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Test
{

    [Resource]
    public sealed class EmployeeModel : AbstractSqlModel
    {
        public EmployeeModel()
            : base("test.employee")
        {
            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(64);
            Fields.Integer("age").WithLabel("Age").WithNotRequired();
            Fields.ManyToMany("departments", "test.department_employee", "eid", "did")
                .WithLabel("Departments");
        }
    }

    [Resource]
    public sealed class DepartmentModel : AbstractSqlModel
    {
        public DepartmentModel()
            : base("test.department")
        {
            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(64);
            Fields.ManyToMany("employees", "test.department_employee", "did", "eid")
                .WithLabel("Employees");
        }
    }

    [Resource]
    public sealed class DepartmentEmployeeModel : AbstractSqlModel
    {

        public DepartmentEmployeeModel()
            : base("test.department_employee")
        {
            this.TableName = "test_department_employee_rel";

            Fields.ManyToOne("did", "test.department").WithLabel("Department").WithRequired();
            Fields.ManyToOne("eid", "test.employee").WithLabel("Employee").WithRequired();
        }
    }


}
