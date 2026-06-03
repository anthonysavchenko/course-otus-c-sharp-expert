using System.Net.Sockets;

namespace Store;

public class TcpServer()
{
  public void StartAsync()
  {
    var c = new Socket("", ProtocolType.IP);
  }
}