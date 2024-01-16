#if NETSTANDARD
using Newtonsoft.Json;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif
using System;
using System.Net;

namespace EasyHttpServer
{
#pragma warning disable CS8981
    public class err
    {
        public string error { get; set; }
        public string stackTrace { get; set; }
    }
#pragma warning restore CS8981
#if !NETSTANDARD
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(err))]
internal partial class SourceGenerationContext: JsonSerializerContext
{
    
}
#endif

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
            var err = new err
            {
                error = exception.Message,
                stackTrace = exception.StackTrace.ToString()
            };
#if NETSTANDARD
            this.PreRequestArgs.ContentToSend = JsonConvert.SerializeObject(err);
#else

this.PreRequestArgs.ContentToSend = JsonSerializer.Serialize(err, SourceGenerationContext.Default.err);
#endif

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