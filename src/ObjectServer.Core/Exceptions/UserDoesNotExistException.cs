using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Exceptions
{
    [Serializable]
    public class UserDoesNotExistException : DataException
    {
        public UserDoesNotExistException(string msg, string login)
            : base(msg)
        {
            this.UserName = login;
        }

        public string UserName { get; private set; }
    }
}
