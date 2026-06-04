using System.Net;
using System.Net.Sockets;

namespace Store;

public class TcpServer()
{
  public async Task StartAsync()
  {
    var ipAddress = IPAddress.Any;
    var port = 8080;
    var cancelationToken = new CancellationToken();

    try
    {
      using var serverSoket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

      serverSoket.Bind(new IPEndPoint(ipAddress, port));
      serverSoket.Listen();

      while (true)
      {
        try
        {
          var clientSocket = await serverSoket.AcceptAsync(cancelationToken);

          _ = ProcessClientAsync(clientSocket);
        }
        catch (Exception e)
        {
          Console.WriteLine(e);
          break;
        }
      }
    }
    catch (SocketException e)
    {
      Console.WriteLine(e);
    }
  }

  private async Task ProcessClientAsync(Socket clientSocket)
  {

  }
}