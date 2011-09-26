using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using ObjectServer.Utility;

namespace ObjectServer
{
    [Serializable]
    public sealed class Session
    {
        public const int IDLength = 16;
        public const string SystemUserName = "system";
        public const long SystemUserId = 0;


        public Session(string dbName, string login, long userID)
            : this()
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            if (string.IsNullOrEmpty(login))
            {
                throw new ArgumentNullException("login");
            }

            if (userID <= 0)
            {
                throw new ArgumentOutOfRangeException("userId");
            }

            this.DBName = dbName;
            this.Login = login;
            this.UserID = userID;
        }

        public Session(string dbName)
            : this()
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            this.DBName = dbName;
            this.Login = SystemUserName;
            this.UserID = SystemUserId;
        }

        private Session()
        {
            this.UserID = 0;
            this.ID = GenerateSessionId();
            this.StartTime = DateTime.Now;
            this.LastActivityTime = this.StartTime;
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
        public string DBName { get; set; }
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
    }
}
