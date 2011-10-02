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
        public const int IDLength = 16;
        public const string SystemUserName = "system";
        public const long SystemUserId = 0;

        public Session(IDictionary<string, object> record)
        {
            this.ID = (string)record["sid"];
            this.Login = (string)record["login"];
            this.UserID = (long)record["userid"];
            this.LastActivityTime = (DateTime)record["last_activity_time"];
            this.StartTime = (DateTime)record["start_time"];
        }


        public Session(string login, long userID)
            : this()
        {

            if (string.IsNullOrEmpty(login))
            {
                throw new ArgumentNullException("login");
            }

            if (userID <= 0)
            {
                throw new ArgumentOutOfRangeException("userId");
            }

            this.Login = login;
            this.UserID = userID;
        }

        private Session()
        {
            this.UserID = 0;
            this.ID = GenerateSessionId();
            this.StartTime = DateTime.Now;
            this.LastActivityTime = this.StartTime;
        }

        public static Session CreateSystemUserSession()
        {
            var s = new Session();
            s.Login = SystemUserName;
            s.UserID = SystemUserId;
            return s;
        }

        private static string GenerateSessionId()
        {
            var bytes = new byte[IDLength];
            using (var rng = RNGCryptoServiceProvider.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes.ToSha();
        }

        public string ID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string Login { get; set; }
        public long UserID { get; set; }

        public DateTime Deadline
        {
            get
            {
                if (!Environment.Initialized)
                {
                    throw new InvalidOperationException("Framework uninitialized");
                }
                var timeout = new TimeSpan(0, Environment.Configuration.SessionTimeoutMinutes, 0);
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
                return this.UserID <= 0;
            }
        }

        public static Session GetByID(IDBContext db, string sid)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            if (string.IsNullOrEmpty(sid))
            {
                throw new ArgumentNullException("sid");
            }

            var sql = new SqlString("select * from core_session where sid=", Parameter.Placeholder);
            var records = db.QueryAsDictionary(sql, sid);
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

        public static Session GetByUserID(IDBContext db, long uid)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            var sql = new SqlString(
                "select * from core_session where userid=", Parameter.Placeholder);
            var records = db.QueryAsDictionary(sql, uid);
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

        public static void Put(IDBContext db, Session s)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            var sql = SqlString.Parse(
                "insert into core_session(sid, start_time, last_activity_time, userid, login) values(?,?,?,?,?)");
            var n = db.Execute(sql, s.ID, s.StartTime, s.LastActivityTime, s.UserID, s.Login);
            if (n != 1)
            {
                throw new Exceptions.DataException("Failed to put session");
            }
        }

        public static void Remove(IDBContext db, string sid)
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

        public static void Pulse(IDBContext db, string sid)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (string.IsNullOrEmpty(sid))
            {
                throw new ArgumentNullException("sid");
            }

            var sql = SqlString.Parse("update core_session set last_activity_time=? where last_activity_time<? and sid=?");
            var now = DateTime.Now;
            db.Execute(sql, now, now, sid);
        }
    }
}
