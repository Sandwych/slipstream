using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NHibernate.SqlCommand;

using ObjectServer.Model;
using ObjectServer.Runtime;
using ObjectServer.Data;

namespace ObjectServer.Core
{
    /// <summary>
    /// </summary>
    [Resource]
    public sealed class ModelDataModel : AbstractSqlModel
    {
        public const string ModelName = "core.model_data";

        private readonly static string SqlToUpdate =
            @"update ""core_model_data"" set ""ref_id""=? where ""model""=? and ""name""=?";

        private readonly static string SqlToCreate =
            @"insert into ""core_model_data""(""name"", ""module"", ""model"", ""ref_id"") values(?,?,?,?)";

        private readonly static string SqlToLookupResourceId =
            @"select ""ref_id"" from ""core_model_data"" where ""model""=? and ""name""= ?";

        public ModelDataModel()
            : base(ModelName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").SetLabel("Key").Required().SetSize(128);
            Fields.Chars("module").SetLabel("Module").Required().SetSize(64);
            Fields.Chars("model").SetLabel("Model").Required().SetSize(64);
            Fields.BigInteger("ref_id").SetLabel("Referenced ID").Required();
            Fields.Text("value").SetLabel("Value");
        }

        internal static void Create(
            IDataContext dbctx, string module, string model, string key, long resId)
        {
            var rows = dbctx.Execute(SqlToCreate, key, module, model, resId);
            if (rows != 1)
            {
                throw new System.Data.DataException(
                    "Failed to insert row of table 'core_model_data'");
            }
        }

        internal static long? TryLookupResourceId(IDataContext conn, string model, string key)
        {
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }

            if (string.IsNullOrEmpty(model))
            {
                throw new ArgumentNullException("model");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("model");
            }

            var rows = conn.QueryAsArray<long>(SqlToLookupResourceId, model, key);
            if (rows.Length == 0)
            {
                return null;
            }

            return (long)rows[0];
        }

        internal static void UpdateResourceId(IDataContext dbctx, string model, string key, long refId)
        {
            if (dbctx == null)
            {
                throw new ArgumentNullException("conn");
            }

            var rowCount = dbctx.Execute(SqlToUpdate, refId, model, key);

            if (rowCount != 1)
            {
                throw new InvalidDataException("More than one record");
            }
        }
    }
}
