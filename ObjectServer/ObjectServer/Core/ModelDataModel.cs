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
        private static readonly string[] s_RefIdFields = new string[] { "ref_id" };

        public ModelDataModel()
            : base(ModelName)
        {
            //删掉基类自动添加的用户列
            Fields.Remove(CreatedUserField);
            Fields.Remove(ModifiedUserField);

            Fields.Chars("name").SetLabel("Key").Required().SetSize(128);
            Fields.Chars("module").SetLabel("Module").Required().SetSize(64);
            Fields.Chars("model").SetLabel("Model").Required().SetSize(64);
            Fields.BigInteger("ref_id").SetLabel("Referenced ID").Required();
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

            return this.CreateInternal(ctx, record);
        }

        public long? TryLookupResourceId(IContext ctx, string model, string key)
        {
            var fields = s_RefIdFields;
            var domain = new object[][] 
            { 
                new object[] { "model", "=", model },
                new object[] { "name", "=", key },
            };

            var ids = this.SearchInternal(ctx, domain, 0, 0);
            if (ids == null || ids.Length == 0)
            {
                return null;
            }

            var records = this.ReadInternal(ctx, ids, fields);
            var refId = (long)records[0]["ref_id"];

            return refId;
        }

        public void UpdateResourceId(IContext ctx, string model, string key, long id)
        {
            var domain = new object[][]
            {
                new object[] { "model", "=", model },
                new object[] { "name", "=", key },
            };
            var ids = this.SearchInternal(ctx, domain);

            if (ids.Length != 0)
            {
                throw new InvalidDataException("More than one record");
            }

            var selfId = (long)ids[0];
            var record = new Dictionary<string, object>()
            {
                { "ref_id", "id" },
            };
            this.WriteInternal(ctx, selfId, record);
        }
    }
}
