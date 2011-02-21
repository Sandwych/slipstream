using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;

using log4net;


using ObjectServer.Backend;
using ObjectServer.Utility;

namespace ObjectServer.Model
{
    public abstract class TableModel : ModelBase, IModel
    {
        private readonly List<IField> modelFields =
            new List<IField>();

        private string tableName = null;
        private string name = null;

        public bool CanCreate { get; protected set; }
        public bool CanRead { get; protected set; }
        public bool CanWrite { get; protected set; }
        public bool CanDelete { get; protected set; }

        public bool Hierarchy { get; protected set; }

        public override bool DatabaseRequired { get { return true; } }

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
                    Log.Error("Model name cannot be empty");
                    throw new ArgumentNullException("value");
                }

                this.name = value;
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
                    Log.Error("Table name cannot be empty");
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
        public override void Initialize(IDatabase db, ObjectPool pool)
        {
            base.Initialize(db, pool);

            var migrator = new TableMigrator(db, pool, this);
            migrator.Migrate();
        }


        public static Type[] GetAllCoreModels()
        {
            var a = Assembly.GetExecutingAssembly();
            return GetModelsFromAssembly(a);
        }

        public static Type[] GetModelsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var result = new List<Type>();
            foreach (var t in types)
            {
                var assemblies = t.GetCustomAttributes(typeof(ServiceObjectAttribute), false);
                if (assemblies.Length > 0)
                {
                    result.Add(t);
                }
            }
            return result.ToArray();
        }

        #region Service Methods

        [ServiceMethod]
        public virtual long[] Search(ISession session, object[][] domain, long offset, long limit)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            var query = new ModelQuery(session, this);
            return query.Search(domain, offset, limit);
        }

        [ServiceMethod]
        public virtual long Create(ISession session, IDictionary<string, object> propertyBag)
        {
            if (!this.CanCreate)
            {
                throw new NotSupportedException();
            }

            //TODO 这里改成定义的列插入，而不是用户提供的列            
            var values = new Dictionary<string, object>(propertyBag);

            //处理用户没有给的默认值
            this.AddDefaultValues(session, values);

            return DoCreate(session, values);
        }

        private long DoCreate(ISession session, IDictionary<string, object> values)
        {
            this.VerifyFields(values.Keys);

            var serial = session.Database.NextSerial(this.SequenceName);

            if (this.ContainsField("_version"))
            {
                values.Add("_version", 0);
            }

            var colValues = new object[values.Count];
            var sbColumns = new StringBuilder();
            var sbArgs = new StringBuilder();
            var firstColumn = true;
            var index = 0;
            foreach (var f in values.Keys)
            {
                if (firstColumn)
                {
                    firstColumn = false;
                }
                else
                {
                    sbColumns.Append(", ");
                    sbArgs.Append(", ");
                }

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
              "INSERT INTO \"{0}\" (\"id\", {1}) VALUES ({2}, {3});",
              this.TableName,
              columnNames,
              serial,
              args);

            var rows = session.Database.Execute(sql, colValues);
            if (rows != 1)
            {
                Log.ErrorFormat("Failed to insert row, SQL: {0}", sql);
                throw new DataException();
            }


            return serial;
        }

        /// <summary>
        /// 添加没有包含在字典 dict 里但是有默认值函数的字段
        /// </summary>
        /// <param name="session"></param>
        /// <param name="values"></param>
        private void AddDefaultValues(ISession session, IDictionary<string, object> propertyBag)
        {
            var defaultFields =
                this.DefinedFields.Where(
                d => (d.DefaultProc != null && !propertyBag.Keys.Contains(d.Name)));

            foreach (var df in defaultFields)
            {
                propertyBag[df.Name] = df.DefaultProc(session);
            }
        }

        [ServiceMethod]
        public virtual void Write(ISession session, object id, IDictionary<string, object> record)
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

            session.Database.Execute(sql, values);
        }

        [ServiceMethod]
        public virtual Dictionary<string, object>[] Read(
            ISession session, object[] ids, object[] fields)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }

            var allFields = new List<string>();
            if (fields == null || fields.Length == 0)
            {
                allFields.AddRange(this.DefinedFields.Select(f => f.Name));
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
                (from d in this.DefinedFields
                 from f in allFields
                 where d.Name == f && d.IsStorable()
                 select d.Name).ToArray();

            //.Where(f => !this.declaredFields[f].IsFunctionField);

            //TODO: SQL 注入问题
            var sql = string.Format("select {0} from \"{1}\" where \"id\" in ({2});",
                columnFields.ToCommaList(),
                this.TableName,
                ids.ToCommaList());

            //先查找表里的简单字段数据
            var result = session.Database.QueryAsDictionary(sql);

            //处理特殊字段
            foreach (var fieldName in allFields)
            {
                var f = this.DefinedFields.Single(i => i.Name == fieldName);

                if (f.Functional)
                {
                    var funcFieldValues = f.Getter(session, ids);
                    foreach (var p in result)
                    {
                        var id = (long)p["id"];
                        p[f.Name] = funcFieldValues[id];
                    }
                }
                else if (f.Type == FieldType.ManyToOne)
                {
                    //获取所有 Name

                    var masterModel = (TableModel)session.Pool.LookupObject(f.Relation);
                    if (masterModel.ContainsField("name")) //如果有 name 字段
                    {
                        var masterTableIds = result.Select(d => d[f.Name]).ToArray();
                        var masters = masterModel.Read(session, masterTableIds, new object[] { "name" });
                        var masterNames = new Dictionary<long, string>(masters.Length);
                        foreach (var master in masters)
                        {
                            var masterId = (long)master["id"];
                            masterNames[masterId] = (string)master["name"];
                        }
                        foreach (var p in result)
                        {
                            var id = (long)p["id"];
                            p[f.Name] = new RelatedField(id, masterNames[id]);
                        }
                    }
                    else
                    {
                        foreach (var p in result)
                        {
                            var id = (long)p["id"];
                            p[f.Name] = new RelatedField(id, string.Empty);
                        }
                    }
                }
                else if (f.Type == FieldType.OneToMany)
                {
                    //查询字表
                    var childModel = (TableModel)session.Pool.LookupObject(f.Relation);
                    //TODO 权限等处理

                    var children = new Dictionary<long, long[]>();
                    foreach (long masterId in ids)
                    {
                        var domain = new List<object[]>();
                        domain.Add(new object[] { f.RelatedField, "=", masterId });
                        var childIds = childModel.Search(session, domain.ToArray(), 0, 0xffff);
                        children[masterId] = childIds;
                    }

                    foreach (var p in result)
                    {
                        var masterId = (long)p["id"];
                        p[f.Name] = children[masterId];
                    }

                }
            }

            return result.ToArray();
        }

        [ServiceMethod]
        public virtual void Delete(ISession session, object[] ids)
        {
            if (!this.CanDelete)
            {
                throw new NotSupportedException();
            }

            var sql = string.Format(
                "delete from \"{0}\" where \"id\" in ({1});",
                this.TableName, ids.ToCommaList());

            var rowCount = session.Database.Execute(sql);
            if (rowCount != ids.Count())
            {
                throw new DataException();
            }
        }

        #endregion

    }
}
