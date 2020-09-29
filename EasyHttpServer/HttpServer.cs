using MimeTypeExtension;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyHttpServer
{
    public class HttpServer : IDisposable
    {
        private Task MainTask { get; set; }
        public string RootPath { get; protected set; }
        public string DefaultErrorResponseType { get; set; }
        public string[] ListenerUrls { get; protected set; }

        private bool LoopEnable { get; set; }
        public string[] IndexFiles { get; set; } = { "index.html", "default.html", "index.htm", "default.htm" };
        public string BadRequestMessage { get; set; } = "<h1>Cannot find file!=></h1><br>{0}";
        public string InternalServerErrorMessage { get; set; } = "<h1>{0}</h1>";
        public string NotFoundMessage { get; set; } = "<h1>Cannot find File!</h1><br>{0}";

        public HttpListener CurrentListener { get; internal set; }

        public event EventHandler<ServerStartErrorEventArgs> ServerError;
        public event EventHandler<ServerStatusEventArgs> ServerLog;
        public event EventHandler<ServerPreRequestHandlerEventArgs> PreResponseHandler;
        public event EventHandler<ServerPreRequestExceptionEventArgs> PreRequestException;
        public HttpServer(string[] listenerUrl, string rootPath = "./")
        {
            this.RootPath = rootPath;
            this.ListenerUrls = listenerUrl;
            this.DefaultErrorResponseType = "text/html; charset=UTF-8";
            this.MainTask = new Task(TaskAction, this);
        }

        public void Start()
        {
            this.LoopEnable = true;
            this.MainTask.Start();
            Console.WriteLine();
            Console.Write("start up");
            while (this.IsRunning() == false && !this.MainTask.IsCompleted)
            {
                
                Console.Write(".");
                Thread.Sleep(100);
            }

            Console.WriteLine();
        }

        public async Task StartAsync()
        {
            this.LoopEnable = true;
            this.MainTask.Start();

            await Task.WhenAll(this.MainTask);
        }

        public void Stop()
        {
            this.LoopEnable = false;
            this.CurrentListener?.Stop();
        }

        public bool IsRunning()
        {
            if(this.CurrentListener == null)
                return false;

            return this.CurrentListener.IsListening;
        }
        public void Abort()
        {
            this.CurrentListener?.Abort();
        }

        private void TaskAction(object o)
        {
            HttpServer server = o as HttpServer;
            if (server == null)
                throw new Exception("cannot resolve ServerObject!");

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, errors) => true;
            using (server.CurrentListener = new HttpListener())
            {
                foreach (string url in server.ListenerUrls)
                {
                    try
                    {
                        server.CurrentListener.Prefixes.Add(url);
                    }
                    catch (Exception e)
                    {
                        OnServerError(new ServerStartErrorEventArgs(e));
                    }
                    
                }

                if (server.CurrentListener.Prefixes.Count <= 0)
                {
                    OnServerLog(new ServerStatusEventArgs("no prefixes added", 0));
                    server.CurrentListener.Close();
                    return;

                }

                try
                {
                    OnServerLog(new ServerStatusEventArgs("start server", 0));
                    server.CurrentListener.Start();
                    foreach (string prefixUrl in server.CurrentListener.Prefixes)
                    {
                        OnServerLog(new ServerStatusEventArgs("listen to:" + prefixUrl, 0));
                    }



                }
                catch (Exception e)
                {
                    server.CurrentListener.Close();
                    OnServerError(new ServerStartErrorEventArgs(e));
                    return;
                }

                try
                {
                    while (LoopEnable)
                    {
                        HttpListenerContext context = server.CurrentListener.GetContext();
                        string absPath = context.Request.Url.LocalPath;
                        bool found = CheckUrlAndGetPath(absPath, server.RootPath, server.IndexFiles, out var path);


                        using (HttpListenerResponse response = context.Response)
                        {
                            OnServerLog(new ServerStatusEventArgs("request:" + context.Request.Url, 0));
                            //CORS
                            response.Headers.Add("Access-Control-Allow-Origin:*");

                            ServerPreRequestHandlerEventArgs args = new ServerPreRequestHandlerEventArgs(context.Request);
                            try
                            {
                                try
                                {
                                    OnPreResponseHandler(args);
                                }
                                catch (Exception e)
                                {
                                    OnPreRequestException(new ServerPreRequestExceptionEventArgs(args, e));

                                }

                            }
                            catch (Exception e)
                            {
                                OnServerError(new ServerStartErrorEventArgs(e));
                                response.Close();
                                continue;
                            }
                            if (args.Handled)
                            {
                                if (args.AdditionalHeaders.Count > 0)
                                {
                                    response.Headers.Add(args.AdditionalHeaders);
                                }

                                response.ContentType = args.MimeType;
                                response.StatusCode = (int)args.ReturnStatus;
                                byte[] bytes = Encoding.UTF8.GetBytes(args.ContentToSend);
                                response.OutputStream.Write(bytes, 0, bytes.Length);
                                response.Close();
                                continue;
                            }

                            if (HandleFileContent(found, server, absPath, response, path))
                            {
                                response.Close();
                                continue;

                            }
                                

                            response.Close();
                        }

                    }

                    server.LoopEnable = false;

                    server.CurrentListener = null;
                }
                catch (Exception e)
                {
                    server.CurrentListener?.Close();
                    server.CurrentListener = null;
                    server.LoopEnable = false;
                    OnServerError(new ServerStartErrorEventArgs(e));

                }

                this.CurrentListener?.Close();
            }
        }

        private bool HandleFileContent(bool found, HttpServer server, string absPath, HttpListenerResponse response,
            string path)
        {
            if (!found)
            {
                string message = string.Format(server.NotFoundMessage, absPath);
                SendNotFoundResponse(message, response, server.DefaultErrorResponseType);
                OnServerLog(new ServerStatusEventArgs(message, response.StatusCode));
                return true;
            }

            response.StatusCode = (int) HttpStatusCode.OK;
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                response.ContentType = fi.MimeTypeOrDefault();
                try
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    using (Stream output = response.OutputStream)
                    {
                        output.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception e)
                {
                    string message = string.Format(server.InternalServerErrorMessage, e.Message);
                    SendInternalServerError(response, message, server.DefaultErrorResponseType);
                    OnServerError(new ServerStartErrorEventArgs(e));
                }
            }
            else
            {
                string message = string.Format(server.BadRequestMessage, absPath);
                SendBadRequestResponse(response, message, server.DefaultErrorResponseType);
                OnServerLog(new ServerStatusEventArgs(message, (int) HttpStatusCode.BadRequest));
            }

            return false;
        }

        private void SendBadRequestResponse(HttpListenerResponse response, string message, string contentType)
        {
            response.ContentType = contentType;
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
        private void SendInternalServerError(HttpListenerResponse response, string message, string contentType)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentType = contentType;
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            response.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private void SendNotFoundResponse(string message, HttpListenerResponse response, string contentType)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            response.ContentType = contentType;
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.Close();
        }

        private bool CheckUrlAndGetPath(string absolutPath, string rootPath, string[] indexFiles, out string cleanedPath)
        {
            // /
            // /wwwroot
            cleanedPath = "";
            if (absolutPath == null)
                absolutPath = "";
            if (absolutPath.StartsWith("/"))
                absolutPath = absolutPath.Substring(1);
            string replacedFile = absolutPath;
            if (string.IsNullOrEmpty(replacedFile))
            {
                foreach (string indexFile in indexFiles)
                {
                    cleanedPath = Path.Combine(rootPath, indexFile);
                    if (File.Exists(cleanedPath))
                    {
                        return true;
                    }
                }
                return false;
            }
            cleanedPath = Path.Combine(rootPath, replacedFile);


            return true;


            
        }



        protected virtual void OnServerError(ServerStartErrorEventArgs e)
        {
            ServerError?.Invoke(this, e);
        }

        protected virtual void OnServerLog(ServerStatusEventArgs e)
        {
            ServerLog?.Invoke(this, e);

        }

        public void Dispose()
        {
            if(MainTask != null)
                this.MainTask.Dispose();
            ((IDisposable)this.CurrentListener)?.Dispose();
        }

        protected virtual void OnPreResponseHandler(ServerPreRequestHandlerEventArgs e)
        {
            PreResponseHandler?.Invoke(this, e);
        }

        protected virtual void OnPreRequestException(ServerPreRequestExceptionEventArgs e)
        {
            PreRequestException?.Invoke(this, e);
        }
    }

    
}
