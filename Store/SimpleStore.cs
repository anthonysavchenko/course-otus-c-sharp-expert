namespace Store;

public class SimpleStore
{
  private readonly ReaderWriterLockSlim _lock = new();
  private readonly Dictionary<string, byte[]> _storage = [];

  public void Set(string key, byte[] value)
  {
    _lock.EnterWriteLock();
    try
    {
      _storage[key] = value;
    }
    finally
    {
      _lock.ExitWriteLock();
    }
  }

  public byte[]? Get(string key)
  {
    _lock.EnterReadLock();
    try
    {
      return _storage.GetValueOrDefault(key);
    }
    finally
    {
      _lock.ExitReadLock();
    }
  }

  public void Delete(string key)
  {
    _lock.EnterWriteLock();
    try
    {
      _storage.Remove(key);
    }
    finally
    {
      _lock.ExitWriteLock();
    }
  }
}
