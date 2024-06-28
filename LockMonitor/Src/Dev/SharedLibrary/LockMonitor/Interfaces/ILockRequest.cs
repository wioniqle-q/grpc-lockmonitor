using LockMonitor.Enums;

namespace LockMonitor.Interfaces;

internal interface ILockRequest
{
    public long TransactionId { get; }
    public LockMode LockMode { get; }
    public DateTime EnqueuedTime { get; }
}