using System;
using System.Net;

namespace ObjectServer.Client.Model
{
    public interface IField
    {
        string Name { get; }
        object Value { get; }
    }
}
