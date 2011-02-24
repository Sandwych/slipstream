using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject]
    public class SessionModel : TableModel
    {
        /*
         *   [Serializable]
    public sealed class Session
    {
        public Session()
        {
            this.SessionId = Guid.NewGuid();
            this.StartTime = DateTime.Now;
            this.LastActivityTime = this.StartTime;
        }

        public Guid SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public string Login { get; set; }
        public long UserId { get; set; }
    }
         */

        public SessionModel()
            : base()
        {
            this.Name = "core.session";


            this.CharsField("session_id", "Session Id", 36, true, null, null);
            this.DateTimeField("start_time", "Start Time", true, null, null);
            this.DateTimeField("last_activity_time", "Last Activity Time", true, null, null);
            this.CharsField("database", "Database Name", 64, true, null, null);
            this.CharsField("login", "User Name", 64, true, null, null);
            this.BitIntegerField("user_id", "User ID", true, null, null); //这里不关联
        }


    }
}
