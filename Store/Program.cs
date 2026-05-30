using System.Text;
using Store.Parser;

var parsedRequest = CommandParser.ParseRequest(Encoding.Unicode.GetBytes("SET user:1 data"));

Console.WriteLine(Encoding.Unicode.GetString(parsedRequest.Command.ToArray()));
Console.WriteLine(Encoding.Unicode.GetString(parsedRequest.Key.ToArray()));
Console.WriteLine(Encoding.Unicode.GetString(parsedRequest.Value.ToArray()));
