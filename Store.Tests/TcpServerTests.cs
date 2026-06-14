using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Store.Tests;

public class TcpServerTests
{
  [Fact]
  public async Task CorrectSet()
  {
    var lines = await SendFromClentToServerAndGetConsoleOutputAsLines("SET user:1 data");

    Assert.Contains("Server 127.0.0.1:8080. Started", lines[0]);
    Assert.Contains("Client message min bytes for ArrayPool: 64", lines[1]);
    Assert.Contains("Client 127.0.0.1", lines[2]); Assert.Contains("Connected", lines[2]);
    Assert.Contains("Client 127.0.0.1", lines[3]); Assert.Contains("Received Command: SET", lines[3]);
    Assert.Contains("Client 127.0.0.1", lines[4]); Assert.Contains("Received Key: user:1", lines[4]);
    Assert.Contains("Client 127.0.0.1", lines[5]); Assert.Contains("Received Value: data", lines[5]);
    Assert.Contains("Client 127.0.0.1", lines[6]); Assert.Contains("Disconnected", lines[6]);
    Assert.Contains("Server 127.0.0.1:8080. Closed", lines[7]);
  }

  [Fact]
  public async Task IncorrectSet()
  {
    var lines = await SendFromClentToServerAndGetConsoleOutputAsLines("SET");

    Assert.Contains("Server 127.0.0.1:8080. Started", lines[0]);
    Assert.Contains("Client message min bytes for ArrayPool: 64", lines[1]);
    Assert.Contains("Client 127.0.0.1", lines[2]); Assert.Contains("Connected", lines[2]);
    Assert.Contains("Client 127.0.0.1", lines[3]); Assert.Contains("Received incorrect request", lines[3]);
    Assert.Contains("Client 127.0.0.1", lines[4]); Assert.Contains("Disconnected", lines[4]);
    Assert.Contains("Server 127.0.0.1:8080. Closed", lines[5]);
  }

  private static async Task<string[]> SendFromClentToServerAndGetConsoleOutputAsLines(string message)
  {
    using var stringWriterOutput = new StringWriter();
    var originalOutput = Console.Out;

    Console.SetOut(stringWriterOutput);

    try
    {
      await Task.Run(() => SendFromClientToServer(message));
    }
    finally
    {
      Console.SetOut(originalOutput);
    }

    var output = stringWriterOutput.ToString();
    var lines = output.Split(Environment.NewLine);

    return lines;
  }

  private static async Task SendFromClientToServer(string message)
  {
    var ipAddress = IPAddress.Parse("127.0.0.1");
    var port = 8080;
    var clientMessageMinBytes = 64;
    var server = new TcpServer(ipAddress, port, clientMessageMinBytes);

    using var cancellationTokenSource = new CancellationTokenSource();
    var cancellationToken = cancellationTokenSource.Token;

    var serverListeningTask = server.StartAsync(cancellationToken);

    var serverEndPoint = new IPEndPoint(ipAddress, port);

    await SendFromClient(message, serverEndPoint, cancellationToken);

    // Ждем перед завершением работы сервера, чтобы он успевал обработать данные клиента
    await Task.Delay(1000);

    await cancellationTokenSource.CancelAsync();

    // Ждем перед завершением программы, чтобы сервер корректно обработал завершение работы
    await serverListeningTask;
  }

  private static async Task SendFromClient(string message, EndPoint serverEndPoint, CancellationToken cancellationToken)
  {
    using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

    await client.ConnectAsync(serverEndPoint, cancellationToken);
    await client.SendAsync(Encoding.Unicode.GetBytes(message), SocketFlags.None, cancellationToken);
    await client.DisconnectAsync(reuseSocket: false, cancellationToken);

    client.Shutdown(SocketShutdown.Both);
    client.Close();
  }
}
