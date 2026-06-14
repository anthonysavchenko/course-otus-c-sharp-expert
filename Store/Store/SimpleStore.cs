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
    CheckIsNullOrEmpty(key, nameof(key));
    CheckIsNullOrEmpty(value, nameof(value));

    void Writer() => _storage[key] = value;

    WriteLocked(Writer);

    Interlocked.Increment(ref _setCount);
  }

  static void CheckIsNullOrEmpty(string param, string paramName)
  {
    if (string.IsNullOrEmpty(param)) throw new EmptyArgumentException(paramName);
  }

  static void CheckIsNullOrEmpty(byte[] param, string paramName)
  {
    if (param == null || param.Length == 0) throw new EmptyArgumentException(paramName);
  }

  private void WriteLocked(Action writer)
  {
    _lock.EnterWriteLock();

    try
    {
      writer.Invoke();
    }
    finally
    {
      _lock.ExitWriteLock();
    }
  }

  public byte[]? Get(string key)
  {
    CheckIsNullOrEmpty(key, nameof(key));

    var value = (byte[]?)null;

    void Reader() => value = _storage.GetValueOrDefault(key);

    ReadLocked(Reader);

    Interlocked.Increment(ref _getCount);

    return value;
  }

  private void ReadLocked(Action reader)
  {
    _lock.EnterReadLock();

    try
    {
      reader.Invoke();
    }
    finally
    {
      _lock.ExitReadLock();
    }
  }

  public void Delete(string key)
  {
    CheckIsNullOrEmpty(key, nameof(key));

    void Writer() => _storage.Remove(key);

    WriteLocked(Writer);

    Interlocked.Increment(ref _deleteCount);
  }

  public (long, long, long) GetStatistics() => (_setCount, _getCount, _deleteCount);
}
