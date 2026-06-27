using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Store.Parser;
using Store.Store;

namespace Store;

// TODO: перенести serverSocket в поле класса и вынести ServerSocketInit
// TODO: сделать возможность выключать запись в консоль
// TODO: успростить метод ProcessClientMessage
// TODO: переименовать ParsedRequest в Command
// TODO: вынести в отдельную папку Server, переименовать Store, чтобы не было две одинаковые папки
// TODO: использовать внутри TcpServer хранилище через интерфейс IStore
// TODO: сократить количество параметров конструктора TcpServer

public class TcpServer(IPAddress ipAddress, int port, int clientMessageMinBytes, SimpleStore store) : IDisposable
{
  private readonly IPEndPoint _endPoint = new(ipAddress, port);

  private readonly int _clientMessageMinBytes = clientMessageMinBytes;

  private readonly SimpleStore _store = store;

  private bool disposed;

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

        _ = Task.Run(() => ProcessClientAsync(clientSocket, cancellationToken), cancellationToken);
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
        ProcessClientMessage(buffer.AsMemory(0, bytesReceived), clientSocket.RemoteEndPoint);
      }

      return bytesReceived;
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }

  private void ProcessClientMessage(ReadOnlyMemory<byte> message, EndPoint? clientEndPoint)
  {
    var request = CommandParser.ParseBytes(message.Span);

    if (request.IsEmpty())
    {
      Console.WriteLine($"Client {clientEndPoint}. Received incorrect request");

      return;
    }

    var commandType = CommandParser.GetString(request.Command).ToLowerInvariant();
    var key = CommandParser.GetString(request.Key);

    switch (commandType)
    {
      case CommandParser.SetCommandType:
        {
          var value = request.Value.ToArray();

          if (value.Length == 0)
          {
            Console.WriteLine($"Client {clientEndPoint}. Received incorrect request");

            return;
          }

          _store.Set(key, value);

          Console.WriteLine($"Client {clientEndPoint}. Received Command: SET {key} {CommandParser.GetString(value)}");

          break;
        }
      case CommandParser.GetCommandType:
        {
          var value = _store.Get(key);

          Console.WriteLine($"Client {clientEndPoint}. Received Command: GET {key} {value}");

          break;
        }
      case CommandParser.DeleteCommandType:
        {
          _store.Delete(key);

          Console.WriteLine($"Client {clientEndPoint}. Received Command: DEL {key}");

          break;
        }
    }
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposed)
    {
      if (disposing)
      {
        _store.Dispose();
      }

      disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
