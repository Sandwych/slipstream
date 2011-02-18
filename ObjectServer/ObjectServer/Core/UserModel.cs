﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;
using ObjectServer.Utility;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class UserModel : ModelBase
    {

        public UserModel()
        {
            this.Name = "core.user";

            this.CharsField("login", "User Name", 64, true, null);
            this.CharsField("password", "Password", 40, true, null);
            this.BooleanField("admin", "Administrator?", true, null);

            //TODO: 加 Salt 列
            /*
            ManyToManyField(
                "groups", "core.group", "core_user_group_rel", "user_id", "group_id",
                "User Groups", false, null);
             */
        }


        private static IDictionary<string, object> HashPassword(IDictionary<string, object> record)
        {
            IDictionary<string, object> values2 = record;

            if (record.ContainsKey("password"))
            {
                values2 = new Dictionary<string, object>(record);
                var password = (string)values2["password"];
                values2["password"] = password.ToSha1(); //数据库里要保存 hash 而不是明文
            }
            return values2;
        }


        #region Service Methods

        [ServiceMethod]
        public override long Create(ISession session, IDictionary<string, object> values)
        {
            IDictionary<string, object> values2 = HashPassword(values);

            return base.Create(session, values2);
        }

        [ServiceMethod]
        public override void Write(ISession session, long id, IDictionary<string, object> record)
        {
            IDictionary<string, object> values2 = HashPassword(record);


            base.Write(session, id, values2);
        }


        #endregion

    }
}
