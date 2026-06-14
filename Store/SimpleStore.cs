namespace Store;

public class SimpleStore
{
  private readonly ReaderWriterLockSlim _lock = new();

  private readonly Dictionary<string, byte[]> _storage = [];

  private long _setCount;

  private long _getCount;

  private long _deleteCount;

  public void Set(string key, byte[] value)
  {
    if (string.IsNullOrEmpty(key)) return;
    if (value == null || value.Length == 0) return;

    _lock.EnterWriteLock();

    try
    {
      _storage[key] = value;
    }
    finally
    {
      _lock.ExitWriteLock();
    }

    Interlocked.Increment(ref _setCount);
  }

  public byte[]? Get(string key)
  {
    if (string.IsNullOrEmpty(key)) return null;

    _lock.EnterReadLock();

    byte[]? value;

    try
    {
      value = _storage.GetValueOrDefault(key);
    }
    finally
    {
      _lock.ExitReadLock();
    }

    Interlocked.Increment(ref _getCount);

    return value;
  }

  public void Delete(string key)
  {
    if (string.IsNullOrEmpty(key)) return;

    _lock.EnterWriteLock();

    try
    {
      _storage.Remove(key);
    }
    finally
    {
      _lock.ExitWriteLock();
    }

    Interlocked.Increment(ref _deleteCount);
  }

  public (long, long, long) GetStatistics() => (_setCount, _getCount, _deleteCount);
}
