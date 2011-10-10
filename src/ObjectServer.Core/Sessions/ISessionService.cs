using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Sessions
{
    public interface ISessionService
    {
        Session GetById(string sessionId);
        Session GetByUserID(long uid);
        void Put(Session s);
        void Remove(string sid);
        void Pulse(string sid);
        void Clear();
    }
}
