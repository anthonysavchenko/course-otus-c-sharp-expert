namespace Store;

public readonly ref struct ParsedRequest
{
  public readonly ReadOnlySpan<byte> Command;
  public readonly ReadOnlySpan<byte> Key;
  public readonly ReadOnlySpan<byte> Value;
}
