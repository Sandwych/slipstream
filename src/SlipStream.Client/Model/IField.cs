using System;
using System.Net;

namespace SlipStream.Client.Model
{
    public interface IField
    {
        string Name { get; }
        object Value { get; }
    }
}
