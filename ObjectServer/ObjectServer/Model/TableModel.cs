using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;


using ObjectServer.Backend;
using ObjectServer.Utility;

namespace ObjectServer.Model
{
    public abstract class TableModel : ModelBase, IModel
    {
        private readonly List<IMetaField> modelFields =
            new List<IMetaField>();

        private string tableName = null;
        private string name = null;

        public bool CanCreate { get; protected set; }
        public bool CanRead { get; protected set; }
        public bool CanWrite { get; protected set; }
        public bool CanDelete { get; protected set; }

        public bool Hierarchy { get; protected set; }

        public override bool DatabaseRequired { get { return true; } }

        public NameGetter NameGetter { get; protected set; }

        public override string Name
        {
            get
            {
                return this.name;
            }
            protected set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Logger.Error(() => "Model name cannot be empty");
                    throw new ArgumentNullException("value");
                }
                this.name = value;
                this.VerifyName();
                this.TableName = value.ToLowerInvariant().Replace('.', '_');

                if (string.IsNullOrEmpty(this.Label))
                {
                    this.Label = value;
                }

                base.Module = this.name.Split('.')[0];
            }
        }

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

        protected TableModel()
            : base()
        {
            this.CanCreate = true;
            this.CanRead = true;
            this.CanWrite = true;
            this.CanDelete = true;
            this.Hierarchy = false;

            this.DateTimeField("_created_time", "Created", false, null, null);
            this.DateTimeField("_modified_time", "Last Modified", false, null, null);
            this.ManyToOneField("_created_user", "core.user", "Creator", false, null, null);
            this.ManyToOneField("_modified_user", "core.user", "Creator", false, null, null);
        }

        /// <summary>
        /// 初始化数据库信息
        /// </summary>
        public override void Initialize(IDatabaseContext db, ObjectPool pool)
        {
            base.Initialize(db, pool);

            if (this.NameGetter == null)
            {
                this.NameGetter = this.DefaultNameGetter;
            }

            if (!this.DefinedFields.ContainsKey("name"))
            {
                Logger.Info(() => string.Format(
                    "I strongly suggest you to add the 'name' field into Model '{0}'",
                    this.Name));
            }

            var migrator = new TableMigrator(db, pool, this);
            migrator.Migrate();
        }


        #region Service Methods

        [ServiceMethod]
        public virtual object[] Search(ICallingContext ctx, object[] domain, long offset, long limit)
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

        [ServiceMethod]
        public virtual long Create(ICallingContext callingContext, IDictionary<string, object> propertyBag)
        {
            if (!this.CanCreate)
            {
                throw new NotSupportedException();
            }

            //TODO 这里改成定义的列插入，而不是用户提供的列            
            var values = new Dictionary<string, object>(propertyBag);

            //处理用户没有给的默认值
            this.AddDefaultValues(callingContext, values);

            return DoCreate(callingContext, values);
        }

        private long DoCreate(ICallingContext callingContext, IDictionary<string, object> values)
        {
            this.VerifyFields(values.Keys);

            var serial = callingContext.Database.NextSerial(this.SequenceName);

            if (this.ContainsField("_version"))
            {
                values.Add("_version", 0);
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

            var rows = callingContext.Database.Execute(sql, colValues);
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
        private void AddDefaultValues(ICallingContext callingContext, IDictionary<string, object> propertyBag)
        {
            var defaultFields =
                this.DefinedFields.Values.Where(
                d => (d.DefaultProc != null && !propertyBag.Keys.Contains(d.Name)));

            foreach (var df in defaultFields)
            {
                propertyBag[df.Name] = df.DefaultProc(callingContext);
            }
        }

        [ServiceMethod]
        public virtual void Write(ICallingContext callingContext, object id, IDictionary<string, object> record)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException();
            }

            //检查字段

            //TODO: 并发检查，处理 _version 字段
            //客户端也需要提供 _version 字段
            var values = new object[record.Count];

            var sbFieldValues = new StringBuilder(15 * record.Count);
            bool isFirstLine = true;
            int i = 0;
            foreach (var pair in record)
            {
                if (!isFirstLine)
                {
                    sbFieldValues.Append(", ");
                    isFirstLine = false;
                }
                values[i] = pair.Value;
                sbFieldValues.AppendFormat("{0}=@{1}", pair.Key, i);
                i++;
            }

            var sql = string.Format(
                "UPDATE \"{0}\" SET {1} WHERE id = {2};",
                this.TableName,
                sbFieldValues.ToString(),
                id);

            callingContext.Database.Execute(sql, values);
        }

        [ServiceMethod]
        public virtual Dictionary<string, object>[] Read(
            ICallingContext callingContext, object[] ids, object[] fields)
        {
            if (callingContext == null)
            {
                throw new ArgumentNullException("callingContext");
            }

            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            if (ids == null || ids.Length == 0)
            {
                throw new ArgumentException("'ids' cannot be null", "ids");
            }

            var allFields = new List<string>();
            if (fields == null || fields.Length == 0)
            {
                allFields.AddRange(this.DefinedFields.Keys);
            }
            else
            {
                //检查是否有不存在的列
                var userFields = fields.Select(o => (string)o);
                this.VerifyFields(userFields);

                allFields.Capacity = fields.Count();
                allFields.AddRange(userFields);
            }

            if (!allFields.Contains("id"))
            {
                allFields.Add("id");
            }

            //表里的列，也就是可以直接用 SQL 查的列
            var columnFields =
                (from f in allFields
                 where this.DefinedFields[f].IsStorable()
                 select f).ToArray();

            //.Where(f => !this.declaredFields[f].IsFunctionField);

            var sql = string.Format("SELECT {0} FROM \"{1}\" WHERE \"id\" IN ({2})",
                columnFields.ToCommaList(),
                this.TableName,
                ids.ToCommaList());

            //先查找表里的简单字段数据
            var records = callingContext.Database.QueryAsDictionary(sql);

            //处理特殊字段
            foreach (var fieldName in allFields)
            {
                var f = this.DefinedFields[fieldName];
                if (f.Name == "id")
                {
                    continue;
                }

                var fieldValues = f.GetFieldValues(callingContext, records);
                foreach (var record in records)
                {
                    var id = (long)record["id"];
                    record[f.Name] = fieldValues[id];
                }
            }

            return records.ToArray();
        }

        [ServiceMethod]
        public virtual void Delete(ICallingContext callingContext, object[] ids)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            var sql = string.Format(
                "DELETE FROM \"{0}\" WHERE \"id\" IN ({1});",
                this.TableName, ids.ToCommaList());

            var rowCount = callingContext.Database.Execute(sql);
            if (rowCount != ids.Count())
            {
                throw new DataException();
            }
        }

        #endregion


        private IDictionary<long, string> DefaultNameGetter(ICallingContext callingContext, object[] ids)
        {
            var result = new Dictionary<long, string>(ids.Length);
            if (this.DefinedFields.ContainsKey("name"))
            {
                var records = this.Read(callingContext, ids, new object[] { "id", "name" });
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

    }
}
