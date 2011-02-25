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
            : base("core.session")
        {
            Fields.Chars("session_id").SetLabel("Session Id").SetRequired().SetSize(36);
            Fields.DateTime("start_time").SetLabel("Start Time").SetRequired();
            Fields.DateTime("last_activity_time").SetLabel("Last Activity Time").SetRequired();
            Fields.Chars("database").SetLabel("Database Name").SetRequired().SetSize(64);
            Fields.Chars("login").SetLabel("User Name").SetRequired().SetSize(64);
            Fields.BigInteger("user_id").SetLabel("User ID").SetRequired();
        }


    }
}
