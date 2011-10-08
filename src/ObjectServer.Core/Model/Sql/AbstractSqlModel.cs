using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;

using NHibernate.SqlCommand;

using ObjectServer.Exceptions;
using ObjectServer.Data;
using ObjectServer.Utility;

namespace ObjectServer.Model
{
    using IRecord = IDictionary<string, object>;
    using Record = Dictionary<string, object>;

    /// <summary>
    /// 基于关系数据库表的实体类基类
    /// </summary>
    public abstract partial class AbstractSqlModel : AbstractModel
    {
        public const string LeftFieldName = "_left";
        public const string RightFieldName = "_right";
        public const string ParentFieldName = "parent";
        public const string ChildrenFieldName = "_children";
        public const string DescendantsFieldName = "_descendants";

        /// <summary>
        /// 这些字段是由 ORM 子系统处理的，客户端不能操作
        /// </summary>
        public static readonly string[] SystemReadonlyFields = new string[]
        {
            IdFieldName,
            CreatedTimeFieldName,
            CreatedUserFieldName,
            UpdatedTimeFieldName,
            UpdatedUserFieldName,
            VersionFieldName,
            LeftFieldName,
            RightFieldName,
            ChildrenFieldName,
            DescendantsFieldName,
        };

        private string tableName = null;
        private string quotedTableName = null;

        public override string TableName
        {
            get
            {
                return this.tableName;
            }
            protected set
            {
                if (string.IsNullOrEmpty(value))
                {
                    LoggerProvider.EnvironmentLogger.Error(() => "Table name cannot be empty");
                    throw new ArgumentNullException("value");
                }

                this.tableName = value;
                this.quotedTableName = DataProvider.Dialect.QuoteForTableName(value);
                this.SequenceName = value + "_" + IdFieldName + "_seq";
            }
        }

        public string SequenceName { get; protected set; }

        protected AbstractSqlModel(string name)
            : base(name)
        {

            this.CanCreate = true;
            this.CanRead = true;
            this.CanWrite = true;
            this.CanDelete = true;
            this.Hierarchy = false;
            this.LogCreation = false;
            this.LogWriting = false;
            this.SetName(name);
        }


        private void SetName(string name)
        {
            this.TableName = name.ToLowerInvariant().Replace('.', '_');
        }

        /// <summary>
        /// 初始化数据库信息
        /// </summary>
        public override void Initialize(ITransactionContext tc, bool update)
        {
            this.AddInternalFields();

            base.Initialize(tc, update);

            if (this.NameGetter == null)
            {
                this.NameGetter = this.DefaultNameGetter;
            }

            if (!this.Fields.ContainsKey("name"))
            {
                LoggerProvider.EnvironmentLogger.Warn(() => string.Format(
                    "I strongly suggest you to add the 'name' field into Model '{0}'",
                    this.Name));
            }

            if (update && this.AutoMigration)
            {
                var migrator = new TableMigrator(tc, this);
                migrator.Migrate();
            }
        }

        private void AddInternalFields()
        {
            Debug.Assert(this.Fields.ContainsKey(IdFieldName));

            //只有非继承的模型才添加内置字段
            if (this.AutoMigration)
            {
                Fields.DateTime(CreatedTimeFieldName).SetLabel("Created")
                    .NotRequired().SetDefaultValueGetter(ctx => DateTime.Now).Readonly();

                Fields.DateTime(UpdatedTimeFieldName).SetLabel("Last Modified")
                    .NotRequired().SetDefaultValueGetter(ctx => DateTime.Now);

                Fields.ManyToOne(CreatedUserFieldName, "core.user").SetLabel("Creator")
                    .NotRequired().Readonly()
                    .SetDefaultValueGetter(ctx => ctx.Session.UserId > 0 ? (object)ctx.Session.UserId : null);

                Fields.ManyToOne(UpdatedUserFieldName, "core.user").SetLabel("Modifier")
                    .NotRequired()
                    .SetDefaultValueGetter(ctx => ctx.Session.UserId > 0 ? (object)ctx.Session.UserId : null);

                if (this.Hierarchy)
                {
                    this.AddHierarchyInternalFields();
                }
            }
        }

        private void AddHierarchyInternalFields()
        {
            Debug.Assert(this.Hierarchy);
            Debug.Assert(!this.Fields.ContainsKey(LeftFieldName));
            Debug.Assert(!this.Fields.ContainsKey(RightFieldName));

            Fields.BigInteger(LeftFieldName).SetLabel("Left Value")
                .Required().SetDefaultValueGetter(ctx => -1);

            Fields.BigInteger(RightFieldName).SetLabel("Right Value")
                .Required().SetDefaultValueGetter(ctx => -1);

            //这里通过 SQL 查询返回
            if (!Fields.ContainsKey(ParentFieldName))
            {
                Fields.ManyToOne(ParentFieldName, this.Name)
                    .SetLabel("Parent");
            }

            Fields.OneToMany(ChildrenFieldName, this.Name, ParentFieldName)
                .SetLabel("Children")
                .SetValueGetter((scope, ids) =>
                {
                    var result = new Dictionary<long, object>(ids.Length);
                    foreach (var id in ids)
                    {
                        var children = this.GetChildrenIDs(scope.DBContext, id);
                        result.Add(id, children);
                    }
                    return result;

                });

            Fields.OneToMany(DescendantsFieldName, this.Name, ParentFieldName)
                .SetLabel("Descendants")
                .SetValueGetter((scope, ids) =>
                {
                    var result = new Dictionary<long, object>(ids.Length);
                    foreach (var id in ids)
                    {
                        var children = this.GetDescendantIDs(scope.DBContext, id);
                        result.Add(id, children);
                    }
                    return result;

                });
        }

        /// <summary>
        /// 层次表获取某指定记录的直系子记录的 IDs
        /// </summary>
        /// <param name="dbctx"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private long[] GetChildrenIDs(IDbContext dbctx, long parentID)
        {
            var sqlFmt =
@"
SELECT  hc.*
FROM    ""{0}"" hp
JOIN    ""{0}"" hc
ON      hc._left BETWEEN hp._left AND hp._right
WHERE   hp._id = ? AND hc._id <> ?
        AND
        (
        SELECT  COUNT(hn._id)
        FROM    ""{0}"" hn
        WHERE   hc._left BETWEEN hn._left AND hn._right
                AND hn._left BETWEEN hp._left AND hp._right
        ) <= 2
";
            var sql = string.Format(sqlFmt, this.TableName);
            var ids = dbctx.QueryAsArray<long>(SqlString.Parse(sql), parentID, parentID);

            return ids.ToArray();
        }

        private long[] GetDescendantIDs(IDbContext dbctx, long parentID)
        {
            var sqlFmt =
@"
select  hc.*
from    {0} hp
join    {0} hc ON hc._left between hp._left and hp._right
where   hp._id=? and hc._id<>?
";
            var sql = string.Format(sqlFmt, this.quotedTableName);
            var ids = dbctx.QueryAsArray<long>(SqlString.Parse(sql), parentID, parentID);
            return ids.ToArray();
        }

        private void ConvertFieldToColumn(
            ITransactionContext ctx, IRecord record, string[] updatableColumnFields)
        {

            foreach (var f in updatableColumnFields)
            {
                var fieldInfo = this.Fields[f];
                var columnValue = fieldInfo.SetFieldValue(ctx, record[f]);
                record[f] = columnValue;
            }
        }

        public override dynamic Browse(ITransactionContext ctx, long id)
        {
            return new BrowsableRecord(ctx, this, id);
        }

        private IDictionary<long, string> DefaultNameGetter(
            ITransactionContext ctx, long[] ids)
        {
            var result = new Dictionary<long, string>(ids.Count());
            if (this.Fields.ContainsKey("name"))
            {
                var records = this.ReadInternal(ctx, ids, new string[] { IdFieldName, "name" });
                foreach (var r in records)
                {
                    var id = (long)r[IdFieldName];
                    result.Add(id, (string)r["name"]);
                }
            }
            else
            {
                foreach (long id in ids)
                {
                    result.Add(id, string.Empty);
                }
            }

            return result;
        }

        private void AuditLog(ITransactionContext ctx, long id, string msg)
        {
            var logRecord = new Record()
                {
                    { "user", ctx.Session.UserId },
                    { "resource", this.Name },
                    { "resource_id", id },
                    { "description", msg }
                };
            var res = (IModel)ctx.GetResource(Core.AuditLogModel.ModelName);
            res.CreateInternal(ctx, logRecord);
        }

        public static string ToColumnList<T>(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            var sb = new StringBuilder();
            var flag = true;
            foreach (var item in items)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Append(",");
                }

                sb.Append('"' + item.ToString() + '"');
            }

            return sb.ToString();
        }


        /// <summary>
        /// 从用户提供的 record 里去掉系统只读的字段
        /// 本来用户不应该指定更新这些字段的，但是我们宽大为怀，饶恕这些无耻客户端的罪孽
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private static Record ClearUserRecord(IRecord record)
        {
            Debug.Assert(record != null);

            return record.Where(p => !SystemReadonlyFields.Contains(p.Key))
                .ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
