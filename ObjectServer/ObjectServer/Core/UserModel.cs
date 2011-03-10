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

    [Resource]
    public sealed class UserModel : TableModel
    {
        public const string ModelName = "core.user";
        public const string PasswordMask = "************";
        public const string RootUserName = "root";

        public UserModel()
            : base(ModelName)
        {
            Fields.Version().SetLabel("Version");
            Fields.Chars("login").SetLabel("User Name").SetSize(64).Required();
            Fields.Chars("password").SetLabel("Password").SetSize(40).Required();
            Fields.Chars("salt").SetLabel("Salt").SetSize(64).Required();
            Fields.Boolean("admin").SetLabel("Administrator?").Required();
            Fields.Chars("name").SetLabel("Name").Required().SetSize(64);

            Fields.ManyToMany("groups", "core.user_group", "uid", "gid").SetLabel("Groups");
        }

        public override void Load(IDatabase db)
        {
            base.Load(db);

            using (var ctx = new ContextScope(db))
            {
                //检测是否有 root 用户
                var domain = new object[][] { new object[] { "login", "=", "root" } };
                var users = this.SearchInternal(ctx, domain, 0, 0);
                if (users.Length <= 0)
                {
                    this.CreateRootUser(ctx);
                }
            }
        }

        private void CreateRootUser(ContextScope ctx)
        {
            //创建 root 用户
            var rootPassword = ObjectServerStarter.Configuration.RootPassword;
            var user = new Dictionary<string, object>()
                    {
                        { "name", "Root User" },
                        { "login", "root" },
                        { "password", rootPassword } ,
                        { "admin", true },
                        { CreatedUserField, DBNull.Value }, //一定要覆盖掉默认设置，因为此时系统里还没有用户，取 Session 里的 UserId 是无意义的
                    };

            this.CreateInternal(ctx, user);
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


        private static bool IsPasswordMatched(string hashedPassword, string salt, string password)
        {
            return hashedPassword == (password + salt).ToSha1();
        }


        public override long CreateInternal(IContext ctx, IDictionary<string, object> values)
        {
            IDictionary<string, object> values2 = HashPassword(values);

            return base.CreateInternal(ctx, values2);
        }
       

        public override void WriteInternal(IContext ctx, long id, IDictionary<string, object> record)
        {
            IDictionary<string, object> values2 = HashPassword(record);


            base.WriteInternal(ctx, id, values2);
        }


        public override Dictionary<string, object>[] ReadInternal(
            IContext ctx, object[] ids, IEnumerable<string> fields)
        {
            var records = base.ReadInternal(ctx, ids, fields);

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


        public Session LogOn(IContext ctx,
            string database, string login, string password)
        {
            var domain = new object[][] { new object[] { "login", "=", login } };

            var users = base.SearchInternal(ctx, domain, 0, 0);
            if (users.Length != 1)
            {
                throw new UserDoesNotExistException("Cannot found user: " + login, login);
            }

            var user = base.ReadInternal(ctx,
                new object[] { users[0] },
                new string[] { "password", "salt" })[0];

            var hashedPassword = (string)user["password"];
            var salt = (string)user["salt"];

            Session result = null;
            if (IsPasswordMatched(hashedPassword, salt, password))
            {
                var session = this.CreateSession(database, login, user);
                result = session;
            }
            else
            {
                var uid = (long)user["id"];
                ObjectServerStarter.SessionStore.RemoveSessionsByUser(database, uid);
            }

            return result;
        }


        public void LogOut(IContext ctx, string sessionId)
        {
            var sessGuid = new Guid(sessionId);
            ObjectServerStarter.SessionStore.Remove(sessGuid);
        }


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



    [Resource]
    public sealed class UserGroupModel : TableModel
    {

        public UserGroupModel()
            : base("core.user_group")
        {
            this.TableName = "core_user_group_rel";

            Fields.ManyToOne("uid", "core.user").SetLabel("User").Required();
            Fields.ManyToOne("gid", "core.group").SetLabel("Group").Required();

        }
    }

}
