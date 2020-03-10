using System;

namespace EasyHttpServer
{
    public class ServerStatusEventArgs : EventArgs
    {
        public ServerStatusEventArgs(string message, int statusCode)
        {
            this.Message = message;
            this.StatusCode = statusCode;
        }

        public string Message{get;}
        public int StatusCode{get;}

    }
}