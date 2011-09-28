using System;
using System.Net;
using System.Windows;
using System.IO;
using System.Threading.Tasks;

namespace ObjectServer.Client.JsonRpc
{
    public static class WebRequestExtensions
    {
        public static Task<WebResponse> GetReponseAsync(this WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var tcs = new TaskCompletionSource<WebResponse>();

            request.BeginGetResponse(ar =>
            {
                try
                {
                    var rep = request.EndGetResponse(ar);
                    tcs.SetResult(rep);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;
        }

        public static Task<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var tcs = new TaskCompletionSource<Stream>();

            request.BeginGetRequestStream(ar =>
            {
                try
                {
                    var stream = request.EndGetRequestStream(ar);
                    tcs.SetResult(stream);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return tcs.Task;

        }
    }
}
