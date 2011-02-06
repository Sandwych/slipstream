using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace ObjectServer.Test
{


    public class Class1
    {

        public Class1()
            : base()
        {
        }


        public static void Main()
        {
            TcpChannel chan = new TcpChannel();
            ChannelServices.RegisterChannel(chan, false);
            ObjectProxy proxy = (ObjectProxy)Activator.GetObject(typeof(ObjectProxy)
                , "tcp://localhost:5000/ObjectProxy");

            var modelName = "Test.TestObject";

            var values = new Dictionary<string, object>()
            {
                { "name", "Oh my new record!" },
            };
            proxy.Execute(modelName, "Create", new object[] { values });

            var ids = new long[] { 1, 2, 17,18 };
            var fields = new string[] { "name" };
            var args = new object[] { fields, ids };
            var dt = (Dictionary<long, Dictionary<string, object>>)proxy.Execute(modelName, "Read", args);

            foreach (KeyValuePair<long, Dictionary<string, object>> pair in dt)
            {
                Console.WriteLine("resule: id=[{0}], name=[{1}]", pair.Value["id"], pair.Value["name"]);
            }

            Console.ReadLine();
        }

    }
}
