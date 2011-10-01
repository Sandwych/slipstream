using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace ObjectServer
{
    internal class StaticSessionStoreProvider : ISessionStoreProvider
    {
        private int SweepInterval = 100;

        private long sweepCounter = 0;
        Dictionary<string, Session> sessions = new Dictionary<string, Session>();

        #region ISessionStoreProvider 成员

        public Session GetSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId");
            }

            Session s;
            if (this.sessions.TryGetValue(sessionId, out s))
            {
                return s;
            }
            else
            {
                return null;
            }
        }

        public Session GetSession(string db, long userID)
        {
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException("sessionId");
            }

            if (userID <= 0)
            {
                throw new ArgumentOutOfRangeException("userID");
            }

            var sessions =
                from p in this.sessions
                where p.Value.DBName == db && p.Value.UserID == userID
                select p.Value;
            return sessions.FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PutSession(Session session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            sweepCounter++;
            if (sweepCounter % SweepInterval == 0)
            {
                this.Sweep();
            }

            this.sessions[session.ID] = session;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TryRemoveSessionsByUser(string database, long uid)
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
                where p.Value.DBName == database && p.Value.UserID == uid
                select p.Value.ID;
            var sid = sessions.SingleOrDefault();
            if (sid != null)
            {
                this.sessions.Remove(sid);
                return true;
            }
            else
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Remove(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentOutOfRangeException("sessionId");
            }

            if (this.sessions.ContainsKey(sessionId))
            {
                this.sessions.Remove(sessionId);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Pulse(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentOutOfRangeException("sessionId");
            }

            var sess = this.sessions[sessionId];
            sess.LastActivityTime = DateTime.Now;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Sweep()
        {
            var sessionIDs = this.sessions.Keys.ToArray();
            foreach (var sid in sessionIDs)
            {
                var session = this.sessions[sid];
                if (!session.IsActive)
                {
                    this.sessions.Remove(sid);
                }
            }
        }



        #endregion
    }
}
