using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace ObjectServer.Sessions
{
    [Export("memory", typeof(IUserSessionStore))]
    public class MemoryUserSessionStore : IUserSessionStore
    {
        #region IUserSessionStore 成员

        private static readonly HashSet<UserSession> s_sessions =
            new HashSet<UserSession>();

        public UserSession GetByToken(string token)
        {
            return s_sessions.SingleOrDefault(s => s.Token == token);
        }

        public UserSession GetByUserId(long userId)
        {
            return s_sessions.SingleOrDefault(s => s.UserId == userId);
        }

        public void Put(UserSession session)
        {
            if (this.GetByToken(session.Token) != null)
            {
                this.Remove(session.Token);
            }

            lock (s_sessions)
            {
                s_sessions.Add(session);
            }
        }

        public void Remove(string token)
        {
            lock (s_sessions)
            {
                s_sessions.RemoveWhere(s => s.Token == token);
            }
        }

        public void Pulse(string token)
        {
            var s = this.GetByToken(token);
            if (s.IsActive)
            {
                var now = DateTime.Now;
                s.LastActivityTime = now;
            }
            else
            {
                this.Remove(token);
            }
        }

        #endregion
    }
}
