namespace Store.Store;

public class SimpleStore
{
  private readonly ReaderWriterLockSlim _lock = new();

  private readonly Dictionary<string, byte[]> _storage = [];

  private long _setCount;

  private long _getCount;

  private long _deleteCount;

  public void Set(string key, byte[] value)
  {
    if (string.IsNullOrEmpty(key)) throw new EmptyArgumentException(nameof(key));
    if (value == null || value.Length == 0) throw new EmptyArgumentException(nameof(value));

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
    if (string.IsNullOrEmpty(key)) throw new EmptyArgumentException(nameof(key));

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
    if (string.IsNullOrEmpty(key)) throw new EmptyArgumentException(nameof(key));

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
