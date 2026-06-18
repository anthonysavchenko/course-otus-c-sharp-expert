using System.Net;
using Store;

var ipAddress = IPAddress.Parse("127.0.0.1");
var port = 8080;
var clientMessageMinBytes = 64;
var server = new TcpServer(ipAddress, port, clientMessageMinBytes);

using var cancellationTokenSource = new CancellationTokenSource();

var serverListeningTask = server.StartAsync(cancellationTokenSource.Token);

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();

await cancellationTokenSource.CancelAsync();
await serverListeningTask;
