using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace ObjectServer
{
    public class RemotingServer
    {


        [STAThread]
        static void Main(string[] args)
        {
            TcpChannel chan = new TcpChannel(5000);
            ChannelServices.RegisterChannel(chan, false);
            RemotingConfiguration.RegisterWellKnownServiceType
                (typeof(ObjectProxy), "ObjectProxy", WellKnownObjectMode.SingleCall);
            System.Console.WriteLine("Hit <enter> to exit...");
            System.Console.ReadLine();

        }

    }
}
