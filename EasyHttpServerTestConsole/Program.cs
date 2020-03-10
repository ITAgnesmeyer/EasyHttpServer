using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EasyHttpServer;

namespace EasyHttpServerCLI
{

    class Optíons
    {
        private Dictionary<string, string> _Args;
        public Optíons(Dictionary<string,string> args)
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
                Console.WriteLine("parameters:");
                Console.WriteLine("/p:[prefixes] => set the base urls to listen to.");
                Console.WriteLine("/x:[path] => set the path to the Folder to search the files.");
                Console.WriteLine("\t\tdefault is the current path of the app");
                return;
            }
            if (!options.ContainsPrefix)
            {
                Console.WriteLine("please give prefix e.g=> /p:\"http://localhost:5001,https://localhost:5002\"");
                Console.WriteLine("");
                return;
            }

            string[] prefixes = {"http://localhost:5001/"};
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
                new HttpServer(prefixes,basePath);
            server.ServerError+=OnServerError;
            server.ServerLog+=OnServerLog;
            server.StartAsync().Wait();
            while (server.IsRunning())
            {
                Thread.Sleep(100);
            }
        }
        static Optíons GetOptions(string[] args)
        {
            Dictionary<string, string> retval = args.ToDictionary(
                k => k.Split(new char[] { ':' }, 2)[0].ToLower(),
                v => v.Split(new char[] { ':' }, 2).Count() > 1 
                    ? v.Split(new char[] { ':' }, 2)[1] 
                    : null);

            Optíons ops = new Optíons(retval);
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


    }
}
