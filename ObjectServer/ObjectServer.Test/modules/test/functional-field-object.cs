using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Test
{

    [Resource]
    public sealed class FunctionalFieldObject : TableModel
    {
        public const string ModelName = "test.functional_field_object";

        public FunctionalFieldObject()
            : base(ModelName)
        {
            Fields.Chars("name").SetLabel("Name").SetRequired().SetSize(64);
            Fields.ManyToOne("user", "core.user").SetGetter(GetUser);
        }

        private Dictionary<long, object> GetUser(IContext ctx, object[] ids)
        {
            var userModel = ctx.Database.Resources["core.user"];
            var domain = new object[][] { new object[] { "login", "=", "root" } };
            var userIds = userModel.Search(ctx, domain, 0, 0);
            var rootId = userIds[0];
            var result = new Dictionary<long, object>(ids.Length);
            foreach (var id in ids)
            {
                result[(long)id] = new object[2] { rootId, "root" };
            }

            return result;
        }
    }



}
