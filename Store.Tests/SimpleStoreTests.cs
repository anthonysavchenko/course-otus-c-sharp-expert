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

    var copyFromKey = "user:1";
    var copyToKey = "user:2";
    var value = "data";
    var count = 10;
    var bytes = GetBytes(value);

    store.Set(copyFromKey, bytes);

    var tasks = ArrangeTasks(store, copyFromKey, copyToKey, count);

    await Task.WhenAll(tasks);

    var valueFromStoreCopyFrom = store.Get(copyFromKey);

    Assert.Equal(value, GetString(valueFromStoreCopyFrom));

    var valueFromStoreCopyTo = store.Get(copyToKey);

    Assert.Equal(value, GetString(valueFromStoreCopyTo));

    store.Delete(copyFromKey);
    store.Delete(copyToKey);

    var (setCount, getCount, deleteCount) = store.GetStatistics();

    Assert.Equal(count + 1, setCount);
    Assert.Equal(count + 2, getCount);
    Assert.Equal(2, deleteCount);
  }

  public IEnumerable<Task> ArrangeTasks(SimpleStore store, string copyFromKey, string copyToKey, int count)
  {
    var tasks = new List<Task>();

    void Action() => CopyStoreValue(store, copyFromKey, copyToKey);

    for (int i = 0; i < count; i++)
    {
      var task = Task.Run(Action, TestContext.Current.CancellationToken);

      tasks.Add(task);
    }

    return tasks;
  }

  private static void CopyStoreValue(SimpleStore store, string copyFromKey, string copyToKey)
  {
    var value = store.Get(copyFromKey);

    store.Set(copyToKey, value!);
  }

  private static byte[] GetBytes(string requestString) => Encoding.Unicode.GetBytes(requestString);

  private static string GetString(ReadOnlySpan<byte> bytes) => Encoding.Unicode.GetString(bytes);
}
