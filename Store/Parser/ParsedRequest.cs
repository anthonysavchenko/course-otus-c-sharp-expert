namespace Store.Parser;

public readonly ref struct ParsedRequest
{
  public readonly ReadOnlySpan<byte> Command;
  public readonly ReadOnlySpan<byte> Key;
  public readonly ReadOnlySpan<byte> Value;

  public ParsedRequest(ReadOnlySpan<byte> command, ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
  {
    Command = command;
    Key = key;
    Value = value;
  }
}
