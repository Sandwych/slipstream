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
    public sealed class ModelDataModel : AbstractTableModel
    {
        public const string ModelName = "core.model_data";

        public ModelDataModel()
            : base(ModelName)
        {
            this.AutoMigration = false;

            Fields.Chars("name").SetLabel("Key").Required().SetSize(128);
            Fields.Chars("module").SetLabel("Module").Required().SetSize(64);
            Fields.Chars("model").SetLabel("Model").Required().SetSize(64);
            Fields.BigInteger("ref_id").SetLabel("Referenced ID").Required();
            Fields.Text("value").SetLabel("Value");
        }

        internal static void Create(
            IDBConnection conn, string module, string model, string key, long resId)
        {
            var sql =
                "INSERT INTO core_model_data(name, module, model, ref_id) VALUES(@0, @1, @2, @3)";
            var rows = conn.Execute(sql, key, module, model, resId);
            if (rows != 1)
            {
                throw new System.Data.DataException(
                    "Failed to insert row of table 'core_model_data'");
            }
        }

        internal static long? TryLookupResourceId(IDBConnection conn, string model, string key)
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

            var sql = new SqlString(
                "select ref_id FROM core_model_data ",
                "where model=", Parameter.Placeholder,
                " and name=", Parameter.Placeholder);
            var rows = conn.QueryAsArray<long>(sql, model, key);
            if (rows.Length == 0)
            {
                return null;
            }

            return (long)rows[0];
        }

        internal static void UpdateResourceId(IDBConnection conn, string model, string key, long refId)
        {
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }

            var sql = "UPDATE core_model_data SET ref_id = @0 WHERE model = @1 AND name = @2";
            var rowCount = conn.Execute(sql, refId, model, key);

            if (rowCount != 1)
            {
                throw new InvalidDataException("More than one record");
            }
        }
    }
}
