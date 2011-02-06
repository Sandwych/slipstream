using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;

using ObjectServer.Utility;

namespace ObjectServer
{
    public abstract class ModelBase : IServiceObject
    {
        private Dictionary<string, MethodInfo> serviceMethods =
            new Dictionary<string, MethodInfo>();

        public bool CanCreate { get; protected set; }
        public bool CanRead { get; protected set; }
        public bool CanWrite { get; protected set; }
        public bool CanDelete { get; protected set; }

        public string Name { get; protected set; }
        public string TableName { get; protected set; }

        protected ModelBase()
        {
            this.CanCreate = true;
            this.CanRead = true;
            this.CanWrite = true;
            this.CanDelete = true;

            this.Name = null;
            this.TableName = null;

            this.RegisterAllServiceMethods();
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
        public virtual Dictionary<long, Dictionary<string, object>> Read(ISession session, IEnumerable<string> fields, IEnumerable<long> ids)
        {
            var allFields = new List<string>();
            allFields.Add("id");
            allFields.AddRange(fields);

            var sql = string.Format("select {0} from \"{1}\" where \"id\" in ({2});",
                allFields.ToCommaList(), //TODO: SQL 注入问题
                this.TableName,
                ids.ToCommaList());

            using (var cmd = session.Connection.CreateCommand() as NpgsqlCommand)
            {
                cmd.CommandText = sql;
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    var result = new Dictionary<long, Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        var rec = new Dictionary<string, object>();
                        var id = reader.GetInt64(reader.GetOrdinal("id"));
                        foreach (var field in allFields)
                        {
                            rec.Add(field, reader.GetValue(reader.GetOrdinal(field)));
                        }
                        result.Add(id, rec);
                    }
                    return result;
                }
            }
        }

        [ServiceMethod]
        public virtual void Create(ISession session, Dictionary<string, object> values)
        {
            var sql = string.Format(
                "INSERT INTO \"{0}\" ({1}) VALUES ({2});",
                this.TableName,
                values.Keys.ToSqlColumns(),
                values.Keys.ToSqlParameters());

            using (var cmd = session.Connection.CreateCommand() as NpgsqlCommand)
            {
                cmd.CommandText = sql;
                foreach (var pair in values)
                {
                    cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                }

                cmd.ExecuteNonQuery();
            }
        }

        [ServiceMethod]
        public virtual void Write(ISession session, long id, Dictionary<string, object> values)
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

            using (var cmd = session.Connection.CreateCommand() as NpgsqlCommand)
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

    }
}
