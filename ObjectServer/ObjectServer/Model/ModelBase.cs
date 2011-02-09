using System;
using System.Collections.Generic;
using System.Collections;
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

namespace ObjectServer.Model
{
    public abstract class ModelBase : IServiceObject
    {
        protected static readonly ILog Log = LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, IFieldInfo> declaredFields =
            new Dictionary<string, IFieldInfo>();

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

            this.RegisterAllServiceMethods();
        }

        /// <summary>
        /// 初始化数据库信息
        /// </summary>
        public void Initialize(Database db)
        {
            //检测此模块是否存在于数据库 core_model 表
            var sql = string.Format(
                "SELECT DISTINCT COUNT(id) FROM core_model WHERE name='{0}'",
                this.Name);
            var count = (long)db.Connection.ExecuteScalar(sql);
            if (count <= 0)
            {
                this.CreateModel(db.Connection);
            }

            //如果需要自动建表就建
            if (this.Automatic)
            {
                var table = new Table();
                if (!table.TableExists(db.Connection, this.TableName))
                {
                    this.CreateTable(db.Connection);
                }
            }
        }

        private void CreateModel(IDbConnection conn)
        {

            var sql = string.Format(
                "INSERT INTO core_model(name) VALUES('{0}');",
                this.Name);
            conn.ExecuteNonQuery(sql);

            //TODO 加入字段信息
        }

        private void CreateTable(IDbConnection conn)
        {
            var table = new Table();
            table.CreateTable(conn, this.TableName, this.Label);

            //创建字段
            if (this.Hierarchy)
            {
                conn.ExecuteNonQuery("");
            }

            foreach (var pair in this.declaredFields)
            {
                var field = pair.Value;
                table.AddColumn(conn, this.tableName, field.Name, field.SqlType);
            }
        }

        protected void DefineField(string name, string label, string type, int size)
        {
            var field = new Fields.SimpleField();
            field.Name = name;
            field.Label = label;
            field.Type = type;
            field.Size = size;
            declaredFields.Add(name, field);
        }

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
            var types = a.GetTypes();
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

        [ServiceMethod]
        public virtual long[] Search(ISession session, string exp)
        {
            throw new NotImplementedException();
        }

        [ServiceMethod]
        public virtual Hashtable[] Read(ISession session, IEnumerable<string> fields, IEnumerable<long> ids)
        {
            var allFields = new List<string>();
            allFields.Add("id");
            allFields.AddRange(fields);

            var sql = string.Format("select {0} from \"{1}\" where \"id\" in ({2});",
                allFields.ToCommaList(), //TODO: SQL 注入问题
                this.TableName,
                ids.ToCommaList());

            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }

            using (var cmd = session.Connection.CreateCommand() as NpgsqlCommand)
            {
                cmd.CommandText = sql;
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    var result = new List<Hashtable>();
                    while (reader.Read())
                    {
                        var rec = new Hashtable(reader.FieldCount);
                        foreach (var field in allFields)
                        {
                            rec.Add(field, reader.GetValue(reader.GetOrdinal(field)));
                        }
                        result.Add(rec);
                    }
                    return result.ToArray();
                }
            }
        }

        [ServiceMethod]
        public virtual long Create(ISession session, IDictionary<string, object> values)
        {
            //TODO 检测是否含有 id 列

            //获取下一个 SEQ id，这里可能要移到 backend 里，利于跨数据库
            var serial = NextSerial(session.Connection);

            var sql = string.Format(
              "INSERT INTO \"{0}\" (id, {1}) VALUES ({2}, {3});",
              this.TableName,
              values.Keys.ToSqlColumns(),
              serial,
              values.Keys.ToSqlParameters());

            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }

            using (var cmd = session.Connection.CreateCommand() as NpgsqlCommand)
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

        private long NextSerial(IDbConnection conn)
        {
            var seqSql = string.Format("SELECT nextval('{0}');",
                this.SequenceName);

            if (Log.IsDebugEnabled)
            {
                Log.Debug(seqSql);
            }
            var serial = (long)conn.ExecuteScalar(seqSql);
            return serial;
        }

        [ServiceMethod]
        public virtual void Write(ISession session, long id, IDictionary<string, object> values)
        {
            using (var cmd = session.Connection.CreateCommand() as NpgsqlCommand)
            {

            }
        }


        [ServiceMethod]
        public virtual void Delete(ISession session, IEnumerable<long> ids)
        {
            var sql = string.Format(
                "DELETE FROM \"{0}\" ({1}) WHERE \"id\" in ({2});",
                this.TableName, ids.ToCommaList());

            if (Log.IsDebugEnabled)
            {
                Log.Debug(sql);
            }

            using (var cmd = session.Connection.CreateCommand() as NpgsqlCommand)
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

    }
}
