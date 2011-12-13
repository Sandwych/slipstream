using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Data;

namespace ObjectServer
{
    public interface IUserSessionService
    {
        UserSession GetById(string sid);
        UserSession GetByUserId(long userId);
        void Put(UserSession session);
        void Remove(string sid);
        void Pulse(string sid);
    }
}
