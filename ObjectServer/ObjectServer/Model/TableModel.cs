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
    public abstract class TableModel : ModelBase, IModel
    {
        public const string CreatedTimeField = "_created_time";
        public const string ModifiedTimeField = "_modified_time";
        public const string CreatedUserField = "_created_user";
        public const string ModifiedUserField = "_modified_user";

        private readonly List<IMetaField> modelFields =
            new List<IMetaField>();

        private string tableName = null;

        public bool CanCreate { get; protected set; }
        public bool CanRead { get; protected set; }
        public bool CanWrite { get; protected set; }
        public bool CanDelete { get; protected set; }

        public bool LogCreation { get; protected set; }
        public bool LogWriting { get; protected set; }

        public bool Hierarchy { get; protected set; }

        public override bool DatabaseRequired { get { return true; } }

        public NameGetter NameGetter { get; protected set; }

        public string TableName
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

        protected TableModel(string name)
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

            Fields.DateTime(CreatedTimeField).SetLabel("Created")
                .NotRequired().SetDefaultProc(ctx => DateTime.Now);

            Fields.DateTime(ModifiedTimeField).SetLabel("Last Modified")
                .NotRequired().SetDefaultProc(ctx => DBNull.Value);

            Fields.ManyToOne(CreatedUserField, "core.user").SetLabel("Creator")
                .NotRequired().Readonly()
                .SetDefaultProc(ctx => ctx.Session.UserId > 0 ? (object)ctx.Session.UserId : DBNull.Value);

            Fields.ManyToOne(ModifiedUserField, "core.user").SetLabel("Creator")
                .NotRequired().SetDefaultProc(ctx => DBNull.Value);
        }

        private void SetName(string name)
        {
            this.TableName = name.ToLowerInvariant().Replace('.', '_');
        }

        /// <summary>
        /// 初始化数据库信息
        /// </summary>
        public override void Load(IDatabase db)
        {
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

            var migrator = new TableMigrator(db, this);
            migrator.Migrate();

        }

        public virtual object[] SearchInternal(
            IContext ctx, object[] domain = null, long offset = 0, long limit = 0)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            object[] domainInternal = domain;
            if (domain == null)
            {
                domainInternal = new object[][] { };
            }

            var query = new ModelQuery(ctx, this);
            return query.Search(domainInternal, offset, limit);
        }

        public virtual long CreateInternal(IContext ctx, IDictionary<string, object> propertyBag)
        {
            if (!this.CanCreate)
            {
                throw new NotSupportedException();
            }

            //TODO 这里改成定义的列插入，而不是用户提供的列            
            var values = new Dictionary<string, object>(propertyBag);

            //处理用户没有给的默认值
            this.AddDefaultValues(ctx, values);

            var id = DoCreate(ctx, values);

            if (this.LogCreation)
            {
                //TODO: 可翻译的
                this.AuditLog(ctx, id, this.Label + " created");
            }

            return id;
        }

        private long DoCreate(IContext ctx, IDictionary<string, object> values)
        {
            this.VerifyFields(values.Keys);

            var serial = ctx.Database.DataContext.NextSerial(this.SequenceName);

            if (this.ContainsField(VersionFieldName))
            {
                values.Add(VersionFieldName, 0);
            }

            var colValues = new object[values.Count];
            var sbColumns = new StringBuilder();
            var sbArgs = new StringBuilder();
            var index = 0;
            foreach (var f in values.Keys)
            {
                sbColumns.Append(", ");
                sbArgs.Append(", ");

                colValues[index] = values[f];

                sbArgs.Append("@" + index.ToString());
                sbColumns.Append('\"');
                sbColumns.Append(f);
                sbColumns.Append('\"');
                index++;
            }

            var columnNames = sbColumns.ToString();
            var args = sbArgs.ToString();

            var sql = string.Format(
              "INSERT INTO \"{0}\" (\"id\" {1}) VALUES ( {2} {3} );",
              this.TableName,
              columnNames,
              serial,
              args);

            var rows = ctx.Database.DataContext.Execute(sql, colValues);
            if (rows != 1)
            {
                Logger.Error(() => string.Format("Failed to insert row, SQL: {0}", sql));
                throw new DataException();
            }


            return serial;
        }

        /// <summary>
        /// 添加没有包含在字典 dict 里但是有默认值函数的字段
        /// </summary>
        /// <param name="session"></param>
        /// <param name="values"></param>
        private void AddDefaultValues(IContext ctx, IDictionary<string, object> propertyBag)
        {
            var defaultFields =
                this.Fields.Values.Where(
                d => (d.DefaultProc != null && !propertyBag.Keys.Contains(d.Name)));

            foreach (var df in defaultFields)
            {
                propertyBag[df.Name] = df.DefaultProc(ctx);
            }
        }

        public virtual void WriteInternal(
            IContext ctx, object id, IDictionary<string, object> userRecord)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException();
            }

            var record = new Dictionary<string, object>(userRecord);

            //处理最近更新用户与最近更新时间字段            
            if (this.ContainsField(ModifiedTimeField))
            {
                record[ModifiedTimeField] = DateTime.Now;
            }
            if (this.ContainsField(ModifiedUserField))
            {
                record[ModifiedUserField] = ctx.Session.UserId;
            }

            var allFields = record.Keys; //记录中的所有字段
            //所有可更新的字段
            var updatableColumnFields = allFields.Where(
                f => this.Fields[f].IsColumn() && !this.Fields[f].IsReadonly);

            //TODO 处理复杂字段
            //检查字段

            var columns = new List<IBinaryExpression>(record.Count);
            int i = 0;
            var args = new List<object>(record.Count);
            foreach (var field in updatableColumnFields)
            {
                var exp = new BinaryExpression(
                    new IdentifierExpression(field),
                    ExpressionOperator.EqualOperator,
                    new ParameterExpression("@" + i.ToString()));
                columns.Add(exp);
                args.Add(record[field]);

                ++i;
            }

            var whereExp = new BinaryExpression(
                new AliasExpression("id"),
                ExpressionOperator.EqualOperator,
                new ValueExpression(id));


            //如果存在 _version 字段就加入版本检查条件
            //TODO: 是否强制要求客户端必须送来 _version 字段？
            if (record.ContainsKey(VersionFieldName))
            {
                var version = (long)record[VersionFieldName];
                var verExp = new BinaryExpression(
                    new AliasExpression(VersionFieldName),
                    ExpressionOperator.EqualOperator,
                    new ValueExpression(version)); //现存数据库的版本必须比用户提供的版本相同
                whereExp = new BinaryExpression(
                    whereExp,
                    ExpressionOperator.AndOperator,
                    verExp);

                //版本号也必须更新
                record[VersionFieldName] = version + 1;
            }

            var updateStatement = new UpdateStatement(
                new AliasExpression(this.TableName),
                new SetClause(columns),
                new WhereClause(whereExp));

            var sql = updateStatement.ToString();
            Console.WriteLine(sql);

            var rowsAffected = ctx.Database.DataContext.Execute(sql, args.ToArray());

            //检查更新结果
            if (rowsAffected != 1)
            {
                var msg = string.Format("不能更新 ['{0}', {1}]，因为其已经被其它用户更新",
                    this.TableName, id);
                throw new ConcurrencyException(msg);
            }

            if (this.LogWriting)
            {
                AuditLog(ctx, (long)id, this.Label + " updated");
            }
        }


        public virtual Dictionary<string, object>[] ReadInternal(
            IContext ctx, object[] ids, object[] fields)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            if (ids == null || ids.Length == 0)
            {
                throw new ArgumentException("'ids' cannot be null", "ids");
            }

            IList<string> allFields;
            if (fields == null || fields.Length == 0)
            {
                allFields = this.Fields.Where(p => !p.Value.Lazy)
                    .Select(p => p.Value.Name).ToList();
            }
            else
            {
                //检查是否有不存在的列
                var userFields = fields.Select(o => (string)o);
                this.VerifyFields(userFields);

                allFields = userFields.ToList();
            }

            if (!allFields.Contains("id"))
            {
                allFields.Add("id");
            }

            //表里的列，也就是可以直接用 SQL 查的列
            var columnFields =
                (from f in allFields
                 where this.Fields[f].IsColumn()
                 select f).ToArray();

            //.Where(f => !this.declaredFields[f].IsFunctionField);

            var sql = string.Format("SELECT {0} FROM \"{1}\" WHERE \"id\" IN ({2})",
                columnFields.ToCommaList(),
                this.TableName,
                ids.ToCommaList());

            //先查找表里的简单字段数据
            var records = ctx.Database.DataContext.QueryAsDictionary(sql);

            //处理特殊字段
            foreach (var fieldName in allFields)
            {
                var f = this.Fields[fieldName];
                if (f.Name == "id")
                {
                    continue;
                }

                var fieldValues = f.GetFieldValues(ctx, records);
                foreach (var record in records)
                {
                    var id = (long)record["id"];
                    record[f.Name] = fieldValues[id];
                }
            }

            return records.ToArray();
        }


        public virtual void DeleteInternal(IContext ctx, object[] ids)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            if (ids == null || ids.Length == 0)
            {
                throw new ArgumentNullException("ids");
            }

            var sql = string.Format(
                "DELETE FROM \"{0}\" WHERE \"id\" IN ({1});",
                this.TableName, ids.ToCommaList());

            var rowCount = ctx.Database.DataContext.Execute(sql);
            if (rowCount != ids.Count())
            {
                throw new DataException();
            }
        }


        #region Service Methods


        [ServiceMethod]
        public static object[] Search(
            dynamic model, IContext ctx, object[] domain = null, long offset = 0, long limit = 0)
        {
            return model.SearchInternal(ctx, domain, offset, limit);
        }

        [ServiceMethod]
        public static Dictionary<string, object>[] Read(
            dynamic model, IContext ctx, object[] ids, object[] fields)
        {
            return model.ReadInternal(ctx, ids, fields);
        }

        [ServiceMethod]
        public static long Create(
            dynamic model, IContext ctx, IDictionary<string, object> propertyBag)
        {
            return model.CreateInternal(ctx, propertyBag);
        }

        [ServiceMethod]
        public static void Write(
           dynamic model, IContext ctx, object id, IDictionary<string, object> userRecord)
        {
            model.WriteInternal(ctx, id, userRecord);
        }

        [ServiceMethod]
        public static void Delete(dynamic model, IContext ctx, object[] ids)
        {
            model.DeleteInternal(ctx, ids);
        }


        #endregion


        public virtual dynamic Browse(IContext ctx, object id)
        {
            var record = this.ReadInternal(ctx, new object[] { id }, null)[0];
            return new BrowsableModel(ctx, this, record);
        }

        private IDictionary<long, string> DefaultNameGetter(IContext ctx, object[] ids)
        {
            var result = new Dictionary<long, string>(ids.Length);
            if (this.Fields.ContainsKey("name"))
            {
                var records = this.ReadInternal(ctx, ids, new object[] { "id", "name" });
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

        private void AuditLog(IContext ctx, long id, string msg)
        {

            var logRecord = new Dictionary<string, object>()
                {
                    { "user", ctx.Session.UserId },
                    { "resource", this.Name },
                    { "resource_id", id },
                    { "description", msg }
                };
            var res = (IModel)ctx.Database.GetResource(Core.AuditLogModel.ModelName);
            res.CreateInternal(ctx, logRecord);

        }

    }
}
