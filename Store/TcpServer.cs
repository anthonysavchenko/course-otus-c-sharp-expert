using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Store.Parser;

namespace Store;

// TODO: перенести serverSocket в поле класса и вынести ServerSocketInit
// TODO: сделать возможность выключать запись в консоль

public class TcpServer(IPAddress ipAddress, int port, int clientMessageMinBytes)
{
  private readonly IPEndPoint _endPoint = new(ipAddress, port);

  private readonly int _clientMessageMinBytes = clientMessageMinBytes;

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      using var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

      serverSocket.Bind(_endPoint);
      serverSocket.Listen();

      Console.WriteLine($"Server {_endPoint}. Started");
      Console.WriteLine($"Client message min bytes for ArrayPool: {_clientMessageMinBytes}");

      await WaitAndProcessClientsAsync(serverSocket, cancellationToken);
    }
    catch (Exception e) when (e is not OperationCanceledException)
    {
      Console.WriteLine($"Server {_endPoint}. Exception occured: {e}");
    }
  }

  private async Task WaitAndProcessClientsAsync(Socket serverSocket, CancellationToken cancellationToken = default)
  {
    try
    {
      while (true)
      {
        var clientSocket = await serverSocket.AcceptAsync(cancellationToken);

        Console.WriteLine($"Client {clientSocket.LocalEndPoint}. Connected");

        _ = ProcessClientAsync(clientSocket, cancellationToken);
      }
    }
    catch (OperationCanceledException)
    {
      if (serverSocket.Connected) serverSocket.Shutdown(SocketShutdown.Both);
      serverSocket.Close();
      Console.WriteLine($"Server {_endPoint}. Closed");
    }
  }

  private async Task ProcessClientAsync(Socket clientSocket, CancellationToken cancellationToken = default)
  {
    using (clientSocket)
    {
      try
      {
        while (true)
        {
          var buffer = ArrayPool<byte>.Shared.Rent(_clientMessageMinBytes);
          var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

          if (bytesReceived != 0)
          {
            var request = CommandParser.ParseBytes(buffer);

            Console.WriteLine($"Client {clientSocket.LocalEndPoint}. Buffer: {Encoding.Unicode.GetString(buffer)}");
            Console.WriteLine($"Client {clientSocket.LocalEndPoint}. Received Command: {Encoding.Unicode.GetString(request.Command)}");
            Console.WriteLine($"Client {clientSocket.LocalEndPoint}. Received Key: {Encoding.Unicode.GetString(request.Key)}");
            Console.WriteLine($"Client {clientSocket.LocalEndPoint}. Received Value: {Encoding.Unicode.GetString(request.Value)}");
          }

          ArrayPool<byte>.Shared.Return(buffer);

          if (bytesReceived == 0) break;
        }
      }
      catch (Exception e) when (e is not OperationCanceledException)
      {
        Console.WriteLine($"Client {clientSocket.LocalEndPoint}. Exception occured {e}");
      }
      finally
      {
        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
        Console.WriteLine($"Client {clientSocket.LocalEndPoint}. Disconnected");
      }
    }
  }

  private async Task<ReadOnlyMemory<byte>> ReadDataFromSocket(TcpClient client)
  {
    return new ReadOnlyMemory<byte>();
  }
}
