using System;

namespace EasyHttpServer
{
    public class ServerStartErrorEventArgs : EventArgs
    {
        public ServerStartErrorEventArgs(Exception exception)
        {
            this.Exception = exception;
            this.Message = exception.Message;
        }

        public string Message{get;}
        public Exception Exception{get;}

    }
}