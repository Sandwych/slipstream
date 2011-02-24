using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;
using ObjectServer.Utility;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class UserModel : TableModel
    {
        public const string ModelName = "core.user";
        public const string PasswordMask = "************";

        public UserModel()
        {
            this.Name = ModelName;
            this.BitIntegerField("_version", "Version", true, null, null);
            this.CharsField("login", "User Name", 64, true, null, null);
            this.CharsField("password", "Password", 40, true, null, null);
            this.CharsField("salt", "Salt", 64, true, null, null);
            this.BooleanField("admin", "Administrator?", true, null, null);
            this.CharsField("name", "Name", 64, true, null, null);

            this.ManyToManyField(
              "groups", "core.user_group", "uid", "gid", "Groups", false, null, null);
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


        #endregion

    }


    [ServiceObject]
    public sealed class UserGroupModel : TableModel
    {

        public UserGroupModel()
        {
            this.Name = "core.user_group";
            this.TableName = "core_user_group_rel";

            this.ManyToOneField("uid", "core.user", "User", true, null, null);
            this.ManyToOneField("gid", "core.group", "Group", true, null, null);

        }
    }

}
