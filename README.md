# EasyHttpServer
A little easy http-Server

## Example
```c#

var server = new HttpServer(new[]{"http://localhost:5001/"},"c:\tmp\wwwroot"
//if you want to wait
server.StartAsync().wait();

//if you want to run it and continue
server.Start();

//ASYNC Function
await server.StartAsync();

```
## Events
```c#
var server = new HttpServer(new[]{"http://localhost:5001/"},"c:\tmp\wwwroot")
server.ServerError+=OnServerError;
server.ServerLog+=OnServerLog;
await server.StartAsync();

...
private static void OnServerLog(object sender, ServerStatusEventArgs e)
{
  Console.WriteLine(e.Message);
}

private static void OnServerError(object sender, ServerStartErrorEventArgs e)
{
  Console.WriteLine(e.Message);
}

```
## Build your own API's
```c#
 server.PreResponseHandler += OnPreHandler;
 server.PreRequestException += OnPreHandlerException;
 ...
 private static void OnPreHandler(object sender, ServerPreRequestHandlerEventArgs e)
 {
     switch (e.Request.Url.LocalPath)
     {
         case "/api":
             var obj = new { message = "halloX" };
             e.ContentToSend = JsonConvert.SerializeObject(obj);
             e.Handled = true;
             break;
         default:
             e.Handled = false;
             break;
     }
 }
 
```

# CLI
```shell
EasyHttpServerCLI /p:"http://localhost:5001/,http://localhost:5002/" /x:C:\tmp\wwwroot
```
# HTTPS
Use the BindingsTool to view or set HTTPS bindings.

See: https://github.com/segor/SslCertBinding.Net




2024 Dipl.-Ing.(FH) Guido Agnesmeyer
