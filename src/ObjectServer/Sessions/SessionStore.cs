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

        public Session GetSession(Guid sessionId)
        {
            Debug.Assert(this.provider != null);
            return this.provider.GetSession(sessionId);
        }

        public void PutSession(Session session)
        {
            Debug.Assert(this.provider != null);
            this.provider.PutSession(session);
        }

        public void RemoveSessionsByUser(string database, long userId)
        {
            Debug.Assert(this.provider != null);
            this.provider.RemoveSessionsByUser(database, userId);
        }

        public void Remove(Guid sessionId)
        {
            Debug.Assert(this.provider != null);
            this.provider.Remove(sessionId);
        }

        public void Pulse(Guid sessionId)
        {
            Debug.Assert(this.provider != null);
            this.provider.Pulse(sessionId);
        }

    }
}
