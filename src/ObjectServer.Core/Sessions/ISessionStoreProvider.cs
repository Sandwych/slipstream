using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface ISessionStoreProvider
    {
        Session GetSession(string sessionId);

        void PutSession(Session session);

        void RemoveSessionsByUser(string database, long uid);

        void Remove(string sessionId);

        void Pulse(string sessionId);

    }
}
