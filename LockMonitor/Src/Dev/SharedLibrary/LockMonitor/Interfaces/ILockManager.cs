using System.Collections.Concurrent;
using LockMonitor.Enums;

namespace LockMonitor.Interfaces;

public interface ILockManager
{
    public Task<bool> AcquireLockAsync(long transactionId, int resourceId, LockMode lockMode);
    public Task ReleaseLockAsync(long transactionId, int resourceId, LockMode lockMode);

    public ConcurrentQueue<int> GetDeadlockDetectionList();
    public ConcurrentDictionary<int, ILockEntry> GetLockTable();
}