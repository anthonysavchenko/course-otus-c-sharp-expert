using System.Text;

namespace Store.Parser;

public static class CommandParser
{
  private static readonly byte[] _separator = Encoding.Unicode.GetBytes([' ']);

  public static ParsedRequest ParseRequest(ReadOnlySpan<byte> source)
  {
    var parsedCommand = SliceFirstToken(source);
    var parsedKey = SliceFirstToken(parsedCommand.Rest);
    var parsedValue = SliceFirstToken(parsedKey.Rest);

    var command = parsedCommand.FirstToken;
    var key = parsedKey.FirstToken;
    var value = parsedValue.FirstToken;

    if (command.IsEmpty || key.IsEmpty) return new ParsedRequest(command: [], key: [], value: []);

    var parsedRequest = new ParsedRequest(command, key, value);

    return parsedRequest;
  }

  private static SlicedFirstToken SliceFirstToken(ReadOnlySpan<byte> source)
  {
    if (source.IsEmpty) return new SlicedFirstToken(firstToken: [], rest: []);

    var separatorStartIndex = source.IndexOf(_separator);

    if (separatorStartIndex == -1) return new SlicedFirstToken(firstToken: source, rest: []);

    var firstToken = source[..separatorStartIndex];
    var restStartIndex = separatorStartIndex + _separator.Length;

    if (restStartIndex == source.Length) return new SlicedFirstToken(firstToken, rest: []);

    var rest = source[restStartIndex..];

    var slicedFirstToken = new SlicedFirstToken(firstToken, rest);

    return slicedFirstToken;
  }
}
