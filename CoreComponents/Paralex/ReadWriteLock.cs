namespace CoreComponents.Paralex;

public class ReadWriteLock
{
    private readonly ReaderWriterLock rwLock;
    private readonly int timeout;
    public ReadWriteLock(int timeout = 3000)
    {
        this.rwLock = new ReaderWriterLock();
        this.timeout = timeout;
    }

    public void Read(Action action)
    {
        rwLock.AcquireReaderLock(timeout);
        try
        {
            action();
            rwLock.ReleaseReaderLock();
        }
        catch (Exception)
        {
            rwLock.ReleaseReaderLock();
            throw;
        }
    }
    public void Write(Action action)
    {
        rwLock.AcquireWriterLock(timeout);
        try
        {
            action();
            rwLock.ReleaseWriterLock();
        }
        catch (Exception)
        {
            rwLock.ReleaseWriterLock();
            throw;
        }
    }
}

public class ReadWriteLock<T>
{
    private readonly T _target;
    private readonly ReaderWriterLock _rwLock;
    private readonly int _timeout;

    public ReadWriteLock(T target, int timeout = 3000)
    {
        this._target = target;
        this._rwLock = new ReaderWriterLock();
        this._timeout = timeout;
    }
    
    public void Read(Action<T> action)
    {
        _rwLock.AcquireReaderLock(_timeout);
        try
        {
            action(_target);
            _rwLock.ReleaseReaderLock();
        }
        catch (Exception)
        {
            _rwLock.ReleaseReaderLock();
            throw;
        }
    }

    public void Write(Action<T> action)
    {
        _rwLock.AcquireWriterLock(_timeout);
        try
        {
            action(_target);
            _rwLock.ReleaseWriterLock();
        }
        catch (Exception)
        {
            _rwLock.ReleaseWriterLock();
            throw;
        }
    }
    public TR Read<TR>(Func<T, TR> action)
    {
        _rwLock.AcquireReaderLock(_timeout);
        try
        {
            var re = action(_target);
            _rwLock.ReleaseReaderLock();
            return re;
        }
        catch (Exception)
        {
            _rwLock.ReleaseReaderLock();
            throw;
        }
    }

    public TR Write<TR>(Func<T, TR> action)
    {
        _rwLock.AcquireWriterLock(_timeout);
        try
        {
            var re = action(_target);
            _rwLock.ReleaseWriterLock();
            return re;
        }
        catch (Exception)
        {
            _rwLock.ReleaseWriterLock();
            throw;
        }
    }
}
public class ReadWriteLock<T1, T2>
{
    private readonly T1 _target1;
    private readonly T2 _target2;
    private readonly ReaderWriterLock _rwLock;
    private readonly int _timeout;

    public ReadWriteLock(T1 target1, T2 target2, int timeout = 3000)
    {
        _target1 = target1;
        _target2 = target2;
        _rwLock = new ReaderWriterLock();
        _timeout = timeout;
    }
    
    public void Read(Action<T1, T2> action)
    {
        _rwLock.AcquireReaderLock(_timeout);
        try
        {
            action(_target1, _target2);
            _rwLock.ReleaseReaderLock();
        }
        catch (Exception)
        {
            _rwLock.ReleaseReaderLock();
            throw;
        }
    }

    public void Write(Action<T1, T2> action)
    {
        _rwLock.AcquireWriterLock(_timeout);
        try
        {
            action(_target1, _target2);
            _rwLock.ReleaseWriterLock();
        }
        catch (Exception)
        {
            _rwLock.ReleaseWriterLock();
            throw;
        }
    }
    public TR Read<TR>(Func<T1, T2, TR> action)
    {
        _rwLock.AcquireReaderLock(_timeout);
        try
        {
            var re = action(_target1, _target2);
            _rwLock.ReleaseReaderLock();
            return re;
        }
        catch (Exception)
        {
            _rwLock.ReleaseReaderLock();
            throw;
        }
    }

    public TR Write<TR>(Func<T1, T2, TR> action)
    {
        _rwLock.AcquireWriterLock(_timeout);
        try
        {
            var re = action(_target1, _target2);
            _rwLock.ReleaseWriterLock();
            return re;
        }
        catch (Exception)
        {
            _rwLock.ReleaseWriterLock();
            throw;
        }
    }
}
