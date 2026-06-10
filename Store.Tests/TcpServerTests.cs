using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Store.Tests;

public class TcpServerTests
{
  [Fact]
  public async Task CorrectSetRequest()
  {
    using var stringWriter = new StringWriter();
    var originalOutput = Console.Out;
    Console.SetOut(stringWriter);

    try
    {
      var ipAddress = IPAddress.Parse("127.0.0.1");
      var port = 8080;
      var clientMessageMinBytes = 64;
      var server = new TcpServer(ipAddress, port, clientMessageMinBytes);

      using var cancellationTokenSource = new CancellationTokenSource();

      var serverListeningTask = server.StartAsync(cancellationTokenSource.Token);

      await Task.Run(async () =>
        {
          using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
          await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080), cancellationTokenSource.Token);
          await client.SendAsync(Encoding.Unicode.GetBytes("SET user:1 data"), SocketFlags.None, cancellationTokenSource.Token);
          await client.DisconnectAsync(reuseSocket: false, cancellationTokenSource.Token);
          client.Shutdown(SocketShutdown.Both);
          client.Close();
        },
        cancellationTokenSource.Token
      );

      await Task.Delay(500, cancellationTokenSource.Token);

      await cancellationTokenSource.CancelAsync();
      await serverListeningTask;

      var output = stringWriter.ToString();
      Assert.Contains("Server 127.0.0.1:8080. Started", output);
      Assert.Contains("Client message min bytes for ArrayPool: 64", output);
      Assert.Contains("Client 127.0.0.1:8080. Connected", output);
      Assert.Contains("Client 127.0.0.1:8080. Buffer: SET user:1 data", output);
      Assert.Contains("Client 127.0.0.1:8080. Received Command: SET", output);
      Assert.Contains("Client 127.0.0.1:8080. Received Key: user:1", output);
      Assert.Contains("Client 127.0.0.1:8080. Received Value: data", output);
      Assert.Contains("Server 127.0.0.1:8080. Closed", output);
    }
    finally
    {
      Console.SetOut(originalOutput);
    }
  }
}
