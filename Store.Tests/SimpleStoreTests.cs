using System.Text;

namespace Store.Tests;

public class SimpleStoreTests
{
  [Fact]
  public void IncorrectSet_NullKey()
  {
    var store = new SimpleStore();
    var key = (string?)null;
    var value = GetBytes("data");

    store.Set(key, value);

    var valueFromStore = store.Get(key);

    Assert.Null(valueFromStore);
  }

  [Fact]
  public void IncorrectSet_EmptyKey()
  {
    var store = new SimpleStore();
    var key = string.Empty;
    var value = GetBytes("data");

    store.Set(key, value);

    var valueFromStore = store.Get(key);

    Assert.Null(valueFromStore);
  }

  [Fact]
  public void IncorrectSet_NullValue()
  {
    var store = new SimpleStore();
    var key = "user:1";
    var value = (byte[]?)null;

    store.Set(key, value);

    var valueFromStore = store.Get(key);

    Assert.Null(valueFromStore);
  }

  [Fact]
  public void IncorrectSet_EmptyValue()
  {
    var store = new SimpleStore();
    var key = "user:1";
    var value = Array.Empty<byte>();

    store.Set(key, value);

    var valueFromStore = store.Get(key);

    Assert.Null(valueFromStore);
  }

  [Fact]
  public void IncorrectDelete_NullKey()
  {
    var store = new SimpleStore();
    var key = (string?)null;

    store.Delete(key);

    Assert.True(true);
  }

  [Fact]
  public void IncorrectDelete_EmptyKey()
  {
    var store = new SimpleStore();
    var key = string.Empty;

    store.Delete(key);

    Assert.True(true);
  }

  [Fact]
  public void CorrectSetGetDelete()
  {
    var store = new SimpleStore();
    var key = "user:1";
    var value = GetBytes("data");

    store.Set(key, value);

    var valueFromStore = store.Get(key);

    store.Delete(key);

    var valueFromStoreAfterDelete = store.Get(key);

    Assert.Equal("data", GetString(valueFromStore));
    Assert.Null(valueFromStoreAfterDelete);
  }

  private static byte[] GetBytes(string requestString) => Encoding.Unicode.GetBytes(requestString);

  private static string GetString(ReadOnlySpan<byte> bytes) => Encoding.Unicode.GetString(bytes);
}
