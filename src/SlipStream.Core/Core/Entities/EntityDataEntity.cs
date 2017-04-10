using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NHibernate.SqlCommand;

using SlipStream.Entity;
using SlipStream.Runtime;
using SlipStream.Data;

namespace SlipStream.Core
{
    /// <summary>
    /// </summary>
    [Resource]
    public sealed class EntityDataEntity : AbstractSqlEntity
    {
        public const string entityName = "core.entity_data";

        private readonly static string SqlToUpdate =
            @"update ""core_entity_data"" set ""ref_id""=? where ""entity""=? and ""name""=?";

        private readonly static string SqlToCreate =
            @"insert into ""core_entity_data""(""name"", ""module"", ""entity"", ""ref_id"") values(?,?,?,?)";

        private readonly static string SqlToLookupResourceId =
            @"select ""ref_id"" from ""core_entity_data"" where ""entity""=? and ""name""= ?";

        public EntityDataEntity()
            : base(entityName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").WithLabel("Key").WithRequired().WithSize(128);
            Fields.Chars("module").WithLabel("Module").WithRequired().WithSize(64);
            Fields.Chars("entity").WithLabel("Entity").WithRequired().WithSize(64);
            Fields.BigInteger("ref_id").WithLabel("Referenced ID").WithRequired();
            Fields.Text("value").WithLabel("Value");
        }

        internal static void Create(
            IDataContext dbctx, string module, string entity, string key, long resId)
        {
            var rows = dbctx.Execute(SqlToCreate, key, module, entity, resId);
            if (rows != 1)
            {
                throw new System.Data.DataException(
                    "Failed to insert row of table 'core_entity_data'");
            }
        }

        internal static long? TryLookupResourceId(IDataContext conn, string entity, string key)
        {
            if (conn == null)
            {
                throw new ArgumentNullException(nameof(conn));
            }

            if (string.IsNullOrEmpty(entity))
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var rows = conn.QueryAsArray<long>(SqlToLookupResourceId, entity, key);
            if (rows.Length == 0)
            {
                return null;
            }

            return (long)rows[0];
        }

        internal static void UpdateResourceId(IDataContext dbctx, string entity, string key, long refId)
        {
            if (dbctx == null)
            {
                throw new ArgumentNullException("conn");
            }

            var rowCount = dbctx.Execute(SqlToUpdate, refId, entity, key);

            if (rowCount != 1)
            {
                throw new InvalidDataException("More than one record");
            }
        }
    }
}
