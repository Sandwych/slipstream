using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace ObjectServer
{
    public class StaticSessionStoreProvider : ISessionStoreProvider
    {
        Dictionary<Guid, Session> sessions = new Dictionary<Guid, Session>();

        #region ISessionStoreProvider 成员

        public Session GetSession(Guid sessionId)
        {
            return this.sessions[sessionId];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetSession(Session session)
        {
            this.sessions[session.SessionId] = session;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveSessionsByUser(string database, long uid)
        {
            var sessions =
                from p in this.sessions
                where p.Value.Database == database && p.Value.UserId == uid
                select p.Value.SessionId;

            foreach (var s in sessions.ToList())
            {
                this.sessions.Remove(s);
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Remove(Guid sessionId)
        {
            if (this.sessions.ContainsKey(sessionId))
            {
                this.sessions.Remove(sessionId);
            }
        }


        #endregion
    }
}
