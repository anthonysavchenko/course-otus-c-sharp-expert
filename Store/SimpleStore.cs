namespace Store;

public class SimpleStore
{
  private readonly ReaderWriterLockSlim _lock = new();
  private readonly Dictionary<string, byte[]> _storage = [];

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
  }

  public byte[]? Get(string key)
  {
    if (string.IsNullOrEmpty(key)) return null;

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
  }
}
