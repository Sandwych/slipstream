using System;
using System.Collections.Generic;
using System.Collections;
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


        /*
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
            var id = (long)proxy.Execute("objectserver", modelName, "Create", new object[] { values });
            Console.WriteLine("id=[{0}]", id);

            var ids = new long[] { 1, 2, 17, 18 };
            var fields = new string[] { "name" };
            var args = new object[] { fields, ids };
            var dt = (Hashtable[])proxy.Execute("objectserver", modelName, "Read", args);

            foreach (Hashtable row in dt)
            {
                Console.WriteLine("resule: id=[{0}], name=[{1}]", row["id"], row["name"]);
            }

            Console.ReadLine();
        }
        */

    }
}
