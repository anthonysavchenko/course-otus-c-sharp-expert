using System.Text;
using Store.Parser;

namespace Store.Tests;

public class CommandParserUnitTest
{
  private static byte[] GetBytes(string requestString) => Encoding.Unicode.GetBytes(requestString);

  private static string GetString(ReadOnlySpan<byte> bytes) => Encoding.Unicode.GetString(bytes);

  private static (string, string, string) GetStrings(ParsedRequest parsedRequest) => (
    GetString(parsedRequest.Command),
    GetString(parsedRequest.Key),
    GetString(parsedRequest.Value)
  );

  [Fact]
  public void CorrectRequest_CommandSet()
  {
    var bytes = GetBytes("SET user:1 data");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal("SET", command);
    Assert.Equal("user:1", key);
    Assert.Equal("data", value);
  }

  [Fact]
  public void CorrectRequest_CommandGet()
  {
    var bytes = GetBytes("GET user:1");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal("GET", command);
    Assert.Equal("user:1", key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectGetRequest_NoKey()
  {
    var bytes = GetBytes("GET");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectSetRequest_NoCommand()
  {
    var bytes = GetBytes(" user:1 data");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectRequest_Empty()
  {
    var bytes = GetBytes("");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectRequest_SpacesOnly()
  {
    var bytes = GetBytes("  ");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectSetRequest_TooManySpacesAfterCommand()
  {
    var bytes = GetBytes("SET        user:1 data");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectSetRequest_TooManySpacesAfterKey()
  {
    var bytes = GetBytes("SET user:1         data");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal("SET", command);
    Assert.Equal("user:1", key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectGetRequest_NoKeyExtraSpace()
  {
    var bytes = GetBytes("GET ");

    var parsedRequest = CommandParser.ParseBytes(bytes);

    var (command, key, value) = GetStrings(parsedRequest);

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }
}
