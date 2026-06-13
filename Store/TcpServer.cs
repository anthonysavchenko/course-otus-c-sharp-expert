using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Store.Parser;

namespace Store;

// TODO: перенести serverSocket в поле класса, реализовать IDisposabel и вынести ServerSocketInit
// TODO: сделать возможность выключать запись в консоль
// TODO: передвавать в сообщениях размер данных, чтобы ограничивать их в буфере при чтении

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
    catch (Exception e)
    {
      Console.WriteLine($"Server {_endPoint}. Exception occured: {e}");
    }
  }

  private async Task WaitAndProcessClientsAsync(Socket serverSocket, CancellationToken cancellationToken = default)
  {
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        var clientSocket = await serverSocket.AcceptAsync(cancellationToken);

        Console.WriteLine($"Client {clientSocket.RemoteEndPoint}. Connected");

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
        while (!cancellationToken.IsCancellationRequested)
        {
          var bytesReceived = await WaitAndProcessClientMessageAsync(clientSocket, cancellationToken);

          if (bytesReceived == 0) break;
        }
      }
      catch (Exception e) when (e is not OperationCanceledException)
      {
        Console.WriteLine($"Client {clientSocket.RemoteEndPoint}. Exception occured {e}");
      }
      finally
      {
        var clientEndPoint = clientSocket.RemoteEndPoint;

        clientSocket.Shutdown(SocketShutdown.Both);
        clientSocket.Close();
        Console.WriteLine($"Client {clientEndPoint}. Disconnected");
      }
    }
  }

  private async Task<int> WaitAndProcessClientMessageAsync(Socket clientSocket, CancellationToken cancellationToken = default)
  {
    var buffer = ArrayPool<byte>.Shared.Rent(_clientMessageMinBytes);

    try
    {
      var bytesReceived = await clientSocket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

      if (bytesReceived != 0)
      {
        ProcessClientMessage(buffer, clientSocket.RemoteEndPoint);
      }

      return bytesReceived;
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }

  private static void ProcessClientMessage(ReadOnlyMemory<byte> message, EndPoint? clientEndPoint)
  {
    var request = CommandParser.ParseBytes(message.Span);

    Console.WriteLine($"Client {clientEndPoint}. Received Command: {Encoding.Unicode.GetString(request.Command)}");
    Console.WriteLine($"Client {clientEndPoint}. Received Key: {Encoding.Unicode.GetString(request.Key)}");
    Console.WriteLine($"Client {clientEndPoint}. Received Value: {Encoding.Unicode.GetString(request.Value)}");
  }
}
