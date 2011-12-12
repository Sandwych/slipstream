using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using NHibernate.SqlCommand;

using ObjectServer.Utility;
using ObjectServer.Data;

namespace ObjectServer
{
    [Serializable]
    public sealed class Session
    {
        private static readonly SqlString SelectByIdSql =
            new SqlString(@"select * from ""core_session"" where ""sid"" = ", Parameter.Placeholder);
        private static readonly SqlString SelectByUserIdSql =
            new SqlString(@"select * from ""core_session"" where ""userid"" = ", Parameter.Placeholder);
        private static readonly SqlString UpdateLastActivityTimeSql = SqlString.Parse(
            @"update ""core_session"" set ""last_activity_time"" = ? where ""last_activity_time"" < ? and ""sid"" = ?");

        public const int IdLength = 16;
        public const string SystemUserName = "system";
        public const long SystemUserId = 0;

        public Session(IDictionary<string, object> record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            this.Id = (string)record["sid"];
            this.Login = (string)record["login"];
            this.UserId = (long)record["userid"];
            this.LastActivityTime = (DateTime)record["last_activity_time"];
            this.StartTime = (DateTime)record["start_time"];
        }


        public Session(string login, long userId)
            : this()
        {

            if (string.IsNullOrEmpty(login))
            {
                throw new ArgumentNullException("login");
            }

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException("userId");
            }

            this.Login = login;
            this.UserId = userId;
        }

        private Session()
        {
            this.UserId = 0;
            this.Id = GenerateSessionId();
            this.StartTime = DateTime.Now;
            this.LastActivityTime = this.StartTime;
        }

        public static Session CreateSystemUserSession()
        {
            var s = new Session();
            s.Login = SystemUserName;
            s.UserId = SystemUserId;
            return s;
        }

        private static string GenerateSessionId()
        {
            var bytes = new byte[IdLength];
            using (var rng = RNGCryptoServiceProvider.Create())
            {
                rng.GetBytes(bytes);
            }
            var hash = bytes.ToSha();
            return Convert.ToBase64String(hash);
        }

        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string Login { get; set; }
        public long UserId { get; set; }

        public DateTime Deadline
        {
            get
            {
                if (!SlipstreamEnvironment.Initialized)
                {
                    throw new InvalidOperationException("Framework uninitialized");
                }
                var timeout = new TimeSpan(0, SlipstreamEnvironment.Configuration.SessionTimeoutMinutes, 0);
                return this.LastActivityTime + timeout;
            }
        }

        public bool IsActive
        {
            get
            {
                return DateTime.Now <= this.Deadline;
            }
        }

        public bool IsSystemUser
        {
            get
            {
                return this.UserId <= 0;
            }
        }

        public static Session GetById(IDataContext db, string sid)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(sid))
            {
                throw new ArgumentNullException("sid");
            }

            var records = db.QueryAsDictionary(SelectByIdSql, sid);
            if (records.Length > 1)
            {
                throw new Exceptions.DataException("More than one session id in table [core_session]!");
            }
            else if (records.Length < 1)
            {
                return null;
            }
            else
            {
                var record = records[0];
                return new Session(record);
            }
        }

        public static Session GetByUserId(IDataContext db, long userId)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            var records = db.QueryAsDictionary(SelectByUserIdSql, userId);
            if (records.Length > 1)
            {
                throw new Exceptions.DataException("More than one user id in table [core_session]!");
            }
            else if (records.Length < 1)
            {
                return null;
            }
            else
            {
                var record = records[0];
                return new Session(record);
            }
        }

        public static void Put(IDataContext db, Session session)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            var sql = SqlString.Parse(
                @"insert into ""core_session""(""sid"", ""start_time"", ""last_activity_time"", ""userid"", ""login"") values(?,?,?,?,?)");
            var n = db.Execute(sql, session.Id, session.StartTime, session.LastActivityTime, session.UserId, session.Login);
            if (n != 1)
            {
                throw new Exceptions.DataException("Failed to put session");
            }
        }

        public static void Remove(IDataContext db, string sid)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(sid))
            {
                throw new ArgumentNullException("sid");
            }

            var sql = SqlString.Parse("delete from core_session where sid=?");
            var n = db.Execute(sql, sid);
            if (n != 1)
            {
                throw new Exceptions.DataException("Failed to remove session");
            }
        }

        public static void Pulse(IDataContext db, string sid)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(sid))
            {
                throw new ArgumentNullException("sid");
            }

            var s = GetById(db, sid);
            if (s.IsActive)
            {
                var now = DateTime.Now;
                db.Execute(UpdateLastActivityTimeSql, now, now, sid);
            }
            else
            {
                Remove(db, sid);
            }
        }

        /// <summary>
        /// 清理所有无效的 Sessions
        /// </summary>
        public static void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
