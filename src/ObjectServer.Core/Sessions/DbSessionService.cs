using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Sessions
{
    public class DbSessionService : ISessionService
    {
        #region ISessionService Members

        public Session GetById(string sessionId)
        {
            throw new NotImplementedException();
        }

        public Session GetByUserID(long uid)
        {
            throw new NotImplementedException();
        }

        public void Put(Session s)
        {
            throw new NotImplementedException();
        }

        public void Remove(string sid)
        {
            throw new NotImplementedException();
        }

        public void Pulse(string sid)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
