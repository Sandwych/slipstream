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

            return Task.Factory.FromAsync<WebResponse>(
                request.BeginGetResponse, request.EndGetResponse, null);
        }

        public static Task<Stream> GetRequestStreamAsync(this WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            return Task.Factory.FromAsync<Stream>(
                request.BeginGetRequestStream, request.EndGetRequestStream, null);
        }
    }
}
