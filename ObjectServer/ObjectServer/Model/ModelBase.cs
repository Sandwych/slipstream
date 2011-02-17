using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;

using log4net;
using Npgsql;
using NpgsqlTypes;

using ObjectServer.Backend;
using ObjectServer.Utility;
using ObjectServer.Model.Query;
using ObjectServer.Model.Fields;

namespace ObjectServer.Model
{
    public abstract class ModelBase : IServiceObject
    {
        protected static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IField> declaredFields =
            new Dictionary<string, IField>();

        private Dictionary<string, IField> modelFields =
            new Dictionary<string, IField>();

        private string tableName = null;
        private string name = null;

        private Dictionary<string, MethodInfo> serviceMethods =
            new Dictionary<string, MethodInfo>();

        public bool CanCreate { get; protected set; }
        public bool CanRead { get; protected set; }
        public bool CanWrite { get; protected set; }
        public bool CanDelete { get; protected set; }
        public bool Automatic { get; protected set; }
        public string Label { get; protected set; }
        public bool Hierarchy { get; protected set; }
        public bool Versioned { get; protected set; }

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
            this.Automatic = true;
            this.Hierarchy = false;
            this.Versioned = true;

            this.RegisterAllServiceMethods();
        }

        /// <summary>
        /// 初始化数据库信息
        /// </summary>
        public void Initialize(Database db)
        {
            this.AddInternalFields();

            //检测此模块是否存在于数据库 core_model 表
            var sql = "SELECT DISTINCT COUNT(id) FROM core_model WHERE name=@0";
            var count = (long)db.QueryValue(sql, this.Name);
            if (count <= 0)
            {
                this.CreateModel(db);
            }

            //如果需要自动建表就建
            if (this.Automatic)
            {
                var table = new Table(db, this.TableName);
                if (!table.TableExists(this.TableName))
                {
                    this.CreateTable(db);
                }
            }
        }

        private void AddInternalFields()
        {
            LongField("id", "ID", true, null);

            if (this.Versioned)
            {
                LongField("_version", "Version", true, null);
            }
            //DefineField("_creator", "Creation User", "BIGINT", 1);
            //DefineField("_updator", "Last Modifiation User", "BIGINT", 1);
        }

        private void CreateModel(Database db)
        {

            var rowCount = db.Execute(
                "INSERT INTO core_model(name) VALUES(@0);",
                this.Name);

            if (rowCount != 1)
            {
                throw new DataException("Failed to insert record of table core_model");
            }
        }

        private void CreateTable(Database db)
        {
            var table = new Table(db, this.TableName);
            table.CreateTable(this.TableName, this.Label);

            //创建字段
            if (this.Hierarchy)
            {
                //conn.ExecuteNonQuery("");
            }

            foreach (var pair in this.declaredFields)
            {
                if (!pair.Value.IsFunctionField)
                {
                    var field = pair.Value;
                    table.AddColumn(field.Name, field.SqlType);
                }
            }
        }

        #region Field Methods

        protected void DefineField(string name, string label, FieldType type, int size, bool required)
        {
            var field = new Fields.Field(name, "");
            field.Label = label;
            field.Size = size;
            field.Required = required;
            declaredFields.Add(name, field);
        }

        protected void IntegerField(string name, string label, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "integer")
            {
                Label = label,
                Required = required,
                Getter = getter,
            };

            declaredFields.Add(name, field);
        }

        protected void LongField(string name, string label, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "bigint")
            {
                Label = label,
                Required = required,
                Getter = getter,
            };

            declaredFields.Add(name, field);
        }

        protected void BooleanField(string name, string label, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "boolean")
            {
                Label = label,
                Required = required,
                Getter = getter,
            };
            declaredFields.Add(name, field);
        }

        protected void TextField(string name, string label, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "text")
            {
                Label = label,
                Required = required,
                Getter = getter,
            };
            declaredFields.Add(name, field);
        }

        protected void CharsField(string name, string label, int size, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "varchar")
            {
                Label = label,
                Size = size,
                Required = required,
                Getter = getter,

            };
            declaredFields.Add(name, field);
        }

        protected void ManyToOneField(
            string name, string masterModel, string label, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "many2one")
            {
                Label = label,
                Required = required,
                Getter = getter,
                Relation = masterModel,
            };

            declaredFields.Add(name, field);
        }

        protected void OneToManyField(
            string name, string childModel, string relatedField, string label, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "one2many")
            {
                Label = label,
                Required = required,
                Getter = getter,
                Relation = childModel,
                RelatedField = relatedField,
            };

            declaredFields.Add(name, field);
        }

        protected void ManyToManyField(
            string name, string relatedModel,
            string refTableName, string originField, string targetField,
            string label, bool required, FieldGetter getter)
        {
            var field = new Fields.Field(name, "many2many")
            {
                Label = label,
                Required = required,
                Getter = getter,
                Relation = refTableName,
                OriginField = targetField,
                RelatedField = originField,
            };

            declaredFields.Add(name, field);
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

        public static IServiceObject CreateModelInstance(Database db, Type t)
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
        public virtual long Create(ISession session, IDictionary<string, object> values)
        {
            if (!this.CanCreate)
            {
                throw new NotSupportedException();
            }

            //TODO 这里改成定义的列插入，而不是用户提供的列
            //TODO 检测是否含有 id 列

            //获取下一个 SEQ id，这里可能要移到 backend 里，利于跨数据库
            var table = new Table(session.Database, this.TableName);
            var serial = table.NextSerial(this.SequenceName);

            var sql = string.Format(
              "INSERT INTO \"{0}\" (id, _version, {1}) VALUES ({2}, 0, {3});",
              this.TableName,
              values.Keys.ToSqlColumns(),
              serial,
              values.Keys.ToSqlParameters());

            using (var cmd = session.Database.Connection.CreateCommand() as NpgsqlCommand)
            {
                cmd.CommandText = sql;
                foreach (var pair in values)
                {
                    cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                }

                var rows = cmd.ExecuteNonQuery();
                if (rows != 1)
                {
                    Log.ErrorFormat("Failed to insert row, SQL: {0}", sql);
                    throw new DataException();
                }
            }

            return serial;
        }

        [ServiceMethod]
        public virtual void Write(ISession session, long id, IDictionary<string, object> record)
        {
            if (!this.CanWrite)
            {
                throw new NotSupportedException();
            }

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
            if (!this.CanRead)
            {
                throw new NotSupportedException();
            }


            var allFields = new List<string>();
            if (fields == null || fields.Count() == 0)
            {
                allFields.AddRange(this.declaredFields.Keys);
            }
            else
            {
                allFields.Capacity = fields.Count();
                foreach (var f in fields)
                {
                    if (this.declaredFields.ContainsKey(f))
                    {
                        allFields.Add(f);
                    }
                }
            }

            if (!allFields.Contains("id"))
            {
                allFields.Add("id");
            }

            //表里的列，也就是可以直接用 SQL 查的列
            var columnFields = allFields
                .Where(f => !this.declaredFields[f].IsFunctionField);

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
                var f = this.declaredFields[fieldName];

                if (f.IsFunctionField)
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

    }
}
