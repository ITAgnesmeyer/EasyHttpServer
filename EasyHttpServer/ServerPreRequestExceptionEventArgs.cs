using Newtonsoft.Json;
using System;
using System.Net;

namespace EasyHttpServer
{
    public class ServerPreRequestExceptionEventArgs 
    {

        public ServerPreRequestExceptionEventArgs(HttpListenerRequest request,Exception exception) 
        {
            this.Exception = exception;
            this.PreRequestArgs = new ServerPreRequestHandlerEventArgs(request)
            {
                ReturnStatus = HttpStatusCode.InternalServerError
            };
            PrefillContent(exception);
        }

        private void PrefillContent(Exception exception)
        {
            var err = new
            {
                error = exception.Message,
                stackTrace = exception.StackTrace.ToString()
            };
            this.PreRequestArgs.ContentToSend = JsonConvert.SerializeObject(err);
        }

        public ServerPreRequestExceptionEventArgs(ServerPreRequestHandlerEventArgs args, Exception exception)
        {
            this.PreRequestArgs = args;
            this.PreRequestArgs.ReturnStatus = HttpStatusCode.InternalServerError;
            this.Exception = exception;
            PrefillContent(this.Exception);

        }
        public ServerPreRequestHandlerEventArgs PreRequestArgs{get;}
        public Exception Exception{get;}
    }
}