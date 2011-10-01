using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface ISessionStoreProvider
    {
        Session GetSession(string sessionId);

        Session GetSession(string db, long userID);

        void PutSession(Session session);

        bool TryRemoveSessionsByUser(string database, long userID);

        void Remove(string sessionId);

        void Pulse(string sessionId);

    }
}
