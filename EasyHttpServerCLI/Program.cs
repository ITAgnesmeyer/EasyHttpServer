﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EasyHttpServer;


namespace EasyHttpServerCLI
{

    class Options
    {
        private Dictionary<string, string> _Args;
        public Options(Dictionary<string, string> args)
        {
            this._Args = args;
        }
        public bool Help
        {
            get => this._Args.ContainsKey("/?");
        }

        public bool ContainsBasePath
        {
            get => this._Args.ContainsKey("/x");
        }
        public bool ContainsPrefix
        {
            get => this._Args.ContainsKey("/p");
        }
        public string BasePath
        {
            get => this._Args["/x"];
        }
        public string Prefix
        {
            get => this._Args["/p"];
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = GetOptions(args);
            if (options.Help)
            {
                PrintHelp();
                return;
            }
            if (!options.ContainsPrefix)
            {
                Console.WriteLine("please give prefix e.g=> /p:\"http://localhost:5001/,https://localhost:5002/\"");
                PrintHelp();
                return;
            }

            string[] prefixes = { "http://localhost:5001/" };
            if (options.ContainsPrefix)
            {
                prefixes = options.Prefix.Split(",");

            }

            string basePath = "./";
            if (options.ContainsBasePath)
            {
                basePath = options.BasePath;
            }

            using var server =
                new HttpServer(prefixes, basePath);
            server.ServerError += OnServerError;
            server.ServerLog += OnServerLog;
            //server.PreResponseHandler += OnPreHandler;
            //server.PreRequestException += OnPreHandlerException;
            server.Start();
            while (server.IsRunning())
            {
                Thread.Sleep(100);
            }
        }

        //private static void OnPreHandlerException(object sender, ServerPreRequestExceptionEventArgs e)
        //{


        //}

        //private static void OnPreHandler(object sender, ServerPreRequestHandlerEventArgs e)
        //{
        //    switch (e.Request.Url.LocalPath)
        //    {
        //        case "/api":
        //            var obj = new { message = "halloX" };
        //            e.ContentToSend = JsonConvert.SerializeObject(obj);
        //            e.Handled = true;
        //            break;
        //        default:
        //            e.Handled = false;
        //            break;
        //    }
        //}

        static Options GetOptions(string[] args)
        {
            Dictionary<string, string> retval = args.ToDictionary(
                k => k.Split(new[] { ':' }, 2)[0].ToLower(),
                v => v.Split(new[] { ':' }, 2).Count() > 1
                    ? v.Split(new[] { ':' }, 2)[1]
                    : null);

            Options ops = new Options(retval);
            return ops;
        }
        private static void OnServerLog(object sender, ServerStatusEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void OnServerError(object sender, ServerStartErrorEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("parameters:");
            Console.WriteLine("/p:[prefixes] => set the base urls to listen to.");
            Console.WriteLine("/x:[path] => set the path to the Folder to search the files.");
            Console.WriteLine("\t\tdefault is the current path of the app");
        }
    }
}
