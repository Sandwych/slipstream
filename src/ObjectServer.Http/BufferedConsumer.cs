using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using Kayak;
using Kayak.Http;

namespace ObjectServer.Http
{
    internal class BufferedConsumer : IDataConsumer
    {
        List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>>();
        Action<byte[]> resultCallback;
        Action<Exception> errorCallback;

        public BufferedConsumer(Action<byte[]> resultCallback, Action<Exception> errorCallback)
        {
            this.resultCallback = resultCallback;
            this.errorCallback = errorCallback;
        }

        public bool OnData(ArraySegment<byte> data, Action continuation)
        {
            // since we're just buffering, ignore the continuation. 
            // TODO: place an upper limit on the size of the buffer. 
            // don't want a client to take up all the RAM on our server! 
            buffer.Add(data);
            return false;
        }

        public void OnError(Exception error)
        {
            errorCallback(error);
        }

        public void OnEnd()
        {
            // turn the buffer into a string. 
            // 
            // (if this isn't what you want, you could skip 
            // this step and make the result callback accept 
            // List<ArraySegment<byte>> or whatever) 
            // 
            using (var ms = new MemoryStream())
            {
                foreach (var b in this.buffer)
                {
                    ms.Write(b.Array, b.Offset, b.Count);
                }

                var fullBuf = ms.ToArray();
                resultCallback(fullBuf);
            }
        }
    }
}
