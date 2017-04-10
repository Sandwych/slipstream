using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Test
{

    [Resource]
    public sealed class EmployeeEntity : AbstractSqlEntity
    {
        public EmployeeEntity()
            : base("test.employee")
        {
            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(64);
            Fields.Integer("age").WithLabel("Age").WithNotRequired();
            Fields.ManyToMany("departments", "test.department_employee", "eid", "did")
                .WithLabel("Departments");
        }
    }

    [Resource]
    public sealed class DepartmentEntity : AbstractSqlEntity
    {
        public DepartmentEntity()
            : base("test.department")
        {
            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(64);
            Fields.ManyToMany("employees", "test.department_employee", "did", "eid")
                .WithLabel("Employees");
        }
    }

    [Resource]
    public sealed class DepartmentEmployeeEntity : AbstractSqlEntity
    {

        public DepartmentEmployeeEntity()
            : base("test.department_employee")
        {
            this.TableName = "test_department_employee_rel";

            Fields.ManyToOne("did", "test.department").WithLabel("Department").WithRequired();
            Fields.ManyToOne("eid", "test.employee").WithLabel("Employee").WithRequired();
        }
    }


}
