using System.Text;

namespace Store.Parser;

public static class CommandParser
{
  public const string SetCommandType = "set";

  public const string GetCommandType = "get";

  public const string DeleteCommandType = "del";

  public static string GetString(ReadOnlySpan<byte> input) => Encoding.UTF8.GetString(input);

  public static byte[] GetBytes(string input) => Encoding.UTF8.GetBytes(input);

  private static readonly byte[] _separator = GetBytes(" ");

  public static ParsedRequest ParseBytes(ReadOnlySpan<byte> bytes)
  {
    var parsedCommand = SliceFirstToken(bytes);
    var parsedKey = SliceFirstToken(parsedCommand.Rest);
    var parsedValue = SliceFirstToken(parsedKey.Rest);

    var command = parsedCommand.FirstToken;
    var key = parsedKey.FirstToken;
    var value = parsedValue.FirstToken;

    if (command.IsEmpty || key.IsEmpty) return new ParsedRequest(command: [], key: [], value: []);

    var parsedRequest = new ParsedRequest(command, key, value);

    return parsedRequest;
  }

  private static SlicedFirstToken SliceFirstToken(ReadOnlySpan<byte> bytes)
  {
    if (bytes.IsEmpty) return new SlicedFirstToken(firstToken: [], rest: []);

    var separatorStartIndex = bytes.IndexOf(_separator);

    if (separatorStartIndex == -1) return new SlicedFirstToken(firstToken: bytes, rest: []);

    var firstToken = bytes[..separatorStartIndex];
    var restStartIndex = separatorStartIndex + _separator.Length;

    if (restStartIndex == bytes.Length) return new SlicedFirstToken(firstToken, rest: []);

    var rest = bytes[restStartIndex..];
    var slicedFirstToken = new SlicedFirstToken(firstToken, rest);

    return slicedFirstToken;
  }
}
