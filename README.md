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
