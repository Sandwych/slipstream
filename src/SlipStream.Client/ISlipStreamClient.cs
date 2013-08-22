using System;
using System.Net;


namespace SlipStream.Client
{
    public interface ISlipStreamClient : IRootService
    {
        string SessionToken { get; }
        string LoggedUserName { get; }
        string LoggedDatabase { get; }
        Uri Uri { get; }
        Uri ServerAddress { get; }

        bool Logged { get; }
    }
}
