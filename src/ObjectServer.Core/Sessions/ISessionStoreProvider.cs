using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface ISessionStoreProvider
    {
        Session GetSession(Guid sessionId);

        void PutSession(Session session);

        void RemoveSessionsByUser(string database, long uid);

        void Remove(Guid sessionId);

        void Pulse(Guid sessionId);

    }
}
