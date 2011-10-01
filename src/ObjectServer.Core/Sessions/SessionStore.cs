using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    public sealed class SessionStore
    {
        private ISessionStoreProvider provider;

        public void Initialize(string sessionProviderType)
        {
            if (string.IsNullOrEmpty(sessionProviderType))
            {
                throw new ArgumentNullException("sessionProviderType");
            }

            var t = Type.GetType(sessionProviderType);
            this.provider = (ISessionStoreProvider)Activator.CreateInstance(t);
        }

        public Session GetSession(string sessionId)
        {
            Debug.Assert(this.provider != null);
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId");
            }

            return this.provider.GetSession(sessionId);
        }

        public Session GetSession(string db, long userID)
        {
            Debug.Assert(this.provider != null);
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException("db");
            }

            if (userID <= 0)
            {
                throw new ArgumentOutOfRangeException("userID");
            }

            return this.provider.GetSession(db, userID);
        }

        public void PutSession(Session session)
        {
            Debug.Assert(this.provider != null);
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            this.provider.PutSession(session);
        }

        public void RemoveSessionsByUser(string database, long userId)
        {
            Debug.Assert(this.provider != null);
            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentNullException("database");
            }
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException("userId");
            }
            this.provider.TryRemoveSessionsByUser(database, userId);
        }

        public void Remove(string sessionId)
        {
            Debug.Assert(this.provider != null);

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId");
            }

            this.provider.Remove(sessionId);
        }

        public void Pulse(string sessionId)
        {
            Debug.Assert(this.provider != null);

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException("sessionId");
            }

            this.provider.Pulse(sessionId);
        }

    }
}
