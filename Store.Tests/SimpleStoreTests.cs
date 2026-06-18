using System.Text;
using Store.Store;

namespace Store.Tests;

public class SimpleStoreTests
{
  [Fact]
  public void IncorrectSet_NullKey()
  {
    using var store = new SimpleStore();
    var key = (string?)null;
    var value = GetBytes("data");

    void Action() => store.Set(key!, value);

    Assert.Throws<ArgumentNullException>(Action);
  }

  [Fact]
  public void IncorrectSet_EmptyKey()
  {
    using var store = new SimpleStore();
    var key = string.Empty;
    var value = GetBytes("data");

    void Action() => store.Set(key, value);

    Assert.Throws<ArgumentException>(Action);
  }

  [Fact]
  public void IncorrectSet_NullValue()
  {
    using var store = new SimpleStore();
    var key = "user:1";
    var value = (byte[]?)null;

    void Action() => store.Set(key, value!);

    Assert.Throws<ArgumentNullException>(Action);
  }

  [Fact]
  public void IncorrectSet_EmptyValue()
  {
    using var store = new SimpleStore();
    var key = "user:1";
    var value = Array.Empty<byte>();

    void Action() => store.Set(key, value);

    Assert.Throws<ArgumentOutOfRangeException>(Action);
  }

  [Fact]
  public void IncorrectGet_NullKey()
  {
    using var store = new SimpleStore();
    var key = (string?)null;

    void Action() => store.Get(key!);

    Assert.Throws<ArgumentNullException>(Action);
  }

  [Fact]
  public void IncorrectGet_EmptyKey()
  {
    using var store = new SimpleStore();
    var key = string.Empty;

    void Action() => store.Get(key);

    Assert.Throws<ArgumentException>(Action);
  }

  [Fact]
  public void IncorrectDelete_NullKey()
  {
    using var store = new SimpleStore();
    var key = (string?)null;

    void Action() => store.Delete(key!);

    Assert.Throws<ArgumentNullException>(Action);
  }

  [Fact]
  public void IncorrectDelete_EmptyKey()
  {
    using var store = new SimpleStore();
    var key = string.Empty;

    void Action() => store.Delete(key);

    Assert.Throws<ArgumentException>(Action);
  }

  [Fact]
  public void CorrectSetGetDelete()
  {
    using var store = new SimpleStore();
    var key = "user:1";
    var value = GetBytes("data");

    store.Set(key, value);

    var valueFromStore = store.Get(key);

    store.Delete(key);

    var valueFromStoreAfterDelete = store.Get(key);

    Assert.Equal("data", GetString(valueFromStore));
    Assert.Null(valueFromStoreAfterDelete);
  }

  [Fact]
  public async Task CorrectSetGetAsync()
  {
    using var store = new SimpleStore();
    var key = "user:1";
    var value = GetBytes("data");
    var valueFromStore = (byte[]?)null;

    void SetAction() => store.Set(key, value);
    void GetFunc() => valueFromStore = store.Get(key);

    var setTask = Task.Run(SetAction, TestContext.Current.CancellationToken);
    var getTask = Task.Run(GetFunc, TestContext.Current.CancellationToken);

    await Task.WhenAll([setTask, getTask]);

    Assert.Equal("data", GetString(valueFromStore));
  }

  private static byte[] GetBytes(string requestString) => Encoding.Unicode.GetBytes(requestString);

  private static string GetString(ReadOnlySpan<byte> bytes) => Encoding.Unicode.GetString(bytes);
}
