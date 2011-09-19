using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Test
{

    [Resource]
    public sealed class FunctionalFieldObject : AbstractTableModel
    {
        public const string ModelName = "test.functional_field_object";

        public FunctionalFieldObject()
            : base(ModelName)
        {
            Fields.Chars("name").SetLabel("Name").Required().SetSize(64);
            Fields.ManyToOne("user", "core.user").SetValueGetter(GetUser);
            Fields.Integer("field1").Required();
            Fields.Integer("field2").Required();
            Fields.Integer("sum_field").SetValueGetter(GetSum).Readonly();
        }

        private Dictionary<long, object> GetSum(IServiceContext ctx, IEnumerable<long> ids)
        {
            var fields = new string[] { "field1", "field2" };
            var records = this.ReadInternal(ctx, ids.ToArray(), fields);
            var result = new Dictionary<long, object>(ids.Count());
            foreach (var record in records)
            {
                var id = (long)record[AbstractModel.IDFieldName];
                var field1 = (int)record["field1"];
                var field2 = (int)record["field2"];
                result[id] = field1 + field2;
            }

            return result;
        }

        private Dictionary<long, object> GetUser(IServiceContext ctx, IEnumerable<long> ids)
        {
            var userModel = (IModel)ctx.GetResource("core.user");
            var constraints = new object[][] { new object[] { "login", "=", "root" } };
            var userIds = Search(userModel, ctx, constraints, null, 0, 0);
            var rootId = userIds[0];
            var result = new Dictionary<long, object>();
            foreach (var id in ids)
            {
                result[id] = new object[2] { rootId, "root" };
            }

            return result;
        }
    }



}
