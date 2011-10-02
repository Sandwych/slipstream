using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;

using NHibernate.SqlCommand;

using ObjectServer.Model;
using ObjectServer.Utility;
using ObjectServer.Data;
using ObjectServer.Exceptions;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class UserModel : AbstractTableModel
    {
        public const string ModelName = "core.user";
        public const string PasswordMask = "************";
        public const string RootUserName = "root";
        public const int SaltLength = 16;

        public UserModel()
            : base(ModelName)
        {
            Fields.Version().SetLabel("Version");
            Fields.Chars("login").SetLabel("User Name").SetSize(64).Required().Unique();
            Fields.Chars("password").SetLabel("Password").SetSize(64).Required();
            Fields.Chars("salt").SetLabel("Salt").SetSize(64).Required();
            Fields.Boolean("admin").SetLabel("Administrator?").Required().SetDefaultValueGetter(r => false);
            Fields.Chars("name").SetLabel("Name").Required().SetSize(64);
            Fields.ManyToMany("roles", "core.user_role", "user", "role").SetLabel("Roles");
            Fields.ManyToOne("organization", "core.organization").SetLabel("Organization");
        }

        public override void Initialize(IDBContext db, bool update)
        {
            base.Initialize(db, update);

            //检测是否有 root 用户
            var isRootUserExisted = UserExists(db, RootUserName);
            if (update && isRootUserExisted)
            {
                this.CreateRootUser(db);
            }
        }

        private static bool UserExists(IDBContext db, string login)
        {
            var sql = new SqlString(
                "select count(*) from ",
                DataProvider.Dialect.QuoteForTableName("core_user"),
                "where ",
                DataProvider.Dialect.QuoteForColumnName("login"), "=", Parameter.Placeholder);

            var rowCount = db.QueryValue(sql, login);
            var isRootUserExisted = rowCount.IsNull() || (long)rowCount <= 0;
            return isRootUserExisted;
        }

        private void CreateRootUser(IDBContext conn)
        {
            Debug.Assert(conn != null);

            LoggerProvider.BizLogger.Info("Creating the Root user...");

            /*
                    insert into core_user(_version, ""name"", ""login"", ""password"", ""admin"", _created_time, salt)
                    values(?,?,?,?,?,?,?)
             */

            var sql = new SqlString(
                "insert into ",
                DataProvider.Dialect.QuoteForTableName("core_user"),
                "(",
                DataProvider.Dialect.QuoteForColumnName(VersionFieldName), ",",
                DataProvider.Dialect.QuoteForColumnName("name"), ",",
                DataProvider.Dialect.QuoteForColumnName("login"), ",",
                DataProvider.Dialect.QuoteForColumnName("password"), ",",
                DataProvider.Dialect.QuoteForColumnName("admin"), ",",
                DataProvider.Dialect.QuoteForColumnName(CreatedTimeFieldName), ",",
                DataProvider.Dialect.QuoteForColumnName("salt"),
                ") values(",
                Parameter.Placeholder, ",",
                Parameter.Placeholder, ",",
                Parameter.Placeholder, ",",
                Parameter.Placeholder, ",",
                Parameter.Placeholder, ",",
                Parameter.Placeholder, ",",
                Parameter.Placeholder, ")");

            //创建 root 用户
            var rootPassword = Environment.Configuration.ServerPassword;
            var user = new Dictionary<string, object>()
                    {
                        { "name", "Root User" },
                        { "login", RootUserName },
                        { "password", rootPassword } ,
                        { "admin", true },
                        { CreatedUserFieldName, DBNull.Value }, //一定要覆盖掉默认设置，因为此时系统里还没有用户，取 Session 里的 UserId 是无意义的
                        { CreatedTimeFieldName, DateTime.Now },
                        { VersionFieldName, 1 },
                    };
            var row = HashPassword(user);

            conn.Execute(
                sql, row[VersionFieldName], row["name"], row["login"], row["password"],
                row["admin"], row["_created_time"], row["salt"]);

            LoggerProvider.BizLogger.Info("Root user has been created.");
        }


        private static IDictionary<string, object> HashPassword(IDictionary<string, object> record)
        {
            IDictionary<string, object> values2 = record;

            if (record.ContainsKey("password"))
            {
                values2 = new Dictionary<string, object>(record);
                var salt = GenerateSalt();
                values2["salt"] = salt;
                var password = (string)values2["password"];
                values2["password"] = (password + salt).ToSha(); //数据库里要保存 hash 而不是明文
            }
            return values2;
        }


        private static string GenerateSalt()
        {
            var bytes = new byte[SaltLength];
            using (var rng = RNGCryptoServiceProvider.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes.ToHex();
        }


        private static bool IsPasswordMatched(string hashedPassword, string salt, string password)
        {
            Debug.Assert(!string.IsNullOrEmpty(hashedPassword));
            Debug.Assert(!string.IsNullOrEmpty(salt));
            Debug.Assert(!string.IsNullOrEmpty(password));

            var newHash = (password + salt).ToSha();
            return hashedPassword == newHash;
        }


        public override long CreateInternal(IServiceContext scope, IDictionary<string, object> values)
        {
            IDictionary<string, object> values2 = HashPassword(values);
            return base.CreateInternal(scope, values2);
        }


        public override void WriteInternal(IServiceContext scope, long id, IDictionary<string, object> record)
        {
            //更新用户记录业务是不能修改密码与 Salt 的
            var values2 = new Dictionary<string, object>(record);
            if (record.ContainsKey("password"))
            {
                values2.Remove("password");
            }
            if (record.ContainsKey("salt"))
            {
                values2.Remove("salt");
            }

            base.WriteInternal(scope, id, values2);

            //TODO 通知 Session 缓存
        }


        public override Dictionary<string, object>[] ReadInternal(
            IServiceContext scope, long[] ids, string[] fields)
        {
            var records = base.ReadInternal(scope, ids, fields);

            //"salt" "password" 是敏感字段，不要让客户端获取
            foreach (var record in records)
            {
                if (record.ContainsKey("salt"))
                {
                    record["salt"] = null;
                }

                if (record.ContainsKey("password"))
                {
                    record["password"] = PasswordMask;
                }
            }


            return records;
        }


        public Session LogOn(IServiceContext scope,
            string database, string login, string password)
        {
            var constraints = new object[][] { new object[] { "login", "=", login } };

            var users = base.SearchInternal(scope, constraints);
            if (users.Length != 1)
            {
                throw new UserDoesNotExistException("Cannot found user: " + login, login);
            }

            var user = base.ReadInternal(scope,
                new long[] { users[0] },
                new string[] { "password", "salt" })[0];

            var hashedPassword = (string)user["password"];
            var salt = (string)user["salt"];

            Session result = null;
            if (IsPasswordMatched(hashedPassword, salt, password))
            {
                var session = this.FetchOrCreateSession(scope, database, login, user);
                result = session;

                LoggerProvider.EnvironmentLogger.Info(() =>
                    String.Format("User[{0}.{1}] logged.", scope.DBContext.DatabaseName, login));
            }
            else
            {
                LoggerProvider.EnvironmentLogger.Warn(() =>
                    String.Format("Failed to log on user: [{0}.{1}]", scope.DBContext.DatabaseName, login));

                var uid = (long)user[IDFieldName];
            }

            return result;
        }


        public void LogOff(IServiceContext scope, string sessionId)
        {
            var session = Session.GetByID(scope.DBContext, sessionId);

            if (session != null)
            {
                Session.Remove(scope.DBContext, sessionId);

                LoggerProvider.EnvironmentLogger.Info(() =>
                    String.Format("User[{0}.{1}] logged out.", scope.DBContext.DatabaseName, session.Login));
            }
            else
            {
                LoggerProvider.EnvironmentLogger.Warn(() =>
                    String.Format("One connection try to log off a unexisted session: {0}", sessionId));
            }
        }

        [ServiceMethod("ChangePassword")]
        public static void ChangePassword(
            IModel model, IServiceContext ctx, string newPassword)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            if (String.IsNullOrEmpty(newPassword))
            {
                throw new ArgumentNullException("newPassword");
            }

            var record = new Dictionary<string, object>()
            {
                { "password", newPassword },
            };
            HashPassword(record);
            model.WriteInternal(ctx, ctx.Session.UserID, record);
        }

        public static Dictionary<string, object>[] GetAllModelAccessEntries(long userId)
        {
            throw new NotImplementedException();
        }


        private Session FetchOrCreateSession(
            IServiceContext ctx, string dbName, string login, IDictionary<string, object> userFields)
        {
            Debug.Assert(ctx != null);
            Debug.Assert(userFields.ContainsKey("password"));

            var uid = (long)userFields[IDFieldName];

            var oldSession = Session.GetByUserID(ctx.DBContext, uid);

            if (oldSession == null)
            {
                var newSession = new Session(login, uid);
                Session.Put(ctx.DBContext, newSession);
                return newSession;
            }
            else
            {
                return oldSession;
            }
        }

        /// <summary>
        /// 凡是 init.sql 里建立的表都不应该返回引用对象
        /// </summary>
        /// <returns></returns>
        public override string[] GetReferencedObjects()
        {
            return new string[] { };
        }
    }





}
