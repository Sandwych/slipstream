using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.Dynamic;


using ObjectServer.Backend;
using ObjectServer.Utility;
using ObjectServer.SqlTree;

namespace ObjectServer.Model
{
    public abstract partial class AbstractTableModel : AbstractModel
    {
        public const string LeftFieldName = "_left";
        public const string RightFieldName = "_right";
        public const string ParentFieldName = "parent";
        public const string ChildrenFieldName = "children";
        public const string DescendantsFieldName = "descendants";

        /// <summary>
        /// 这些字段是由 ORM 子系统处理的，客户端不能操作
        /// </summary>
        public static readonly string[] SystemReadonlyFields = new string[]
        {
            IDFieldName,
            CreatedTimeFieldName,
            CreatedUserFieldName,
            ModifiedTimeFieldName,
            ModifiedUserFieldName,
            VersionFieldName,
        };

        private string tableName = null;

        public override bool CanCreate { get; protected set; }
        public override bool CanRead { get; protected set; }
        public override bool CanWrite { get; protected set; }
        public override bool CanDelete { get; protected set; }

        public override bool LogCreation { get; protected set; }
        public override bool LogWriting { get; protected set; }

        public override bool Hierarchy { get; protected set; }

        public override bool DatabaseRequired { get { return true; } }

        public override NameGetter NameGetter { get; protected set; }

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
                    Logger.Error(() => "Table name cannot be empty");
                    throw new ArgumentNullException("value");
                }

                this.tableName = value;
                this.SequenceName = value + "_id_seq";
            }
        }

        public string SequenceName { get; protected set; }

        protected AbstractTableModel(string name)
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


            this.RegisterInternalServiceMethods();
        }


        private void SetName(string name)
        {
            this.TableName = name.ToLowerInvariant().Replace('.', '_');
        }

        /// <summary>
        /// 初始化数据库信息
        /// </summary>
        public override void Load(IDBProfile db)
        {
            this.AddInternalFields();

            base.Load(db);

            if (this.NameGetter == null)
            {
                this.NameGetter = this.DefaultNameGetter;
            }

            if (!this.Fields.ContainsKey("name"))
            {
                Logger.Info(() => string.Format(
                    "I strongly suggest you to add the 'name' field into Model '{0}'",
                    this.Name));
            }

            if (this.AutoMigration)
            {
                new TableMigrator(db, this).Migrate();
            }
        }


        private void RegisterInternalServiceMethods()
        {
            var selfType = typeof(AbstractTableModel);
            this.RegisterServiceMethod(selfType.GetMethod("Search"));
            this.RegisterServiceMethod(selfType.GetMethod("Create"));
            this.RegisterServiceMethod(selfType.GetMethod("Read"));
            this.RegisterServiceMethod(selfType.GetMethod("Write"));
            this.RegisterServiceMethod(selfType.GetMethod("Delete"));
        }

        private void AddInternalFields()
        {
            Debug.Assert(!this.Fields.ContainsKey(IDFieldName));

            Fields.BigInteger(IDFieldName).SetLabel("ID").Required();

            //只有非继承的模型才添加内置字段
            if (this.AutoMigration)
            {

                Fields.DateTime(CreatedTimeFieldName).SetLabel("Created")
                    .NotRequired().DefaultValueGetter(ctx => DateTime.Now).Readonly();

                Fields.DateTime(ModifiedTimeFieldName).SetLabel("Last Modified")
                    .NotRequired().DefaultValueGetter(ctx => DBNull.Value);

                Fields.ManyToOne(CreatedUserFieldName, "core.user").SetLabel("Creator")
                    .NotRequired().Readonly()
                    .DefaultValueGetter(ctx => ctx.Session.UserId > 0 ? (object)ctx.Session.UserId : DBNull.Value);

                Fields.ManyToOne(ModifiedUserFieldName, "core.user").SetLabel("Creator")
                    .NotRequired()
                    .DefaultValueGetter(ctx => ctx.Session.UserId > 0 ? (object)ctx.Session.UserId : DBNull.Value);

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
                .Required().DefaultValueGetter(ctx => -1);

            Fields.BigInteger(RightFieldName).SetLabel("Right Value")
                .Required().DefaultValueGetter(ctx => -1);

            //这里通过 SQL 查询返回
            Fields.ManyToOne(ParentFieldName, this.Name)
                .SetLabel("Parent");

            Fields.OneToMany(ChildrenFieldName, this.Name, ParentFieldName)
                .SetLabel("Children")
                .ValueGetter((scope, ids) =>
                {
                    var result = new Dictionary<long, object>(ids.Length);
                    foreach (var id in ids)
                    {
                        var children = this.GetChildrenIDs(scope.Connection, id);
                        result.Add(id, children);
                    }
                    return result;

                });

            Fields.OneToMany(DescendantsFieldName, this.Name, ParentFieldName)
                .SetLabel("Descendants")
                .ValueGetter((scope, ids) =>
                {
                    var result = new Dictionary<long, object>(ids.Length);
                    foreach (var id in ids)
                    {
                        var children = this.GetDescendantIDs(scope.Connection, id);
                        result.Add(id, children);
                    }
                    return result;

                });
        }

        /// <summary>
        /// 层次表获取某指定记录的直系子记录的 IDs
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private long[] GetChildrenIDs(IDBConnection conn, long parentID)
        {
            var sqlFmt =
@"
SELECT  hc.*
FROM    ""{0}"" hp
JOIN    ""{0}"" hc
ON      hc._left BETWEEN hp._left AND hp._right
WHERE   hp.id = @0 AND hc.id <> @0
        AND
        (
        SELECT  COUNT(hn.id)
        FROM    ""{0}"" hn
        WHERE   hc._left BETWEEN hn._left AND hn._right
                AND hn._left BETWEEN hp._left AND hp._right
        ) <= 2
";
            var sql = string.Format(sqlFmt, this.TableName);
            var ids = conn.QueryAsArray<long>(sql, parentID).Select(o => (long)o);

            return ids.ToArray();
        }

        private long[] GetDescendantIDs(IDBConnection conn, long parentID)
        {
            var sqlFmt =
@"
SELECT  hc.*
FROM    ""{0}"" hp
JOIN    ""{0}"" hc ON hc._left BETWEEN hp._left AND hp._right
WHERE   hp.id = @0 AND hc.id <> @0
";
            var sql = string.Format(sqlFmt, this.TableName);
            var ids = conn.QueryAsArray<long>(sql, parentID).Select(o => (long)o);
            return ids.ToArray();
        }

        private void ConvertFieldToColumn(
            IServiceScope ctx, Dictionary<string, object> record, string[] updatableColumnFields)
        {

            foreach (var f in updatableColumnFields)
            {
                var fieldInfo = this.Fields[f];
                var columnValue = fieldInfo.SetFieldValue(ctx, record[f]);
                record[f] = columnValue;
            }
        }

        #region Service Methods

        [ServiceMethod]
        public static long[] Search(
            IModel model, IServiceScope ctx, object[] domain = null, object[] order = null, long offset = 0, long limit = 0)
        {
            OrderExpression[] orderInfos = OrderExpression.GetDefaultOrders();

            if (order != null)
            {
                orderInfos = new OrderExpression[order.Length];
                for (int i = 0; i < orderInfos.Length; i++)
                {
                    var orderTuple = (object[])order[i];
                    var so = SortDirectionParser.Parser((string)orderTuple[1]);
                    orderInfos[i] = new OrderExpression((string)orderTuple[0], so);
                }
            }

            return model.SearchInternal(ctx, (object[][])domain, orderInfos, offset, limit);
        }

        [ServiceMethod]
        public static Dictionary<string, object>[] Read(
            IModel model, IServiceScope ctx, object[] clientIds, object[] fields = null)
        {
            string[] strFields = null;
            if (fields != null)
            {
                strFields = fields.Select(f => (string)f).ToArray();
            }
            var ids = clientIds.Select(id => (long)id).ToArray();
            return model.ReadInternal(ctx, ids, strFields);
        }

        [ServiceMethod]
        public static long Create(
            IModel model, IServiceScope ctx, IDictionary<string, object> propertyBag)
        {
            return model.CreateInternal(ctx, propertyBag);
        }

        [ServiceMethod]
        public static void Write(
           IModel model, IServiceScope ctx, object id, IDictionary<string, object> userRecord)
        {
            model.WriteInternal(ctx, (long)id, userRecord);
        }

        [ServiceMethod]
        public static void Delete(
            IModel model, IServiceScope ctx, object[] ids)
        {
            var longIds = ids.Select(id => (long)id).ToArray();
            model.DeleteInternal(ctx, longIds);
        }

        #endregion


        public override dynamic Browse(IServiceScope ctx, long id)
        {
            return new BrowsableRecord(ctx, this, id);
        }

        private IDictionary<long, string> DefaultNameGetter(
            IServiceScope ctx, long[] ids)
        {
            var result = new Dictionary<long, string>(ids.Count());
            if (this.Fields.ContainsKey("name"))
            {
                var records = this.ReadInternal(ctx, ids, new string[] { "id", "name" });
                foreach (var r in records)
                {
                    var id = (long)r["id"];
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

        private void AuditLog(IServiceScope ctx, long id, string msg)
        {

            var logRecord = new Dictionary<string, object>()
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


        private static Dictionary<string, object> ClearUserRecord(IDictionary<string, object> record)
        {
            Debug.Assert(record != null);

            return record.Where(p => !SystemReadonlyFields.Contains(p.Key))
                .ToDictionary(p => p.Key, p => p.Value);
        }

    }
}
