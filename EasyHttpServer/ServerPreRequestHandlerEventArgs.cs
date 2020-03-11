using System;
using System.Collections.Specialized;
using System.Net;

namespace EasyHttpServer
{
    public class ServerPreRequestHandlerEventArgs : EventArgs
    {
        public ServerPreRequestHandlerEventArgs(HttpListenerRequest request)
        {
            this.Request = request;
            this.AdditionalHeaders = new NameValueCollection();
            this.Handled = false;
            this.MimeType = "application/json";
            this.ReturnStatus = HttpStatusCode.OK;
        }

        public HttpListenerRequest Request{get;protected set;}
        public string ContentToSend{get;set;}
        public HttpStatusCode ReturnStatus{get;set;}
        public bool Handled{get;set;}
        public string MimeType { get;set; }
        public NameValueCollection AdditionalHeaders { get;protected set; }

    }
}