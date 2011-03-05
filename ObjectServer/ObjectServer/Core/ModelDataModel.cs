using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using Newtonsoft.Json;

using ObjectServer.Model;
using ObjectServer.Runtime;
using ObjectServer.Backend;

namespace ObjectServer.Core
{
    /// <summary>
    /// </summary>
    [Resource]
    public sealed class ModelDataModel : TableModel
    {
        public const string ModelName = "core.model_data";
        private static readonly object[] s_RefIdFields = new object[] { "ref_id" };

        public ModelDataModel()
            : base(ModelName)
        {
            //删掉基类自动添加的用户列
            Fields.Remove(CreatedUserField);
            Fields.Remove(ModifiedUserField);

            Fields.Chars("name").SetLabel("Key").SetRequired().SetSize(128);
            Fields.Chars("module").SetLabel("Module").SetRequired().SetSize(64);
            Fields.Chars("model").SetLabel("Model").SetRequired().SetSize(64);
            Fields.BigInteger("ref_id").SetLabel("Referenced ID").SetRequired();
            Fields.Text("value").SetLabel("Value");
        }

        public long Create(IContext ctx, string module, string model, string key, long resId)
        {
            var record = new Dictionary<string, object>()
                {
                    { "name", key },
                    { "module", module },
                    { "model", model },
                    { "ref_id", resId },
                };

            return this.Create(ctx, record);
        }

        public long? TryLookupResourceId(IContext ctx, string model, string key)
        {
            var fields = s_RefIdFields;
            var domain = new object[][] 
            { 
                new object[] { "model", "=", model },
                new object[] { "key", "=", key },
            };

            var ids = this.Search(ctx, domain, 0, 0);
            var records = this.Read(ctx, ids, fields);
            if (records.Length != 1)
            {
                return null;
            }
            else
            {
                return (long)records[0]["ref_id"];
            }
        }

        public void UpdateResourceId(IContext ctx, string model, string key, long id)
        {
            var domain = new object[][]
            {
                new object[] { "model", "=", model },
                new object[] { "key", "=", key },
            };
            var ids = this.Search(ctx, domain);

            if (ids.Length != 0)
            {
                throw new InvalidDataException("More than one record");
            }

            var selfId = (long)ids[0];
            var record = new Dictionary<string, object>()
            {
                { "ref_id", "id" },
            };
            this.Write(ctx, selfId, record);
        }
    }
}
