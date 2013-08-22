using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Data;

namespace SlipStream
{
    public interface IUserSessionStore
    {
        UserSession GetByToken(string token);
        UserSession GetByUserId(long userId);
        void Put(UserSession session);
        void Remove(string token);
        void Pulse(string token);
    }
}
