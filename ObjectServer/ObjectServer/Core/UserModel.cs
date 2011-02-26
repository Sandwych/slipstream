using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Model;
using ObjectServer.Utility;
using ObjectServer.Backend;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class UserModel : TableModel
    {
        public const string ModelName = "core.user";
        public const string PasswordMask = "************";
        public const string RootUserName = "root";

        public UserModel()
            : base(ModelName)
        {
            Fields.BigInteger("_version").SetLabel("Version").SetRequired();
            Fields.Chars("login").SetLabel("User Name").SetSize(64).SetRequired();
            Fields.Chars("password").SetLabel("Password").SetSize(40).SetRequired();
            Fields.Chars("salt").SetLabel("Salt").SetSize(64).SetRequired();
            Fields.Boolean("admin").SetLabel("Administrator?").SetRequired();
            Fields.Chars("name").SetLabel("Name").SetRequired().SetSize(64);

            Fields.ManyToMany("groups", "core.user_group", "uid", "gid").SetLabel("Groups");
        }

        public override void Initialize(IDatabaseContext db, IObjectPool pool)
        {
            base.Initialize(db, pool);

            using (var callingCtx = new CallingContext(db))
            {
                //检测是否有 root 用户
                var domain = new object[][] { new object[] { "login", "=", "root" } };
                var users = this.Search(callingCtx, domain, 0, 0);
                if (users.Length <= 0)
                {
                    this.CreateRootUser(callingCtx);
                }
            }
        }

        private void CreateRootUser(CallingContext callingCtx)
        {
            //创建 root 用户
            var rootPassword = ObjectServerStarter.Configuration.RootPassword;
            var user = new Dictionary<string, object>()
                    {
                        { "name", "Root User" },
                        { "login", "root" },
                        { "password", rootPassword } ,
                        { "admin", true },
                    };

            this.Create(callingCtx, user);
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
                values2["password"] = (password + salt).ToSha1(); //数据库里要保存 hash 而不是明文
            }
            return values2;
        }

        private static string GenerateSalt()
        {
            var r = new Random();
            var bytes = new byte[16];
            r.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static bool IsValidPassword(string hashedPassword, string salt, string password)
        {
            return hashedPassword == (password + salt).ToSha1();
        }

        #region Service Methods

        [ServiceMethod]
        public override long Create(ICallingContext callingContext, IDictionary<string, object> values)
        {
            IDictionary<string, object> values2 = HashPassword(values);

            return base.Create(callingContext, values2);
        }

        [ServiceMethod]
        public override void Write(ICallingContext callingContext, object id, IDictionary<string, object> record)
        {
            IDictionary<string, object> values2 = HashPassword(record);


            base.Write(callingContext, id, values2);
        }

        [ServiceMethod]
        public override Dictionary<string, object>[] Read(ICallingContext callingContext, object[] ids, object[] fields)
        {
            var records = base.Read(callingContext, ids, fields);

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

        [ServiceMethod]
        public string LogOn(ICallingContext callingContext,
            string database, string login, string password)
        {
            var domain = new object[][] { new object[] { "login", "=", login } };

            var users = base.Search(callingContext, domain, 0, 0);
            if (users.Length != 1)
            {
                throw new UserDoesNotExistException("Cannot found user: " + login, login);
            }

            var user = base.Read(callingContext,
                new object[] { users[0] },
                new object[] { "password", "salt" })[0];

            var hashedPassword = (string)user["password"];
            var salt = (string)user["salt"];

            string result = null;
            if (IsValidPassword(hashedPassword, salt, password))
            {
                var session = this.CreateSession(database, login, user);
                result = session.SessionId.ToString();
            }
            else
            {
                var uid = (long)user["id"];
                ObjectServerStarter.SessionStore.RemoveSessionsByUser(database, uid);
            }

            return result;
        }


        [ServiceMethod]
        public void LogOut(ICallingContext callingContext, string sessionId)
        {
            var sessGuid = new Guid(sessionId);
            ObjectServerStarter.SessionStore.Remove(sessGuid);
        }

        #endregion

        private Session CreateSession(string dbName, string login, IDictionary<string, object> userFields)
        {
            Debug.Assert(userFields.ContainsKey("password"));

            var uid = (long)userFields["id"];
            var session = new Session(dbName, login, uid);

            var sessStore = ObjectServerStarter.SessionStore;
            sessStore.RemoveSessionsByUser(dbName, uid);
            sessStore.PutSession(session);
            return session;
        }
    }



    [ServiceObject]
    public sealed class UserGroupModel : TableModel
    {

        public UserGroupModel()
            : base("core.user_group")
        {
            this.TableName = "core_user_group_rel";

            Fields.ManyToOne("uid", "core.user").SetLabel("User").SetRequired();
            Fields.ManyToOne("gid", "core.group").SetLabel("Group").SetRequired();

        }
    }

}
