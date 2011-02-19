using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;

using log4net;


using ObjectServer.Backend;
using ObjectServer.Utility;
using ObjectServer.Model.Query;

namespace ObjectServer.Model
{
    public abstract class ModelBase : IServiceObject
    {
        protected static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<IField> declaredFields =
            new List<IField>();

        private readonly List<IField> modelFields =
            new List<IField>();

        private readonly List<IField> requiredFields = new List<IField>();

        private ITable table;

        private string tableName = null;
        private string name = null;

        private readonly Dictionary<string, MethodInfo> serviceMethods =
            new Dictionary<string, MethodInfo>();

        public ObjectPool Pool { get; set; }

        public bool CanCreate { get; protected set; }
        public bool CanRead { get; protected set; }
        public bool CanWrite { get; protected set; }
        public bool CanDelete { get; protected set; }
        public string Label { get; protected set; }
        public bool Hierarchy { get; protected set; }
        public bool Versioned { get; protected set; }
        public string Module { get; private set; }

        public bool DatabaseRequired { get { return true; } }

        public string Name
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

                this.Module = this.name.Split('.')[0];
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

        protected ModelBase()
        {
            this.CanCreate = true;
            this.CanRead = true;
            this.CanWrite = true;
            this.CanDelete = true;
            this.Hierarchy = false;
            this.Versioned = true;

            this.RegisterAllServiceMethods();
        }

        /// <summary>
        /// 初始化数据库信息
        /// </summary>
        public virtual void Initialize(IDatabase db)
        {
            this.AddInternalFields();

            this.table = db.CreateTableHandler(this.TableName);

            //检测此模块是否存在于数据库 core_model 表
            var sql = "SELECT DISTINCT COUNT(id) FROM core_model WHERE name=@0";
            var count = (long)db.QueryValue(sql, this.Name);
            if (count <= 0)
            {
                this.CreateModel(db);
            }

            //如果表不存在就自动建表
            if (!this.table.TableExists(db, this.TableName))
            {
                this.CreateTable(db);
            }

            //TODO:
            //检查字段表里的定义与实际的数据库表有什么变化，自动迁移
        }

        private void AddInternalFields()
        {
            BitIntegerField("id", "ID", true, null, null);

            if (this.Versioned)
            {
                BitIntegerField("_version", "Version", true, null, null);
            }
            //DefineField("_creator", "Creation User", "BIGINT", 1);
            //DefineField("_updator", "Last Modifiation User", "BIGINT", 1);
        }

        private void CreateModel(IDatabase db)
        {
            var rowCount = db.Execute(
                "INSERT INTO core_model(name, module, label) VALUES(@0, @1, @2);",
                this.Name, this.Module, this.Label);

            if (rowCount != 1)
            {
                throw new DataException("Failed to insert record of table core_model");
            }
        }

        private void CreateTable(IDatabase db)
        {
            this.table.CreateTable(db, this.TableName, this.Label);

            //创建字段
            if (this.Hierarchy)
            {
                //conn.ExecuteNonQuery("");
            }

            foreach (var field in this.declaredFields)
            {
                if (field.IsStorable() && field.Name != "id")
                {
                    table.AddColumn(db, field);
                }
            }
        }

        #region Field Methods


        protected void IntegerField(string name, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Integer)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            declaredFields.Add(field);
        }

        protected void BitIntegerField(string name, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.BigInteger)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void BooleanField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Boolean)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void TextField(
            string name, string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Text)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void CharsField(
            string name, string label, int size, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.Chars)
            {
                Label = label,
                Size = size,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void ManyToOneField(
            string name, string masterModel, string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.ManyToOne)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
                Relation = masterModel,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void OneToManyField(
            string name, string childModel, string relatedField,
            string label, bool required,
            FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.OneToMany)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
                Relation = childModel,
                RelatedField = relatedField,
            };

            field.Validate();
            declaredFields.Add(field);
        }

        protected void ManyToManyField(
            string name, string relatedModel,
            string refTableName, string originField, string targetField,
            string label, bool required, FieldGetter getter, FieldDefaultProc defaultProc)
        {
            var field = new Field(name, FieldType.ManyToMany)
            {
                Label = label,
                Required = required,
                Getter = getter,
                DefaultProc = defaultProc,
                Relation = refTableName,
                OriginField = targetField,
                RelatedField = originField,
            };

            field.Validate();
            declaredFields.Add(field);
        }


        #endregion

        private void RegisterAllServiceMethods()
        {
            var t = this.GetType();
            var methods = t.GetMethods();
            foreach (var m in methods)
            {
                var attrs = m.GetCustomAttributes(typeof(ServiceMethodAttribute), false);
                if (attrs.Length > 0)
                {
                    this.RegisterServiceMethod(m);
                }
            }
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

        public static IServiceObject CreateModelInstance(IDatabase db, Type t)
        {
            var obj = Activator.CreateInstance(t) as IServiceObject;
            if (obj == null)
            {
                var msg = string.Format("类型 '{0}' 没有实现 IServiceObject 接口", t.FullName);
                throw new InvalidCastException(msg);
            }
            obj.Initialize(db);
            return obj;
        }

        /// <summary>
        /// 为动态语言预留的
        /// </summary>
        public void RegisterServiceMethod(MethodInfo mi)
        {
            this.serviceMethods.Add(mi.Name, mi);
        }

        public MethodInfo GetServiceMethod(string name)
        {
            return this.serviceMethods[name];
        }


        public long[] Search(ISession session, IExpression exp)
        {
            if (exp == null)
            {
                throw new ArgumentNullException("exp");
            }

            var ruleExp = new TrueExpression();

            var sql = string.Format(
                "select id from \"{0}\" where ({1}) and ({2});",
                this.TableName, exp.ToSqlString(), ruleExp.ToSqlString());

            using (var cmd = session.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    var result = new List<long>();
                    while (reader.Read())
                    {
                        result.Add(reader.GetInt64(0));
                    }
                    return result.ToArray();
                }
            }
        }

        #region Service Methods

        [ServiceMethod]
        public virtual long[] Search(ISession session, string domain, long offset, long limit)
        {
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }
            //TODO: exp to IExpression
            //example: "and((like name '%test%') (equal address 'street1'))"
            return this.Search(session, new TrueExpression());
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

        private long DoCreate(ISession session, Dictionary<string, object> values)
        {
            this.VerifyFields(values.Keys);

            var serial = session.Database.NextSerial(this.SequenceName);

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
              "INSERT INTO \"{0}\" (\"id\", \"_version\", {1}) VALUES ({2}, 0, {3});",
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
                this.declaredFields.Where(
                d => (d.DefaultProc != null && !propertyBag.Keys.Contains(d.Name)));

            foreach (var df in defaultFields)
            {
                propertyBag[df.Name] = df.DefaultProc(session);
            }
        }

        [ServiceMethod]
        public virtual void Write(ISession session, long id, IDictionary<string, object> record)
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
            ISession session, IEnumerable<long> ids, IEnumerable<string> fields)
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
            if (fields == null || fields.Count() == 0)
            {
                allFields.AddRange(this.declaredFields.Select(f => f.Name));
            }
            else
            {
                //检查是否有不存在的列
                this.VerifyFields(fields);

                allFields.Capacity = fields.Count();
                allFields.AddRange(fields);
            }

            if (!allFields.Contains("id"))
            {
                allFields.Add("id");
            }

            //表里的列，也就是可以直接用 SQL 查的列
            var columnFields =
                (from d in this.declaredFields
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

            //处理函数字段
            foreach (var fieldName in allFields)
            {
                var f = this.declaredFields.Single(i => i.Name == fieldName);

                if (f.Functional)
                {
                    var funcFieldValues = f.Getter(session, ids);
                    foreach (var p in result)
                    {
                        var id = (long)p["id"];
                        p[f.Name] = funcFieldValues[id];
                    }
                }
            }

            return result.ToArray();
        }

        [ServiceMethod]
        public virtual void Delete(ISession session, IEnumerable<long> ids)
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


        private void VerifyFields(IEnumerable<string> fields)
        {
            Debug.Assert(fields != null);
            var notExistedFields =
                fields.Count(fn => !this.declaredFields.Exists(f => f.Name == fn));
            if (notExistedFields > 0)
            {
                throw new ArgumentException("fields");
            }

            var internalFields =
                 fields.Count(fn => this.declaredFields.Exists(f => f.Name == fn && f.Internal));
            if (internalFields > 0)
            {
                throw new ArgumentException("fields");
            }
        }

    }
}
