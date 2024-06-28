using System.Collections.Concurrent;
using LockMonitor.Enums;
using LockMonitor.Interfaces;

namespace LockMonitor.Abstractions;

public abstract class LockManagerAbstract : ILockManager
{
    public abstract Task<bool> AcquireLockAsync(long transactionId, int resourceId, LockMode lockMode);
    public abstract Task ReleaseLockAsync(long transactionId, int resourceId, LockMode lockMode);
    public abstract ConcurrentQueue<int> GetDeadlockDetectionList();
    public abstract ConcurrentDictionary<int, ILockEntry> GetLockTable();
}