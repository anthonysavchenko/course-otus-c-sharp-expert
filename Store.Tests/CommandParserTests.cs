using System.Text;
using Store.Parser;

namespace Store.Tests;

public class CommandParserTests
{
  [Fact]
  public void CorrectSet()
  {
    var (command, key, value) = Parse("SET user:1 data");

    Assert.Equal("SET", command);
    Assert.Equal("user:1", key);
    Assert.Equal("data", value);
  }

  [Fact]
  public void CorrectGet()
  {
    var (command, key, value) = Parse("GET user:1");

    Assert.Equal("GET", command);
    Assert.Equal("user:1", key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectGet_NoKey()
  {
    var (command, key, value) = Parse("GET");

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectSet_NoCommand()
  {
    var (command, key, value) = Parse(" user:1 data");

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void Incorrect_Empty()
  {
    var (command, key, value) = Parse("");

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void Incorrect_SpacesOnly()
  {
    var (command, key, value) = Parse("  ");

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectSet_TooManySpacesAfterCommand()
  {
    var (command, key, value) = Parse("SET        user:1 data");

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectSet_TooManySpacesAfterKey()
  {
    var (command, key, value) = Parse("SET user:1         data");

    Assert.Equal("SET", command);
    Assert.Equal("user:1", key);
    Assert.Equal(string.Empty, value);
  }

  [Fact]
  public void IncorrectGet_NoKeyExtraSpace()
  {
    var (command, key, value) = Parse("GET ");

    Assert.Equal(string.Empty, command);
    Assert.Equal(string.Empty, key);
    Assert.Equal(string.Empty, value);
  }

  private static (string, string, string) Parse(string requestString)
  {
    var bytes = CommandParser.GetBytes(requestString);
    var parsedRequest = CommandParser.ParseBytes(bytes);

    return GetStrings(parsedRequest);
  }

  private static (string, string, string) GetStrings(ParsedRequest parsedRequest) => (
    CommandParser.GetString(parsedRequest.Command),
    CommandParser.GetString(parsedRequest.Key),
    CommandParser.GetString(parsedRequest.Value)
  );
}
