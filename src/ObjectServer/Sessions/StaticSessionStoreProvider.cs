using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace ObjectServer
{
    internal class StaticSessionStoreProvider : ISessionStoreProvider
    {
        Dictionary<Guid, Session> sessions = new Dictionary<Guid, Session>();

        #region ISessionStoreProvider 成员

        public Session GetSession(Guid sessionId)
        {
            return this.sessions[sessionId];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PutSession(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            this.Sweep();
            this.sessions[session.Id] = session;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveSessionsByUser(string database, long uid)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            if (uid <= 0)
            {
                throw new ArgumentOutOfRangeException("uid");
            }

            var sessions =
                from p in this.sessions
                where p.Value.Database == database && p.Value.UserId == uid
                select p.Value.Id;

            foreach (var s in sessions.ToList())
            {
                this.sessions.Remove(s);
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Remove(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException("sessionId");
            }

            if (this.sessions.ContainsKey(sessionId))
            {
                this.sessions.Remove(sessionId);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Pulse(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException("sessionId");
            }

            var sess = this.sessions[sessionId];
            sess.LastActivityTime = DateTime.Now;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Sweep()
        {
            var keys = this.sessions.Keys.ToArray();
            foreach (var k in keys)
            {
                var session = this.sessions[k];
                if (!session.IsActive)
                {
                    this.sessions.Remove(k);
                }
            }
        }



        #endregion
    }
}
