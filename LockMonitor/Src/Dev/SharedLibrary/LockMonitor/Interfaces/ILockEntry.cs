using LockMonitor.Enums;

namespace LockMonitor.Interfaces;

public interface ILockEntry
{
    public Task<bool> CanGrantLockAsync(LockMode lockMode);
    public Task GrantLockAsync(LockMode lockMode);
    public Task ReleaseLockAsync(LockMode lockMode);
    public bool WaitingQueueTimeout(TimeSpan timeout);
    public Task<bool> EnqueueLockRequestAsync(long transactionId, LockMode lockMode);
    public Task GrantWaitingLocksAsync();
}