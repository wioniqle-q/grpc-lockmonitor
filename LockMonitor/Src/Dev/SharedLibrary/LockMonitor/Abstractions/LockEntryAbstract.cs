using LockMonitor.Enums;
using LockMonitor.Interfaces;

namespace LockMonitor.Abstractions;

public abstract class LockEntryAbstract : ILockEntry
{
    public abstract Task<bool> CanGrantLockAsync(LockMode lockMode);
    public abstract Task GrantLockAsync(LockMode lockMode);
    public abstract Task ReleaseLockAsync(LockMode lockMode);
    public abstract bool WaitingQueueTimeout(TimeSpan timeout);
    public abstract Task<bool> EnqueueLockRequestAsync(long transactionId, LockMode lockMode);
    public abstract Task GrantWaitingLocksAsync();
}