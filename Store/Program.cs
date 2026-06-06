using System.Net;
using Store;

var ipAddress = IPAddress.Any;
var port = 8080;
var clientMessageMinBytes = 64;
var server = new TcpServer(ipAddress, port, clientMessageMinBytes);

using var cancellationTokenSource = new CancellationTokenSource();

_ = server.StartAsync(cancellationTokenSource.Token);

Console.WriteLine($"Server is listenting on {ipAddress}:{port}");
Console.WriteLine($"Client message min bytes for ArrayPool: {clientMessageMinBytes}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

await cancellationTokenSource.CancelAsync();
