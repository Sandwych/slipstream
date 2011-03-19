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
            Fields.ManyToOne("user", "core.user").ValueGetter(GetUser);
        }

        private Dictionary<long, object> GetUser(IResourceScope ctx, IEnumerable<long> ids)
        {
            var userModel = ctx.GetResource("core.user");
            var domain = new object[][] { new object[] { "login", "=", "root" } };
            var userIds = Search(userModel, ctx, domain, 0, 0);
            var rootId = userIds[0];
            var result = new Dictionary<long, object>();
            foreach (var id in ids)
            {
                result[(long)id] = new object[2] { rootId, "root" };
            }

            return result;
        }
    }



}
