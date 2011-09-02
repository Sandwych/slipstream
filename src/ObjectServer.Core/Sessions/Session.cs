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
        public const int IdLength = 16;
        public const string SystemUserName = "system";
        public const long SystemUserId = 0;

        public Session()
        {
            this.UserId = 0;
            this.Id = GenerateSessionId();
            this.StartTime = DateTime.Now;
            this.LastActivityTime = this.StartTime;
        }

        private static string GenerateSessionId()
        {
            var bytes = new byte[IdLength];
            using (var rng = RNGCryptoServiceProvider.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes.ToHex();
        }

        public Session(string dbName, string login, long userId)
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

            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException("userId");
            }

            this.Database = dbName;
            this.Login = login;
            this.UserId = userId;
        }

        public Session(string dbName)
            : this()
        {
            if (string.IsNullOrEmpty(dbName))
            {
                throw new ArgumentNullException("dbName");
            }

            this.Database = dbName;
            this.Login = SystemUserName;
            this.UserId = SystemUserId;
        }

        public string Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string Database { get; set; }
        public string Login { get; set; }
        public long UserId { get; set; }

        public DateTime Deadline
        {
            get
            {
                if (!Environment.Initialized)
                {
                    throw new InvalidOperationException("Framework uninitialized");
                }
                var timeout = Environment.Configuration.SessionTimeout;
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
    }
}
