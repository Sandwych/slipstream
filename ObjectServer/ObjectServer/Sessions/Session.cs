using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    [Serializable]
    public sealed class Session
    {
        public Session()
        {
            this.SessionId = Guid.NewGuid();
            this.StartTime = DateTime.Now;
            this.LastActivityTime = this.StartTime;
        }

        public Session(string dbName, string login, long uid)
            : this()
        {
            this.Database = dbName;
            this.Login = login;
            this.UserId = uid;
        }

        public Guid SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string Database { get; set; }
        public string Login { get; set; }
        public long UserId { get; set; }

        public DateTime Deadline
        {
            get
            {
                if (!ObjectServerStarter.Initialized)
                {
                    throw new InvalidOperationException("Framework uninitialized");
                }
                var timeout = ObjectServerStarter.Configuration.SessionTimeout;
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
    }
}
