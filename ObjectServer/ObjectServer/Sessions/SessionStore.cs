using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ObjectServer
{
    public sealed class SessionStore : IGlobalObject
    {
        private ISessionStoreProvider provider;


        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
            var t = Type.GetType(cfg.SessionProvider);
            this.provider = (ISessionStoreProvider)Activator.CreateInstance(t);
        }

        #endregion


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

        public void RemoveSessionsByUser(string database, long uid)
        {
            Debug.Assert(this.provider != null);
            this.provider.RemoveSessionsByUser(database, uid);
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
