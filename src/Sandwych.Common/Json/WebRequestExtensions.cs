using System;
using System.Net;
using System.IO;

namespace Sandwych.JsonRpc
{

#if TPL
using System.Threading.Tasks;
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
                WebResponse rep = null;
                try
                {
                    rep = request.EndGetResponse(ar);
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
                Stream stream = null;
                try
                {
                    stream = request.EndGetRequestStream(ar);
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
#endif //TPL
}
